using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Nv.Types
{
    class NvIoctlNotImplementedException : Exception
    {
        public ServiceCtx Context { get; }
        public NvDeviceFile DeviceFile { get; }
        public NvIoctl Command { get; }

        public NvIoctlNotImplementedException(ServiceCtx context, NvDeviceFile deviceFile, NvIoctl command)
            : this(context, deviceFile, command, "The ioctl is not implemented.")
        { }

        public NvIoctlNotImplementedException(ServiceCtx context, NvDeviceFile deviceFile, NvIoctl command, string message)
            : base(message)
        {
            Context = context;
            DeviceFile = deviceFile;
            Command = command;
        }

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
            StringBuilder sb = new();

            sb.AppendLine($"Device File: {DeviceFile.GetType().Name}");
            sb.AppendLine();

            sb.AppendLine($"Ioctl (0x{Command.RawValue:x8})");
            sb.AppendLine($"\tNumber: 0x{Command.Number:x8}");
            sb.AppendLine($"\tType: 0x{Command.Type:x8}");
            sb.AppendLine($"\tSize: 0x{Command.Size:x8}");
            sb.AppendLine($"\tDirection: {Command.DirectionValue}");

            sb.AppendLine("Guest Stack Trace:");
            sb.AppendLine(Context.Thread.GetGuestStackTrace());

            return sb.ToString();
        }
    }
}
