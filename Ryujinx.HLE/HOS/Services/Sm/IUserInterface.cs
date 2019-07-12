using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
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
    [Service("sm:")]
    class IUserInterface : IpcService
    {
        private Dictionary<string, Type> _services;

        private ConcurrentDictionary<string, KPort> _registeredServices;

        private bool _isInitialized;

        public IUserInterface(ServiceCtx context = null)
        {
            _registeredServices = new ConcurrentDictionary<string, KPort>();

            _services = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(type => type.GetCustomAttributes(typeof(ServiceAttribute), true)
                .Select(service => (((ServiceAttribute)service).Name, type)))
                .ToDictionary(service => service.Name, service => service.type);
        }

        public static void InitializePort(Horizon system)
        {
            KPort port = new KPort(system, 256, false, 0);

            port.ClientPort.SetName("sm:");

            port.ClientPort.Service = new IUserInterface();
        }

        [Command(0)]
        // Initialize(pid, u64 reserved)
        public long Initialize(ServiceCtx context)
        {
            _isInitialized = true;

            return 0;
        }

        [Command(1)]
        // GetService(ServiceName name) -> handle<move, session>
        public long GetService(ServiceCtx context)
        {
            if (!_isInitialized)
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.NotInitialized);
            }

            string name = ReadName(context);

            if (name == string.Empty)
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.InvalidName);
            }

            KSession session = new KSession(context.Device.System);

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

                    session.ClientSession.Service = serviceAttribute.Parameter != null ? (IpcService)Activator.CreateInstance(type, context, serviceAttribute.Parameter)
                                                                                       : (IpcService)Activator.CreateInstance(type, context);
                }
                else
                {
                    if (ServiceConfiguration.IgnoreMissingServices)
                    {
                        Logger.PrintWarning(LogClass.Service, $"Missing service {name} ignored");

                        session.ClientSession.Service = new DummyService(name);
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

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);

            return 0;
        }

        [Command(2)]
        // RegisterService(ServiceName name, u8, u32 maxHandles) -> handle<move, port>
        public long RegisterService(ServiceCtx context)
        {
            if (!_isInitialized)
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.NotInitialized);
            }

            long namePosition = context.RequestData.BaseStream.Position;

            string name = ReadName(context);

            context.RequestData.BaseStream.Seek(namePosition + 8, SeekOrigin.Begin);

            bool isLight = (context.RequestData.ReadInt32() & 1) != 0;

            int maxSessions = context.RequestData.ReadInt32();

            if (name == string.Empty)
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.InvalidName);
            }

            Logger.PrintInfo(LogClass.ServiceSm, $"Register \"{name}\".");

            KPort port = new KPort(context.Device.System, maxSessions, isLight, 0);

            if (!_registeredServices.TryAdd(name, port))
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.AlreadyRegistered);
            }

            if (context.Process.HandleTable.GenerateHandle(port.ServerPort, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);

            return 0;
        }

        [Command(3)]
        // UnregisterService(ServiceName name)
        public long UnregisterService(ServiceCtx context)
        {
            if (!_isInitialized)
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.NotInitialized);
            }

            long namePosition = context.RequestData.BaseStream.Position;

            string name = ReadName(context);

            context.RequestData.BaseStream.Seek(namePosition + 8, SeekOrigin.Begin);

            bool isLight = (context.RequestData.ReadInt32() & 1) != 0;

            int maxSessions = context.RequestData.ReadInt32();

            if (name == string.Empty)
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.InvalidName);
            }

            if (!_registeredServices.TryRemove(name, out _))
            {
                return ErrorCode.MakeError(ErrorModule.Sm, SmErr.NotRegistered);
            }

            return 0;
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