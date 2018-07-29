using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.OsHle.Services
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

                int DomainCmd       = (DomainWord0 >> 0)  & 0xff;
                int InputObjCount   = (DomainWord0 >> 8)  & 0xff;
                int DataPayloadSize = (DomainWord0 >> 16) & 0xffff;

                Context.RequestData.BaseStream.Seek(0x10 + DataPayloadSize, SeekOrigin.Begin);

                for (int Index = 0; Index < InputObjCount; Index++)
                {
                    Context.Request.ObjectIds.Add(Context.RequestData.ReadInt32());
                }

                Context.RequestData.BaseStream.Seek(0x10, SeekOrigin.Begin);

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

                Context.Ns.Log.PrintDebug(LogClass.KernelIpc, $"{Service.GetType().Name}: {ProcessRequest.Method.Name}");

                long Result = ProcessRequest(Context);

                if (IsDomain)
                {
                    foreach (int Id in Context.Response.ObjectIds)
                    {
                        Context.ResponseData.Write(Id);
                    }

                    Context.ResponseData.BaseStream.Seek(0, SeekOrigin.Begin);

                    Context.ResponseData.Write(Context.Response.ObjectIds.Count);
                }

                Context.ResponseData.BaseStream.Seek(IsDomain ? 0x10 : 0, SeekOrigin.Begin);

                Context.ResponseData.Write(IpcMagic.Sfco);
                Context.ResponseData.Write(Result);
            }
            else
            {
                string DbgMessage = $"{Context.Session.ServiceName} {Service.GetType().Name}: {CommandId}";

                throw new NotImplementedException(DbgMessage);
            }
        }

        protected static void MakeObject(ServiceCtx Context, IpcService Obj)
        {
            IpcService Service = Context.Session.Service;

            if (Service.IsDomain)
            {
                Context.Response.ObjectIds.Add(Service.Add(Obj));
            }
            else
            {
                KSession Session = new KSession(Obj, Context.Session.ServiceName);

                int Handle = Context.Process.HandleTable.OpenHandle(Session);

                Context.Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);
            }
        }

        protected static T GetObject<T>(ServiceCtx Context, int Index) where T : IpcService
        {
            IpcService Service = Context.Session.Service;

            if (!Service.IsDomain)
            {
                int Handle = Context.Request.HandleDesc.ToMove[Index];

                KSession Session = Context.Process.HandleTable.GetData<KSession>(Handle);

                return Session?.Service is T ? (T)Session.Service : null;
            }

            int ObjId = Context.Request.ObjectIds[Index];

            IIpcService Obj = Service.GetObject(ObjId);

            return Obj is T ? (T)Obj : null;
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