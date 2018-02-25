using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.IpcServices;
using System;
using System.IO;

namespace Ryujinx.Core.OsHle.Ipc
{
    static class IpcHandler
    {
        private const long SfciMagic = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'I' << 24;
        private const long SfcoMagic = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'O' << 24;

        public static void IpcCall(
            Switch     Ns,
            AMemory    Memory,
            HSession   Session,
            IpcMessage Request,
            int        ThreadId,
            long       CmdPtr,
            int        HndId)
        {
            IpcMessage Response = new IpcMessage(Request.IsDomain && Request.Type == IpcMessageType.Request);

            using (MemoryStream Raw = new MemoryStream(Request.RawData))
            {
                BinaryReader ReqReader = new BinaryReader(Raw);

                if (Request.Type == IpcMessageType.Request)
                {
                    string ServiceName = Session.Service.GetType().Name;

                    ServiceProcessRequest ProcReq = null;

                    bool IgnoreNullPR = false;

                    string DbgServiceName = string.Empty;

                    if (Session is HDomain Dom)
                    {
                        if (Request.DomCmd == IpcDomCmd.SendMsg)
                        {
                            long Magic =      ReqReader.ReadInt64();
                            int  CmdId = (int)ReqReader.ReadInt64();

                            object Obj = Dom.GetObject(Request.DomObjId);

                            if (Obj is HDomain)
                            {
                                Session.Service.Commands.TryGetValue(CmdId, out ProcReq);

                                DbgServiceName = $"{ProcReq?.Method.Name ?? CmdId.ToString()}";
                            }
                            else if (Obj != null)
                            {
                                ((IIpcService)Obj).Commands.TryGetValue(CmdId, out ProcReq);

                                DbgServiceName = $"{Obj.GetType().Name} {ProcReq?.Method.Name ?? CmdId.ToString()}";
                            }
                        }
                        else if (Request.DomCmd == IpcDomCmd.DeleteObj)
                        {
                            Dom.DeleteObject(Request.DomObjId);

                            Response = FillResponse(Response, 0);

                            IgnoreNullPR = true;
                        }
                    }
                    else
                    {
                        long Magic =      ReqReader.ReadInt64();
                        int  CmdId = (int)ReqReader.ReadInt64();

                        if (Session is HSessionObj)
                        {
                            object Obj = ((HSessionObj)Session).Obj;

                            ((IIpcService)Obj).Commands.TryGetValue(CmdId, out ProcReq);

                            DbgServiceName = $"{Obj.GetType().Name} {ProcReq?.Method.Name ?? CmdId.ToString()}";
                        }
                        else
                        {
                            Session.Service.Commands.TryGetValue(CmdId, out ProcReq);

                            DbgServiceName = $"{ProcReq?.Method.Name ?? CmdId.ToString()}";
                        }
                    }

                    DbgServiceName = $"Tid {ThreadId} {ServiceName} {DbgServiceName}";

                    Logging.Debug($"IpcMessage: {DbgServiceName}");

                    if (ProcReq != null)
                    {
                        using (MemoryStream ResMS = new MemoryStream())
                        {
                            BinaryWriter ResWriter = new BinaryWriter(ResMS);

                            ServiceCtx Context = new ServiceCtx(
                                Ns,
                                Memory,
                                Session,
                                Request,
                                Response,
                                ReqReader,
                                ResWriter);

                            long Result = ProcReq(Context);

                            Response = FillResponse(Response, Result, ResMS.ToArray());
                        }
                    }
                    else if (!IgnoreNullPR)
                    {   
                        throw new NotImplementedException(DbgServiceName);
                    }
                }
                else if (Request.Type == IpcMessageType.Control)
                {
                    long Magic = ReqReader.ReadInt64();
                    long CmdId = ReqReader.ReadInt64();

                    switch (CmdId)
                    {
                        case 0: Request = IpcConvertSessionToDomain(Ns, Session, Response, HndId); break;
                        case 3: Request = IpcQueryBufferPointerSize(Response);                     break;
                        case 2: //IpcDuplicateSession, differences is unknown. 
                        case 4: Request = IpcDuplicateSessionEx(Ns, Session, Response, ReqReader); break;

                        default: throw new NotImplementedException(CmdId.ToString());
                    }
                }
                else if (Request.Type == IpcMessageType.Unknown2)
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

        private static IpcMessage IpcConvertSessionToDomain(
            Switch     Ns,
            HSession   Session,
            IpcMessage Response,
            int        HndId)
        {
            HDomain Dom = new HDomain(Session);

            Ns.Os.Handles.ReplaceData(HndId, Dom);

            return FillResponse(Response, 0, Dom.GenerateObjectId(Dom));
        }

        private static IpcMessage IpcDuplicateSessionEx(
            Switch       Ns,
            HSession     Session,
            IpcMessage   Response,
            BinaryReader ReqReader)
        {
            int Unknown = ReqReader.ReadInt32();

            int Handle = Ns.Os.Handles.GenerateId(Session);

            Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

            return FillResponse(Response, 0);
        }

        private static IpcMessage IpcQueryBufferPointerSize(IpcMessage Response)
        {
            return FillResponse(Response, 0, 0x500);
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

                Writer.Write(SfcoMagic);
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
