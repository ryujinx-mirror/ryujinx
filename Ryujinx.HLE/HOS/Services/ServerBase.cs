using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.IO;

namespace Ryujinx.HLE.HOS.Services
{
    class ServerBase
    {
        private struct IpcRequest
        {
            public Switch Device { get; }
            public KProcess Process => Thread?.Owner;
            public KThread Thread { get; }
            public KClientSession Session { get; }
            public ulong MessagePtr { get; }
            public ulong MessageSize { get; }

            public IpcRequest(Switch device, KThread thread, KClientSession session, ulong messagePtr, ulong messageSize)
            {
                Device = device;
                Thread = thread;
                Session = session;
                MessagePtr = messagePtr;
                MessageSize = messageSize;
            }

            public void SignalDone(KernelResult result)
            {
                Thread.ObjSyncResult = result;
                Thread.Reschedule(ThreadSchedState.Running);
            }
        }

        private readonly AsyncWorkQueue<IpcRequest> _ipcProcessor;

        public ServerBase(string name)
        {
            _ipcProcessor = new AsyncWorkQueue<IpcRequest>(Process, name);
        }

        public void PushMessage(Switch device, KThread thread, KClientSession session, ulong messagePtr, ulong messageSize)
        {
            _ipcProcessor.Add(new IpcRequest(device, thread, session, messagePtr, messageSize));
        }

        private void Process(IpcRequest message)
        {
            byte[] reqData = new byte[message.MessageSize];

            message.Process.CpuMemory.Read(message.MessagePtr, reqData);

            IpcMessage request = new IpcMessage(reqData, (long)message.MessagePtr);
            IpcMessage response = new IpcMessage();

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
                            message.Device,
                            message.Process,
                            message.Process.CpuMemory,
                            message.Thread,
                            message.Session,
                            request,
                            response,
                            reqReader,
                            resWriter);

                        message.Session.Service.CallMethod(context);

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
                            request = FillResponse(response, 0, message.Session.Service.ConvertToDomain());
                            break;

                        case 3:
                            request = FillResponse(response, 0, 0x1000);
                            break;

                        // TODO: Whats the difference between IpcDuplicateSession/Ex?
                        case 2:
                        case 4:
                            int unknown = reqReader.ReadInt32();

                            if (message.Process.HandleTable.GenerateHandle(message.Session, out int handle) != KernelResult.Success)
                            {
                                throw new InvalidOperationException("Out of handles!");
                            }

                            response.HandleDesc = IpcHandleDesc.MakeMove(handle);

                            request = FillResponse(response, 0);

                            break;

                        default: throw new NotImplementedException(cmdId.ToString());
                    }
                }
                else if (request.Type == IpcMessageType.CloseSession)
                {
                    message.SignalDone(KernelResult.PortRemoteClosed);
                    return;
                }
                else
                {
                    throw new NotImplementedException(request.Type.ToString());
                }

                message.Process.CpuMemory.Write(message.MessagePtr, response.GetBytes((long)message.MessagePtr));
            }

            message.SignalDone(KernelResult.Success);
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