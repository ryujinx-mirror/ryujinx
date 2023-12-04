using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types;
using Ryujinx.Horizon.Common;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel
{
    internal class NvHostGpuDeviceFile : NvHostChannelDeviceFile
    {
#pragma warning disable IDE0052 // Remove unread private member
        private readonly KEvent _smExceptionBptIntReportEvent;
        private readonly KEvent _smExceptionBptPauseReportEvent;
        private readonly KEvent _errorNotifierEvent;
#pragma warning restore IDE0052

        private int _smExceptionBptIntReportEventHandle;
        private int _smExceptionBptPauseReportEventHandle;
        private int _errorNotifierEventHandle;

        public NvHostGpuDeviceFile(ServiceCtx context, IVirtualMemoryManager memory, ulong owner) : base(context, memory, owner)
        {
            _smExceptionBptIntReportEvent = CreateEvent(context, out _smExceptionBptIntReportEventHandle);
            _smExceptionBptPauseReportEvent = CreateEvent(context, out _smExceptionBptPauseReportEventHandle);
            _errorNotifierEvent = CreateEvent(context, out _errorNotifierEventHandle);
        }

        private KEvent CreateEvent(ServiceCtx context, out int handle)
        {
            KEvent evnt = new(context.Device.System.KernelContext);

            if (context.Process.HandleTable.GenerateHandle(evnt.ReadableEvent, out handle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            return evnt;
        }

        public override NvInternalResult Ioctl2(NvIoctl command, Span<byte> arguments, Span<byte> inlineInBuffer)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvHostMagic)
            {
                switch (command.Number)
                {
                    case 0x1b:
                        result = CallIoctlMethod<SubmitGpfifoArguments, ulong>(SubmitGpfifoEx, arguments, inlineInBuffer);
                        break;
                }
            }

            return result;
        }

        public override NvInternalResult QueryEvent(out int eventHandle, uint eventId)
        {
            // TODO: accurately represent and implement those events.
            eventHandle = eventId switch
            {
                0x1 => _smExceptionBptIntReportEventHandle,
                0x2 => _smExceptionBptPauseReportEventHandle,
                0x3 => _errorNotifierEventHandle,
                _ => 0,
            };
            return eventHandle != 0 ? NvInternalResult.Success : NvInternalResult.InvalidInput;
        }

        private NvInternalResult SubmitGpfifoEx(ref SubmitGpfifoArguments arguments, Span<ulong> inlineData)
        {
            return SubmitGpfifo(ref arguments, inlineData);
        }

        public override void Close()
        {
            if (_smExceptionBptIntReportEventHandle != 0)
            {
                Context.Process.HandleTable.CloseHandle(_smExceptionBptIntReportEventHandle);
                _smExceptionBptIntReportEventHandle = 0;
            }

            if (_smExceptionBptPauseReportEventHandle != 0)
            {
                Context.Process.HandleTable.CloseHandle(_smExceptionBptPauseReportEventHandle);
                _smExceptionBptPauseReportEventHandle = 0;
            }

            if (_errorNotifierEventHandle != 0)
            {
                Context.Process.HandleTable.CloseHandle(_errorNotifierEventHandle);
                _errorNotifierEventHandle = 0;
            }

            base.Close();
        }
    }
}
