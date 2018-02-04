using ChocolArm64.Memory;
using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;
using System.IO;

namespace Ryujinx.OsHle
{
    class ServiceCtx
    {
        public Switch       Ns         { get; private set; }
        public AMemory      Memory       { get; private set; }
        public HSession     Session      { get; private set; }
        public IpcMessage   Request      { get; private set; }
        public IpcMessage   Response     { get; private set; }
        public BinaryReader RequestData  { get; private set; }
        public BinaryWriter ResponseData { get; private set; }

        public ServiceCtx(
            Switch       Ns,
            AMemory      Memory,
            HSession     Session,
            IpcMessage   Request,
            IpcMessage   Response,
            BinaryReader RequestData,
            BinaryWriter ResponseData)
        {
            this.Ns           = Ns;
            this.Memory       = Memory;
            this.Session      = Session;
            this.Request      = Request;
            this.Response     = Response;
            this.RequestData  = RequestData;
            this.ResponseData = ResponseData;
        }

        public T GetObject<T>()
        {
            object Obj = null;
    
            if (Session is HSessionObj SessionObj)
            {
                Obj = SessionObj.Obj; 
            }
            if (Session is HDomain Dom)
            {
                Obj = Dom.GetObject(Request.DomObjId);
            }

            return Obj is T ? (T)Obj : default(T);
        }
    }
}