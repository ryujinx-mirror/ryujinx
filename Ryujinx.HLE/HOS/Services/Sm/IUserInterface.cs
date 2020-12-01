using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ryujinx.HLE.HOS.Services.Sm
{
    class IUserInterface : IpcService
    {
        private Dictionary<string, Type> _services;

        private readonly ConcurrentDictionary<string, KPort> _registeredServices;

        private readonly ServerBase _commonServer;

        private bool _isInitialized;

        public IUserInterface(KernelContext context)
        {
            _registeredServices = new ConcurrentDictionary<string, KPort>();

            _services = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(type => type.GetCustomAttributes(typeof(ServiceAttribute), true)
                .Select(service => (((ServiceAttribute)service).Name, type)))
                .ToDictionary(service => service.Name, service => service.type);

            TrySetServer(new ServerBase(context, "SmServer") { SmObject = this });

            _commonServer = new ServerBase(context, "CommonServer");
        }

        [Command(0)]
        // Initialize(pid, u64 reserved)
        public ResultCode Initialize(ServiceCtx context)
        {
            _isInitialized = true;

            return ResultCode.Success;
        }

        [Command(1)]
        // GetService(ServiceName name) -> handle<move, session>
        public ResultCode GetService(ServiceCtx context)
        {
            if (!_isInitialized)
            {
                return ResultCode.NotInitialized;
            }

            string name = ReadName(context);

            if (name == string.Empty)
            {
                return ResultCode.InvalidName;
            }

            KSession session = new KSession(context.Device.System.KernelContext);

            if (_registeredServices.TryGetValue(name, out KPort port))
            {
                KernelResult result = port.EnqueueIncomingSession(session.ServerSession);

                if (result != KernelResult.Success)
                {
                    throw new InvalidOperationException($"Session enqueue on port returned error \"{result}\".");
                }
            }
            else
            {
                if (_services.TryGetValue(name, out Type type))
                {
                    ServiceAttribute serviceAttribute = (ServiceAttribute)type.GetCustomAttributes(typeof(ServiceAttribute)).First(service => ((ServiceAttribute)service).Name == name);

                    IpcService service = serviceAttribute.Parameter != null
                        ? (IpcService)Activator.CreateInstance(type, context, serviceAttribute.Parameter)
                        : (IpcService)Activator.CreateInstance(type, context);

                    service.TrySetServer(_commonServer);
                    service.Server.AddSessionObj(session.ServerSession, service);
                }
                else
                {
                    if (ServiceConfiguration.IgnoreMissingServices)
                    {
                        Logger.Warning?.Print(LogClass.Service, $"Missing service {name} ignored");
                    }
                    else
                    {
                        throw new NotImplementedException(name);
                    }
                }
            }

            if (context.Process.HandleTable.GenerateHandle(session.ClientSession, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            session.ServerSession.DecrementReferenceCount();
            session.ClientSession.DecrementReferenceCount();

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);

            return ResultCode.Success;
        }

        [Command(2)]
        // RegisterService(ServiceName name, u8, u32 maxHandles) -> handle<move, port>
        public ResultCode RegisterService(ServiceCtx context)
        {
            if (!_isInitialized)
            {
                return ResultCode.NotInitialized;
            }

            long namePosition = context.RequestData.BaseStream.Position;

            string name = ReadName(context);

            context.RequestData.BaseStream.Seek(namePosition + 8, SeekOrigin.Begin);

            bool isLight = (context.RequestData.ReadInt32() & 1) != 0;

            int maxSessions = context.RequestData.ReadInt32();

            if (string.IsNullOrEmpty(name))
            {
                return ResultCode.InvalidName;
            }

            Logger.Info?.Print(LogClass.ServiceSm, $"Register \"{name}\".");

            KPort port = new KPort(context.Device.System.KernelContext, maxSessions, isLight, 0);

            if (!_registeredServices.TryAdd(name, port))
            {
                return ResultCode.AlreadyRegistered;
            }

            if (context.Process.HandleTable.GenerateHandle(port.ServerPort, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);

            return ResultCode.Success;
        }

        [Command(3)]
        // UnregisterService(ServiceName name)
        public ResultCode UnregisterService(ServiceCtx context)
        {
            if (!_isInitialized)
            {
                return ResultCode.NotInitialized;
            }

            long namePosition = context.RequestData.BaseStream.Position;

            string name = ReadName(context);

            context.RequestData.BaseStream.Seek(namePosition + 8, SeekOrigin.Begin);

            bool isLight = (context.RequestData.ReadInt32() & 1) != 0;

            int maxSessions = context.RequestData.ReadInt32();

            if (string.IsNullOrEmpty(name))
            {
                return ResultCode.InvalidName;
            }

            if (!_registeredServices.TryRemove(name, out _))
            {
                return ResultCode.NotRegistered;
            }

            return ResultCode.Success;
        }

        private static string ReadName(ServiceCtx context)
        {
            string name = string.Empty;

            for (int index = 0; index < 8 &&
                context.RequestData.BaseStream.Position <
                context.RequestData.BaseStream.Length; index++)
            {
                byte chr = context.RequestData.ReadByte();

                if (chr >= 0x20 && chr < 0x7f)
                {
                    name += (char)chr;
                }
            }

            return name;
        }
    }
}