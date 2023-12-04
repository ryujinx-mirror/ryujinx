using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Nv.Types
{
    class NvQueryEventNotImplementedException : Exception
    {
        public ServiceCtx Context { get; }
        public NvDeviceFile DeviceFile { get; }
        public uint EventId { get; }

        public NvQueryEventNotImplementedException(ServiceCtx context, NvDeviceFile deviceFile, uint eventId)
            : this(context, deviceFile, eventId, "This query event is not implemented.")
        { }

        public NvQueryEventNotImplementedException(ServiceCtx context, NvDeviceFile deviceFile, uint eventId, string message)
            : base(message)
        {
            Context = context;
            DeviceFile = deviceFile;
            EventId = eventId;
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

            sb.AppendLine($"Event ID: (0x{EventId:x8})");

            sb.AppendLine("Guest Stack Trace:");
            sb.AppendLine(Context.Thread.GetGuestStackTrace());

            return sb.ToString();
        }
    }
}
