using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types;
using System;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel
{
    internal class NvHostGpuDeviceFile : NvHostChannelDeviceFile
    {
        private KEvent _smExceptionBptIntReportEvent;
        private KEvent _smExceptionBptPauseReportEvent;
        private KEvent _errorNotifierEvent;

        public NvHostGpuDeviceFile(ServiceCtx context) : base(context)
        {
            _smExceptionBptIntReportEvent   = new KEvent(context.Device.System);
            _smExceptionBptPauseReportEvent = new KEvent(context.Device.System);
            _errorNotifierEvent             = new KEvent(context.Device.System);
        }

        public override NvInternalResult Ioctl2(NvIoctl command, Span<byte> arguments, Span<byte> inlineInBuffer)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvHostMagic)
            {
                switch (command.Number)
                {
                    case 0x1b:
                        result = CallIoctlMethod<SubmitGpfifoArguments, long>(SubmitGpfifoEx, arguments, inlineInBuffer);
                        break;
                }
            }

            return result;
        }

        public override NvInternalResult QueryEvent(out int eventHandle, uint eventId)
        {
            // TODO: accurately represent and implement those events.
            KEvent targetEvent = null;

            switch (eventId)
            {
                case 0x1:
                    targetEvent = _smExceptionBptIntReportEvent;
                    break;
                case 0x2:
                    targetEvent = _smExceptionBptPauseReportEvent;
                    break;
                case 0x3:
                    targetEvent = _errorNotifierEvent;
                    break;
            }

            if (targetEvent != null)
            {
                if (Owner.HandleTable.GenerateHandle(targetEvent.ReadableEvent, out eventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }
            else
            {
                eventHandle = 0;

                return NvInternalResult.InvalidInput;
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult SubmitGpfifoEx(ref SubmitGpfifoArguments arguments, Span<long> inlineData)
        {
            return SubmitGpfifo(ref arguments, inlineData);
        }
    }
}
