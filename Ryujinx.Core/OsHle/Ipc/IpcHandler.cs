using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using System;
using System.IO;

namespace Ryujinx.Core.OsHle.Ipc
{
    static class IpcHandler
    {
        public static void IpcCall(
            Switch     Ns,
            Process    Process,
            AMemory    Memory,
            KSession   Session,
            IpcMessage Request,
            long       CmdPtr)
        {
            IpcMessage Response = new IpcMessage();

            using (MemoryStream Raw = new MemoryStream(Request.RawData))
            {
                BinaryReader ReqReader = new BinaryReader(Raw);

                if (Request.Type == IpcMessageType.Request)
                {
                    Response.Type = IpcMessageType.Response;

                    using (MemoryStream ResMS = new MemoryStream())
                    {
                        BinaryWriter ResWriter = new BinaryWriter(ResMS);

                        ServiceCtx Context = new ServiceCtx(
                            Ns,
                            Process,
                            Memory,
                            Session,
                            Request,
                            Response,
                            ReqReader,
                            ResWriter);

                        Session.Service.CallMethod(Context);

                        Response.RawData = ResMS.ToArray();
                    }
                }
                else if (Request.Type == IpcMessageType.Control)
                {
                    long Magic = ReqReader.ReadInt64();
                    long CmdId = ReqReader.ReadInt64();

                    switch (CmdId)
                    {
                        case 0:
                        {
                            Request = FillResponse(Response, 0, Session.Service.ConvertToDomain());

                            break;
                        }

                        case 3:
                        {
                            Request = FillResponse(Response, 0, 0x500);
                            
                            break;
                        }

                        //TODO: Whats the difference between IpcDuplicateSession/Ex? 
                        case 2: 
                        case 4:
                        {
                            int Unknown = ReqReader.ReadInt32();

                            int Handle = Process.HandleTable.OpenHandle(Session);

                            Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

                            Request = FillResponse(Response, 0);

                            break;
                        }

                        default: throw new NotImplementedException(CmdId.ToString());
                    }
                }
                else if (Request.Type == IpcMessageType.CloseSession)
                {
                    //TODO
                }
                else
                {
                    throw new NotImplementedException(Request.Type.ToString());
                }

                AMemoryHelper.WriteBytes(Memory, CmdPtr, Response.GetBytes(CmdPtr));
            }
        }

        private static IpcMessage FillResponse(IpcMessage Response, long Result, params int[] Values)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                foreach (int Value in Values)
                {
                    Writer.Write(Value);
                }

                return FillResponse(Response, Result, MS.ToArray());
            }
        }

        private static IpcMessage FillResponse(IpcMessage Response, long Result, byte[] Data = null)
        {
            Response.Type = IpcMessageType.Response;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write(IpcMagic.Sfco);
                Writer.Write(Result);

                if (Data != null)
                {
                    Writer.Write(Data);
                }

                Response.RawData = MS.ToArray();
            }

            return Response;
        }
    }
}
