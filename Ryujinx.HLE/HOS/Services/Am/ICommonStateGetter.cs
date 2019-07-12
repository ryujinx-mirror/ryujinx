using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ICommonStateGetter : IpcService
    {
        private KEvent _displayResolutionChangeEvent;

        public ICommonStateGetter(Horizon system)
        {
            _displayResolutionChangeEvent = new KEvent(system);
        }

        [Command(0)]
        // GetEventHandle() -> handle<copy>
        public long GetEventHandle(ServiceCtx context)
        {
            KEvent Event = context.Device.System.AppletState.MessageEvent;

            if (context.Process.HandleTable.GenerateHandle(Event.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        [Command(1)]
        // ReceiveMessage() -> nn::am::AppletMessage
        public long ReceiveMessage(ServiceCtx context)
        {
            if (!context.Device.System.AppletState.TryDequeueMessage(out MessageInfo message))
            {
                return MakeError(ErrorModule.Am, AmErr.NoMessages);
            }

            context.ResponseData.Write((int)message);

            return 0;
        }

        [Command(5)]
        // GetOperationMode() -> u8
        public long GetOperationMode(ServiceCtx context)
        {
            OperationMode mode = context.Device.System.State.DockedMode
                ? OperationMode.Docked
                : OperationMode.Handheld;

            context.ResponseData.Write((byte)mode);

            return 0;
        }

        [Command(6)]
        // GetPerformanceMode() -> u32
        public long GetPerformanceMode(ServiceCtx context)
        {
            Apm.PerformanceMode mode = context.Device.System.State.DockedMode
                ? Apm.PerformanceMode.Docked
                : Apm.PerformanceMode.Handheld;

            context.ResponseData.Write((int)mode);

            return 0;
        }

        [Command(8)]
        // GetBootMode() -> u8
        public long GetBootMode(ServiceCtx context)
        {
            context.ResponseData.Write((byte)0); //Unknown value.

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(9)]
        // GetCurrentFocusState() -> u8
        public long GetCurrentFocusState(ServiceCtx context)
        {
            context.ResponseData.Write((byte)context.Device.System.AppletState.FocusState);

            return 0;
        }

        [Command(60)] // 3.0.0+
        // GetDefaultDisplayResolution() -> (u32, u32)
        public long GetDefaultDisplayResolution(ServiceCtx context)
        {
            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);

            return 0;
        }

        [Command(61)] // 3.0.0+
        // GetDefaultDisplayResolutionChangeEvent() -> handle<copy>
        public long GetDefaultDisplayResolutionChangeEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_displayResolutionChangeEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }
    }
}