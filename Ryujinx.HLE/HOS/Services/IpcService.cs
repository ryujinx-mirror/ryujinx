using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using Ryujinx.Profiler;
using System.Reflection;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services
{
    abstract class IpcService : IIpcService
    {
        public IReadOnlyDictionary<int, MethodInfo> Commands { get; }

        private IdDictionary _domainObjects;

        private int _selfId;

        private bool _isDomain;

        public IpcService()
        {
            Commands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type == GetType())
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .SelectMany(methodInfo => methodInfo.GetCustomAttributes(typeof(CommandAttribute))
                .Select(command => (((CommandAttribute)command).Id, methodInfo)))
                .ToDictionary(command => command.Id, command => command.methodInfo);

            _domainObjects = new IdDictionary();

            _selfId = -1;
        }

        public int ConvertToDomain()
        {
            if (_selfId == -1)
            {
                _selfId = _domainObjects.Add(this);
            }

            _isDomain = true;

            return _selfId;
        }

        public void ConvertToSession()
        {
            _isDomain = false;
        }

        public void CallMethod(ServiceCtx context)
        {
            IIpcService service = this;

            if (_isDomain)
            {
                int domainWord0 = context.RequestData.ReadInt32();
                int domainObjId = context.RequestData.ReadInt32();

                int domainCmd       = (domainWord0 >> 0)  & 0xff;
                int inputObjCount   = (domainWord0 >> 8)  & 0xff;
                int dataPayloadSize = (domainWord0 >> 16) & 0xffff;

                context.RequestData.BaseStream.Seek(0x10 + dataPayloadSize, SeekOrigin.Begin);

                for (int index = 0; index < inputObjCount; index++)
                {
                    context.Request.ObjectIds.Add(context.RequestData.ReadInt32());
                }

                context.RequestData.BaseStream.Seek(0x10, SeekOrigin.Begin);

                if (domainCmd == 1)
                {
                    service = GetObject(domainObjId);

                    context.ResponseData.Write(0L);
                    context.ResponseData.Write(0L);
                }
                else if (domainCmd == 2)
                {
                    Delete(domainObjId);

                    context.ResponseData.Write(0L);

                    return;
                }
                else
                {
                    throw new NotImplementedException($"Domain command: {domainCmd}");
                }
            }

            long sfciMagic =      context.RequestData.ReadInt64();
            int  commandId = (int)context.RequestData.ReadInt64();

            bool serviceExists = service.Commands.TryGetValue(commandId, out MethodInfo processRequest);

            if (ServiceConfiguration.IgnoreMissingServices || serviceExists)
            {
                ResultCode result = ResultCode.Success;

                context.ResponseData.BaseStream.Seek(_isDomain ? 0x20 : 0x10, SeekOrigin.Begin);

                if (serviceExists)
                {
                    Logger.PrintDebug(LogClass.KernelIpc, $"{service.GetType().Name}: {processRequest.Name}");

                    ProfileConfig profile = Profiles.ServiceCall;

                    profile.SessionGroup = service.GetType().Name;
                    profile.SessionItem  = processRequest.Name;

                    Profile.Begin(profile);

                    result = (ResultCode)processRequest.Invoke(service, new object[] { context });

                    Profile.End(profile);
                }
                else
                {
                    string serviceName;

                    DummyService dummyService = service as DummyService;

                    serviceName = (dummyService == null) ? service.GetType().FullName : dummyService.ServiceName;

                    Logger.PrintWarning(LogClass.KernelIpc, $"Missing service {serviceName}: {commandId} ignored");
                }

                if (_isDomain)
                {
                    foreach (int id in context.Response.ObjectIds)
                    {
                        context.ResponseData.Write(id);
                    }

                    context.ResponseData.BaseStream.Seek(0, SeekOrigin.Begin);

                    context.ResponseData.Write(context.Response.ObjectIds.Count);
                }

                context.ResponseData.BaseStream.Seek(_isDomain ? 0x10 : 0, SeekOrigin.Begin);

                context.ResponseData.Write(IpcMagic.Sfco);
                context.ResponseData.Write((long)result);
            }
            else
            {
                string dbgMessage = $"{service.GetType().FullName}: {commandId}";

                throw new ServiceNotImplementedException(context, dbgMessage);
            }
        }

        protected static void MakeObject(ServiceCtx context, IpcService obj)
        {
            IpcService service = context.Session.Service;

            if (service._isDomain)
            {
                context.Response.ObjectIds.Add(service.Add(obj));
            }
            else
            {
                KSession session = new KSession(context.Device.System);

                session.ClientSession.Service = obj;

                if (context.Process.HandleTable.GenerateHandle(session.ClientSession, out int handle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }

                context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);
            }
        }

        protected static T GetObject<T>(ServiceCtx context, int index) where T : IpcService
        {
            IpcService service = context.Session.Service;

            if (!service._isDomain)
            {
                int handle = context.Request.HandleDesc.ToMove[index];

                KClientSession session = context.Process.HandleTable.GetObject<KClientSession>(handle);

                return session?.Service is T ? (T)session.Service : null;
            }

            int objId = context.Request.ObjectIds[index];

            IIpcService obj = service.GetObject(objId);

            return obj is T ? (T)obj : null;
        }

        private int Add(IIpcService obj)
        {
            return _domainObjects.Add(obj);
        }

        private bool Delete(int id)
        {
            object obj = _domainObjects.Delete(id);

            if (obj is IDisposable disposableObj)
            {
                disposableObj.Dispose();
            }

            return obj != null;
        }

        private IIpcService GetObject(int id)
        {
            return _domainObjects.GetData<IIpcService>(id);
        }
    }
}
