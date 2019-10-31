using ARMeilleure.Memory;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    static class IpcHandler
    {
        public static KernelResult IpcCall(
            Switch         device,
            KProcess       process,
            MemoryManager  memory,
            KThread        thread,
            KClientSession session,
            IpcMessage     request,
            long           cmdPtr)
        {
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
                            device,
                            process,
                            memory,
                            thread,
                            session,
                            request,
                            response,
                            reqReader,
                            resWriter);

                        session.Service.CallMethod(context);

                        response.RawData = resMs.ToArray();
                    }
                }
                else if (request.Type == IpcMessageType.Control ||
                         request.Type == IpcMessageType.ControlWithContext)
                {
                    long magic = reqReader.ReadInt64();
                    long cmdId = reqReader.ReadInt64();

                    switch (cmdId)
                    {
                        case 0:
                        {
                            request = FillResponse(response, 0, session.Service.ConvertToDomain());

                            break;
                        }

                        case 3:
                        {
                            request = FillResponse(response, 0, 0x1000);

                            break;
                        }

                        // TODO: Whats the difference between IpcDuplicateSession/Ex?
                        case 2:
                        case 4:
                        {
                            int unknown = reqReader.ReadInt32();

                            if (process.HandleTable.GenerateHandle(session, out int handle) != KernelResult.Success)
                            {
                                throw new InvalidOperationException("Out of handles!");
                            }

                            response.HandleDesc = IpcHandleDesc.MakeMove(handle);

                            request = FillResponse(response, 0);

                            break;
                        }

                        default: throw new NotImplementedException(cmdId.ToString());
                    }
                }
                else if (request.Type == IpcMessageType.CloseSession)
                {
                    // TODO
                    return KernelResult.PortRemoteClosed;
                }
                else
                {
                    throw new NotImplementedException(request.Type.ToString());
                }

                memory.WriteBytes(cmdPtr, response.GetBytes(cmdPtr));
            }

            return KernelResult.Success;
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
