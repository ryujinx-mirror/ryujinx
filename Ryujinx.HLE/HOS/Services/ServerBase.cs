using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services
{
    class ServerBase
    {
        // Must be the maximum value used by services (highest one know is the one used by nvservices = 0x8000).
        // Having a size that is too low will cause failures as data copy will fail if the receiving buffer is
        // not large enough.
        private const int PointerBufferSize = 0x8000;

        private readonly static int[] DefaultCapabilities = new int[]
        {
            0x030363F7,
            0x1FFFFFCF,
            0x207FFFEF,
            0x47E0060F,
            0x0048BFFF,
            0x01007FFF
        };

        private readonly KernelContext _context;
        private KProcess _selfProcess;

        private readonly List<int> _sessionHandles = new List<int>();
        private readonly List<int> _portHandles = new List<int>();
        private readonly Dictionary<int, IpcService> _sessions = new Dictionary<int, IpcService>();
        private readonly Dictionary<int, IpcService> _ports = new Dictionary<int, IpcService>();

        public ManualResetEvent InitDone { get; }
        public IpcService SmObject { get; set; }
        public string Name { get; }

        public ServerBase(KernelContext context, string name)
        {
            InitDone = new ManualResetEvent(false);
            Name = name;
            _context = context;

            const ProcessCreationFlags flags =
                ProcessCreationFlags.EnableAslr |
                ProcessCreationFlags.AddressSpace64Bit |
                ProcessCreationFlags.Is64Bit |
                ProcessCreationFlags.PoolPartitionSystem;

            ProcessCreationInfo creationInfo = new ProcessCreationInfo("Service", 1, 0, 0x8000000, 1, flags, 0, 0);

            KernelStatic.StartInitialProcess(context, creationInfo, DefaultCapabilities, 44, ServerLoop);
        }

        private void AddPort(int serverPortHandle, IpcService obj)
        {
            _portHandles.Add(serverPortHandle);
            _ports.Add(serverPortHandle, obj);
        }

        public void AddSessionObj(KServerSession serverSession, IpcService obj)
        {
            _selfProcess.HandleTable.GenerateHandle(serverSession, out int serverSessionHandle);
            AddSessionObj(serverSessionHandle, obj);
        }

        public void AddSessionObj(int serverSessionHandle, IpcService obj)
        {
            _sessionHandles.Add(serverSessionHandle);
            _sessions.Add(serverSessionHandle, obj);
        }

        private void ServerLoop()
        {
            _selfProcess = KernelStatic.GetCurrentProcess();

            if (SmObject != null)
            {
                _context.Syscall.ManageNamedPort("sm:", 50, out int serverPortHandle);

                AddPort(serverPortHandle, SmObject);

                InitDone.Set();
            }
            else
            {
                InitDone.Dispose();
            }

            KThread thread = KernelStatic.GetCurrentThread();
            ulong messagePtr = thread.TlsAddress;
            _context.Syscall.SetHeapSize(0x200000, out ulong heapAddr);

            _selfProcess.CpuMemory.Write(messagePtr + 0x0, 0);
            _selfProcess.CpuMemory.Write(messagePtr + 0x4, 2 << 10);
            _selfProcess.CpuMemory.Write(messagePtr + 0x8, heapAddr | ((ulong)PointerBufferSize << 48));

            int replyTargetHandle = 0;

            while (true)
            {
                int[] portHandles = _portHandles.ToArray();
                int[] sessionHandles = _sessionHandles.ToArray();
                int[] handles = new int[portHandles.Length + sessionHandles.Length];

                portHandles.CopyTo(handles, 0);
                sessionHandles.CopyTo(handles, portHandles.Length);

                // We still need a timeout here to allow the service to pick up and listen new sessions...
                var rc = _context.Syscall.ReplyAndReceive(handles, replyTargetHandle, 1000000L, out int signaledIndex);

                thread.HandlePostSyscall();

                if (!thread.Context.Running)
                {
                    break;
                }

                replyTargetHandle = 0;

                if (rc == KernelResult.Success && signaledIndex >= portHandles.Length)
                {
                    // We got a IPC request, process it, pass to the appropriate service if needed.
                    int signaledHandle = handles[signaledIndex];

                    if (Process(signaledHandle, heapAddr))
                    {
                        replyTargetHandle = signaledHandle;
                    }
                }
                else
                {
                    if (rc == KernelResult.Success)
                    {
                        // We got a new connection, accept the session to allow servicing future requests.
                        if (_context.Syscall.AcceptSession(handles[signaledIndex], out int serverSessionHandle) == KernelResult.Success)
                        {
                            AddSessionObj(serverSessionHandle, _ports[handles[signaledIndex]]);
                        }
                    }

                    _selfProcess.CpuMemory.Write(messagePtr + 0x0, 0);
                    _selfProcess.CpuMemory.Write(messagePtr + 0x4, 2 << 10);
                    _selfProcess.CpuMemory.Write(messagePtr + 0x8, heapAddr | ((ulong)PointerBufferSize << 48));
                }
            }
        }

        private bool Process(int serverSessionHandle, ulong recvListAddr)
        {
            KProcess process = KernelStatic.GetCurrentProcess();
            KThread thread = KernelStatic.GetCurrentThread();
            ulong messagePtr = thread.TlsAddress;
            ulong messageSize = 0x100;

            byte[] reqData = new byte[messageSize];

            process.CpuMemory.Read(messagePtr, reqData);

            IpcMessage request = new IpcMessage(reqData, (long)messagePtr);
            IpcMessage response = new IpcMessage();

            ulong tempAddr = recvListAddr;
            int sizesOffset = request.RawData.Length - ((request.RecvListBuff.Count * 2 + 3) & ~3);

            bool noReceive = true;

            for (int i = 0; i < request.ReceiveBuff.Count; i++)
            {
                noReceive &= (request.ReceiveBuff[i].Position == 0);
            }

            if (noReceive)
            {
                for (int i = 0; i < request.RecvListBuff.Count; i++)
                {
                    int size = BinaryPrimitives.ReadInt16LittleEndian(request.RawData.AsSpan().Slice(sizesOffset + i * 2, 2));

                    response.PtrBuff.Add(new IpcPtrBuffDesc((long)tempAddr, i, size));

                    request.RecvListBuff[i] = new IpcRecvListBuffDesc((long)tempAddr, size);

                    tempAddr += (ulong)size;
                }
            }

            bool shouldReply = true;

            using (MemoryStream raw = new MemoryStream(request.RawData))
            {
                BinaryReader reqReader = new BinaryReader(raw);

                if (request.Type == IpcMessageType.Request ||
                    request.Type == IpcMessageType.RequestWithContext)
                {
                    response.Type = IpcMessageType.Response;

                    using (MemoryStream resMs = new MemoryStream())
                    {
                        BinaryWriter resWriter = new BinaryWriter(resMs);

                        ServiceCtx context = new ServiceCtx(
                            _context.Device,
                            process,
                            process.CpuMemory,
                            thread,
                            request,
                            response,
                            reqReader,
                            resWriter);

                        _sessions[serverSessionHandle].CallMethod(context);

                        response.RawData = resMs.ToArray();
                    }
                }
                else if (request.Type == IpcMessageType.Control ||
                         request.Type == IpcMessageType.ControlWithContext)
                {
                    uint magic = (uint)reqReader.ReadUInt64();
                    uint cmdId = (uint)reqReader.ReadUInt64();

                    switch (cmdId)
                    {
                        case 0:
                            request = FillResponse(response, 0, _sessions[serverSessionHandle].ConvertToDomain());
                            break;

                        case 3:
                            request = FillResponse(response, 0, PointerBufferSize);
                            break;

                        // TODO: Whats the difference between IpcDuplicateSession/Ex?
                        case 2:
                        case 4:
                            int unknown = reqReader.ReadInt32();

                            _context.Syscall.CreateSession(false, 0, out int dupServerSessionHandle, out int dupClientSessionHandle);

                            AddSessionObj(dupServerSessionHandle, _sessions[serverSessionHandle]);

                            response.HandleDesc = IpcHandleDesc.MakeMove(dupClientSessionHandle);

                            request = FillResponse(response, 0);

                            break;

                        default: throw new NotImplementedException(cmdId.ToString());
                    }
                }
                else if (request.Type == IpcMessageType.CloseSession)
                {
                    _context.Syscall.CloseHandle(serverSessionHandle);
                    _sessionHandles.Remove(serverSessionHandle);
                    IpcService service = _sessions[serverSessionHandle];
                    if (service is IDisposable disposableObj)
                    {
                        disposableObj.Dispose();
                    }
                    _sessions.Remove(serverSessionHandle);
                    shouldReply = false;
                }
                else
                {
                    throw new NotImplementedException(request.Type.ToString());
                }

                process.CpuMemory.Write(messagePtr, response.GetBytes((long)messagePtr, recvListAddr | ((ulong)PointerBufferSize << 48)));
                return shouldReply;
            }
        }

        private static IpcMessage FillResponse(IpcMessage response, long result, params int[] values)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                foreach (int value in values)
                {
                    writer.Write(value);
                }

                return FillResponse(response, result, ms.ToArray());
            }
        }

        private static IpcMessage FillResponse(IpcMessage response, long result, byte[] data = null)
        {
            response.Type = IpcMessageType.Response;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                writer.Write(IpcMagic.Sfco);
                writer.Write(result);

                if (data != null)
                {
                    writer.Write(data);
                }

                response.RawData = ms.ToArray();
            }

            return response;
        }
    }
}