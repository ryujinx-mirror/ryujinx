using Ryujinx.Common;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services
{
    class ServerBase : IDisposable
    {
        // Must be the maximum value used by services (highest one know is the one used by nvservices = 0x8000).
        // Having a size that is too low will cause failures as data copy will fail if the receiving buffer is
        // not large enough.
        private const int PointerBufferSize = 0x8000;

        private readonly static uint[] DefaultCapabilities = new uint[]
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
        private readonly Dictionary<int, Func<IpcService>> _ports = new Dictionary<int, Func<IpcService>>();

        private readonly MemoryStream _requestDataStream;
        private readonly BinaryReader _requestDataReader;

        private readonly MemoryStream _responseDataStream;
        private readonly BinaryWriter _responseDataWriter;

        public ManualResetEvent InitDone { get; }
        public string Name { get; }
        public Func<IpcService> SmObjectFactory { get; }

        public ServerBase(KernelContext context, string name, Func<IpcService> smObjectFactory = null)
        {
            _context = context;

            _requestDataStream = MemoryStreamManager.Shared.GetStream();
            _requestDataReader = new BinaryReader(_requestDataStream);

            _responseDataStream = MemoryStreamManager.Shared.GetStream();
            _responseDataWriter = new BinaryWriter(_responseDataStream);

            InitDone = new ManualResetEvent(false);
            Name = name;
            SmObjectFactory = smObjectFactory;

            const ProcessCreationFlags flags =
                ProcessCreationFlags.EnableAslr |
                ProcessCreationFlags.AddressSpace64Bit |
                ProcessCreationFlags.Is64Bit |
                ProcessCreationFlags.PoolPartitionSystem;

            ProcessCreationInfo creationInfo = new ProcessCreationInfo("Service", 1, 0, 0x8000000, 1, flags, 0, 0);

            KernelStatic.StartInitialProcess(context, creationInfo, DefaultCapabilities, 44, Main);
        }

        private void AddPort(int serverPortHandle, Func<IpcService> objectFactory)
        {
            _portHandles.Add(serverPortHandle);
            _ports.Add(serverPortHandle, objectFactory);
        }

        public void AddSessionObj(KServerSession serverSession, IpcService obj)
        {
            // Ensure that the sever loop is running.
            InitDone.WaitOne();

            _selfProcess.HandleTable.GenerateHandle(serverSession, out int serverSessionHandle);
            AddSessionObj(serverSessionHandle, obj);
        }

        public void AddSessionObj(int serverSessionHandle, IpcService obj)
        {
            _sessionHandles.Add(serverSessionHandle);
            _sessions.Add(serverSessionHandle, obj);
        }

        private void Main()
        {
            ServerLoop();
        }

        private void ServerLoop()
        {
            _selfProcess = KernelStatic.GetCurrentProcess();

            if (SmObjectFactory != null)
            {
                _context.Syscall.ManageNamedPort(out int serverPortHandle, "sm:", 50);

                AddPort(serverPortHandle, SmObjectFactory);
            }

            InitDone.Set();

            KThread thread = KernelStatic.GetCurrentThread();
            ulong messagePtr = thread.TlsAddress;
            _context.Syscall.SetHeapSize(out ulong heapAddr, 0x200000);

            _selfProcess.CpuMemory.Write(messagePtr + 0x0, 0);
            _selfProcess.CpuMemory.Write(messagePtr + 0x4, 2 << 10);
            _selfProcess.CpuMemory.Write(messagePtr + 0x8, heapAddr | ((ulong)PointerBufferSize << 48));

            int replyTargetHandle = 0;

            while (true)
            {
                int handleCount = _portHandles.Count + _sessionHandles.Count;

                int[] handles = ArrayPool<int>.Shared.Rent(handleCount);
                
                _portHandles.CopyTo(handles, 0);
                _sessionHandles.CopyTo(handles, _portHandles.Count);

                // We still need a timeout here to allow the service to pick up and listen new sessions...
                var rc = _context.Syscall.ReplyAndReceive(out int signaledIndex, handles.AsSpan(0, handleCount), replyTargetHandle, 1000000L);

                thread.HandlePostSyscall();

                if (!thread.Context.Running)
                {
                    break;
                }

                replyTargetHandle = 0;

                if (rc == Result.Success && signaledIndex >= _portHandles.Count)
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
                    if (rc == Result.Success)
                    {
                        // We got a new connection, accept the session to allow servicing future requests.
                        if (_context.Syscall.AcceptSession(out int serverSessionHandle, handles[signaledIndex]) == Result.Success)
                        {
                            IpcService obj = _ports[handles[signaledIndex]].Invoke();

                            AddSessionObj(serverSessionHandle, obj);
                        }
                    }

                    _selfProcess.CpuMemory.Write(messagePtr + 0x0, 0);
                    _selfProcess.CpuMemory.Write(messagePtr + 0x4, 2 << 10);
                    _selfProcess.CpuMemory.Write(messagePtr + 0x8, heapAddr | ((ulong)PointerBufferSize << 48));
                }

                ArrayPool<int>.Shared.Return(handles);
            }

            Dispose();
        }

        private bool Process(int serverSessionHandle, ulong recvListAddr)
        {
            KProcess process = KernelStatic.GetCurrentProcess();
            KThread thread = KernelStatic.GetCurrentThread();
            ulong messagePtr = thread.TlsAddress;

            IpcMessage request = ReadRequest(process, messagePtr);

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
                    ulong size = (ulong)BinaryPrimitives.ReadInt16LittleEndian(request.RawData.AsSpan(sizesOffset + i * 2, 2));

                    response.PtrBuff.Add(new IpcPtrBuffDesc(tempAddr, (uint)i, size));

                    request.RecvListBuff[i] = new IpcRecvListBuffDesc(tempAddr, size);

                    tempAddr += size;
                }
            }

            bool shouldReply = true;
            bool isTipcCommunication = false;

            _requestDataStream.SetLength(0);
            _requestDataStream.Write(request.RawData);
            _requestDataStream.Position = 0;

            if (request.Type == IpcMessageType.HipcRequest ||
                request.Type == IpcMessageType.HipcRequestWithContext)
            {
                response.Type = IpcMessageType.HipcResponse;

                _responseDataStream.SetLength(0);

                ServiceCtx context = new ServiceCtx(
                    _context.Device,
                    process,
                    process.CpuMemory,
                    thread,
                    request,
                    response,
                    _requestDataReader,
                    _responseDataWriter);

                _sessions[serverSessionHandle].CallHipcMethod(context);

                response.RawData = _responseDataStream.ToArray();
            }
            else if (request.Type == IpcMessageType.HipcControl ||
                        request.Type == IpcMessageType.HipcControlWithContext)
            {
                uint magic = (uint)_requestDataReader.ReadUInt64();
                uint cmdId = (uint)_requestDataReader.ReadUInt64();

                switch (cmdId)
                {
                    case 0:
                        FillHipcResponse(response, 0, _sessions[serverSessionHandle].ConvertToDomain());
                        break;

                    case 3:
                        FillHipcResponse(response, 0, PointerBufferSize);
                        break;

                    // TODO: Whats the difference between IpcDuplicateSession/Ex?
                    case 2:
                    case 4:
                        int unknown = _requestDataReader.ReadInt32();

                        _context.Syscall.CreateSession(out int dupServerSessionHandle, out int dupClientSessionHandle, false, 0);

                        AddSessionObj(dupServerSessionHandle, _sessions[serverSessionHandle]);

                        response.HandleDesc = IpcHandleDesc.MakeMove(dupClientSessionHandle);

                        FillHipcResponse(response, 0);

                        break;

                    default: throw new NotImplementedException(cmdId.ToString());
                }
            }
            else if (request.Type == IpcMessageType.HipcCloseSession || request.Type == IpcMessageType.TipcCloseSession)
            {
                _context.Syscall.CloseHandle(serverSessionHandle);
                _sessionHandles.Remove(serverSessionHandle);
                IpcService service = _sessions[serverSessionHandle];
                (service as IDisposable)?.Dispose();
                _sessions.Remove(serverSessionHandle);
                shouldReply = false;
            }
            // If the type is past 0xF, we are using TIPC
            else if (request.Type > IpcMessageType.TipcCloseSession)
            {
                isTipcCommunication = true;

                // Response type is always the same as request on TIPC.
                response.Type = request.Type;

                _responseDataStream.SetLength(0);

                ServiceCtx context = new ServiceCtx(
                    _context.Device,
                    process,
                    process.CpuMemory,
                    thread,
                    request,
                    response,
                    _requestDataReader,
                    _responseDataWriter);

                _sessions[serverSessionHandle].CallTipcMethod(context);

                response.RawData = _responseDataStream.ToArray();

                using var responseStream = response.GetStreamTipc();
                process.CpuMemory.Write(messagePtr, responseStream.GetReadOnlySequence());
            }
            else
            {
                throw new NotImplementedException(request.Type.ToString());
            }

            if (!isTipcCommunication)
            {
                using var responseStream = response.GetStream((long)messagePtr, recvListAddr | ((ulong)PointerBufferSize << 48));
                process.CpuMemory.Write(messagePtr, responseStream.GetReadOnlySequence());
            }

            return shouldReply;
        }

        private static IpcMessage ReadRequest(KProcess process, ulong messagePtr)
        {
            const int messageSize = 0x100;

            byte[] reqData = ArrayPool<byte>.Shared.Rent(messageSize);

            Span<byte> reqDataSpan = reqData.AsSpan(0, messageSize);
            reqDataSpan.Clear();

            process.CpuMemory.Read(messagePtr, reqDataSpan);

            IpcMessage request = new IpcMessage(reqDataSpan, (long)messagePtr);

            ArrayPool<byte>.Shared.Return(reqData);

            return request;
        }

        private void FillHipcResponse(IpcMessage response, long result)
        {
            FillHipcResponse(response, result, ReadOnlySpan<byte>.Empty);
        }

        private void FillHipcResponse(IpcMessage response, long result, int value)
        {
            Span<byte> span = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(span, value);
            FillHipcResponse(response, result, span);
        }

        private void FillHipcResponse(IpcMessage response, long result, ReadOnlySpan<byte> data)
        {
            response.Type = IpcMessageType.HipcResponse;

            _responseDataStream.SetLength(0);

            _responseDataStream.Write(IpcMagic.Sfco);
            _responseDataStream.Write(result);

            _responseDataStream.Write(data);

            response.RawData = _responseDataStream.ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IpcService service in _sessions.Values)
                {
                    if (service is IDisposable disposableObj)
                    {
                        disposableObj.Dispose();
                    }

                    service.DestroyAtExit();
                }

                _sessions.Clear();

                _requestDataReader.Dispose();
                _requestDataStream.Dispose();
                _responseDataWriter.Dispose();
                _responseDataStream.Dispose();

                InitDone.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}