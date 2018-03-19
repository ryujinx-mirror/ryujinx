using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Handles;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.OsHle.IpcServices
{
    abstract class IpcService : IIpcService
    {
        public abstract IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; }

        private IdDictionary DomainObjects;

        private int SelfId;

        private bool IsDomain;       

        public IpcService()
        {
            DomainObjects = new IdDictionary();

            SelfId = -1;
        }

        public int ConvertToDomain()
        {
            if (SelfId == -1)
            {
                SelfId = DomainObjects.Add(this);
            }

            IsDomain = true;

            return SelfId;
        }

        public void ConvertToSession()
        {
            IsDomain = false;
        }

        public void CallMethod(ServiceCtx Context)
        {
            IIpcService Service = this;

            if (IsDomain)
            {
                int DomainWord0 = Context.RequestData.ReadInt32();
                int DomainObjId = Context.RequestData.ReadInt32();

                long Padding = Context.RequestData.ReadInt64();

                int DomainCmd = DomainWord0 & 0xff;

                if (DomainCmd == 1)
                {
                    Service = GetObject(DomainObjId);

                    Context.ResponseData.Write(0L);
                    Context.ResponseData.Write(0L);
                }
                else if (DomainCmd == 2)
                {
                    Delete(DomainObjId);

                    Context.ResponseData.Write(0L);

                    return;
                }
                else
                {
                    throw new NotImplementedException($"Domain command: {DomainCmd}");
                }
            }

            long SfciMagic =      Context.RequestData.ReadInt64();
            int  CommandId = (int)Context.RequestData.ReadInt64();

            if (Service.Commands.TryGetValue(CommandId, out ServiceProcessRequest ProcessRequest))
            {
                Context.ResponseData.BaseStream.Seek(IsDomain ? 0x20 : 0x10, SeekOrigin.Begin);

                Logging.Trace($"{Service.GetType().Name}: {ProcessRequest.Method.Name}");

                long Result = ProcessRequest(Context);

                if (IsDomain)
                {
                    foreach (int Id in Context.Response.ResponseObjIds)
                    {
                        Context.ResponseData.Write(Id);
                    }

                    Context.ResponseData.BaseStream.Seek(0, SeekOrigin.Begin);

                    Context.ResponseData.Write(Context.Response.ResponseObjIds.Count);
                }

                Context.ResponseData.BaseStream.Seek(IsDomain ? 0x10 : 0, SeekOrigin.Begin);

                Context.ResponseData.Write(IpcMagic.Sfco);
                Context.ResponseData.Write(Result);
            }
            else
            {
                throw new NotImplementedException($"{Service.GetType().Name}: {CommandId}");
            }
        }

        protected static void MakeObject(ServiceCtx Context, IpcService Obj)
        {
            IpcService Service = Context.Session.Service;

            if (Service.IsDomain)
            {
                Context.Response.ResponseObjIds.Add(Service.Add(Obj));
            }
            else
            {
                KSession Session = new KSession(Obj);

                int Handle = Context.Process.HandleTable.OpenHandle(Session);

                Context.Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);
            }
        }

        private int Add(IIpcService Obj)
        {
            return DomainObjects.Add(Obj);
        }

        private bool Delete(int Id)
        {
            object Obj = DomainObjects.Delete(Id);

            if (Obj is IDisposable DisposableObj)
            {
                DisposableObj.Dispose();
            }

            return Obj != null;
        }

        private IIpcService GetObject(int Id)
        {
            return DomainObjects.GetData<IIpcService>(Id);
        }
    }
}