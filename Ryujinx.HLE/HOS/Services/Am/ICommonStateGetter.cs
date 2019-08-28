using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ICommonStateGetter : IpcService
    {
        private KEvent _displayResolutionChangeEvent;

        private Apm.CpuBoostMode _cpuBoostMode = Apm.CpuBoostMode.Disabled;

        public ICommonStateGetter(Horizon system)
        {
            _displayResolutionChangeEvent = new KEvent(system);
        }

        [Command(0)]
        // GetEventHandle() -> handle<copy>
        public ResultCode GetEventHandle(ServiceCtx context)
        {
            KEvent Event = context.Device.System.AppletState.MessageEvent;

            if (context.Process.HandleTable.GenerateHandle(Event.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return ResultCode.Success;
        }

        [Command(1)]
        // ReceiveMessage() -> nn::am::AppletMessage
        public ResultCode ReceiveMessage(ServiceCtx context)
        {
            if (!context.Device.System.AppletState.TryDequeueMessage(out MessageInfo message))
            {
                return ResultCode.NoMessages;
            }

            context.ResponseData.Write((int)message);

            return ResultCode.Success;
        }

        [Command(5)]
        // GetOperationMode() -> u8
        public ResultCode GetOperationMode(ServiceCtx context)
        {
            OperationMode mode = context.Device.System.State.DockedMode
                ? OperationMode.Docked
                : OperationMode.Handheld;

            context.ResponseData.Write((byte)mode);

            return ResultCode.Success;
        }

        [Command(6)]
        // GetPerformanceMode() -> u32
        public ResultCode GetPerformanceMode(ServiceCtx context)
        {
            Apm.PerformanceMode mode = context.Device.System.State.DockedMode
                ? Apm.PerformanceMode.Docked
                : Apm.PerformanceMode.Handheld;

            context.ResponseData.Write((int)mode);

            return ResultCode.Success;
        }

        [Command(8)]
        // GetBootMode() -> u8
        public ResultCode GetBootMode(ServiceCtx context)
        {
            context.ResponseData.Write((byte)0); //Unknown value.

            Logger.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(9)]
        // GetCurrentFocusState() -> u8
        public ResultCode GetCurrentFocusState(ServiceCtx context)
        {
            context.ResponseData.Write((byte)context.Device.System.AppletState.FocusState);

            return ResultCode.Success;
        }

        [Command(60)] // 3.0.0+
        // GetDefaultDisplayResolution() -> (u32, u32)
        public ResultCode GetDefaultDisplayResolution(ServiceCtx context)
        {
            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);

            return ResultCode.Success;
        }

        [Command(61)] // 3.0.0+
        // GetDefaultDisplayResolutionChangeEvent() -> handle<copy>
        public ResultCode GetDefaultDisplayResolutionChangeEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_displayResolutionChangeEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(66)] // 6.0.0+
        // SetCpuBoostMode(u32 cpu_boost_mode)
        public ResultCode SetCpuBoostMode(ServiceCtx context)
        {
            uint cpuBoostMode = context.RequestData.ReadUInt32();

            if (cpuBoostMode > 1)
            {
                return ResultCode.CpuBoostModeInvalid;
            }

            _cpuBoostMode = (Apm.CpuBoostMode)cpuBoostMode;

            // NOTE: There is a condition variable after the assignment, probably waiting something with apm:sys service (SetCpuBoostMode call?).
            //       Since we will probably never support CPU boost things, it's not needed to implement more.

            return ResultCode.Success;
        }
    }
}