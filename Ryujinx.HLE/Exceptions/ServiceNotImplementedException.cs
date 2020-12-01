using Ryujinx.Common;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Ryujinx.HLE.Exceptions
{
    [Serializable]
    internal class ServiceNotImplementedException : Exception
    {
        public IIpcService Service { get; }
        public ServiceCtx Context { get; }
        public IpcMessage Request { get; }

        public ServiceNotImplementedException(IIpcService service, ServiceCtx context)
            : this(service, context, "The service call is not implemented.")
        { }

        public ServiceNotImplementedException(IIpcService service, ServiceCtx context, string message)
            : base(message)
        {
            Service = service;
            Context = context;
            Request = context.Request;
        }

        public ServiceNotImplementedException(IIpcService service, ServiceCtx context, string message, Exception inner)
            : base(message, inner)
        {
            Service = service;
            Context = context;
            Request = context.Request;
        }

        protected ServiceNotImplementedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public override string Message
        {
            get
            {
                return base.Message +
                        Environment.NewLine +
                        Environment.NewLine +
                        BuildMessage();
            }
        }

        private string BuildMessage()
        {
            StringBuilder sb = new StringBuilder();

            // Print the IPC command details (service name, command ID, and handler)
            (Type callingType, MethodBase callingMethod) = WalkStackTrace(new StackTrace(this));

            if (callingType != null && callingMethod != null)
            {
                var ipcCommands = Service.Commands;

                // Find the handler for the method called
                var ipcHandler   = ipcCommands.FirstOrDefault(x => x.Value == callingMethod);
                var ipcCommandId = ipcHandler.Key;
                var ipcMethod    = ipcHandler.Value;

                if (ipcMethod != null)
                {
                    sb.AppendLine($"Service Command: {Service.GetType().FullName}: {ipcCommandId} ({ipcMethod.Name})");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("Guest Stack Trace:");
            sb.AppendLine(Context.Thread.GetGuestStackTrace());

            // Print buffer information
            if (Request.PtrBuff.Count > 0 ||
                Request.SendBuff.Count > 0 ||
                Request.ReceiveBuff.Count > 0 ||
                Request.ExchangeBuff.Count > 0 ||
                Request.RecvListBuff.Count > 0)
            {
                sb.AppendLine("Buffer Information:");

                if (Request.PtrBuff.Count > 0)
                {
                    sb.AppendLine("\tPtrBuff:");

                    foreach (var buff in Request.PtrBuff)
                    {
                        sb.AppendLine($"\t[{buff.Index}] Position: 0x{buff.Position:x16} Size: 0x{buff.Size:x16}");
                    }
                }

                if (Request.SendBuff.Count > 0)
                {
                    sb.AppendLine("\tSendBuff:");

                    foreach (var buff in Request.SendBuff)
                    {
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16} Flags: {buff.Flags}");
                    }
                }

                if (Request.ReceiveBuff.Count > 0)
                {
                    sb.AppendLine("\tReceiveBuff:");

                    foreach (var buff in Request.ReceiveBuff)
                    {
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16} Flags: {buff.Flags}");
                    }
                }

                if (Request.ExchangeBuff.Count > 0)
                {
                    sb.AppendLine("\tExchangeBuff:");

                    foreach (var buff in Request.ExchangeBuff)
                    {
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16} Flags: {buff.Flags}");
                    }
                }

                if (Request.RecvListBuff.Count > 0)
                {
                    sb.AppendLine("\tRecvListBuff:");

                    foreach (var buff in Request.RecvListBuff)
                    {
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16}");
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("Raw Request Data:");
            sb.Append(HexUtils.HexTable(Request.RawData));

            return sb.ToString();
        }

        private (Type, MethodBase) WalkStackTrace(StackTrace trace)
        {
            int i = 0;

            StackFrame frame;
            // Find the IIpcService method that threw this exception
            while ((frame = trace.GetFrame(i++)) != null)
            {
                var method   = frame.GetMethod();
                var declType = method.DeclaringType;

                if (typeof(IIpcService).IsAssignableFrom(declType))
                {
                    return (declType, method);
                }
            }

            return (null, null);
        }
    }
}
