using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class ICommonStateGetter : IpcService
    {
        private Apm.ManagerServer       _apmManagerServer;
        private Apm.SystemManagerServer _apmSystemManagerServer;
        private Lbl.LblControllerServer _lblControllerServer;

        private bool _vrModeEnabled;
        private bool _lcdBacklighOffEnabled;
        private int  _messageEventHandle;
        private int  _displayResolutionChangedEventHandle;

        public ICommonStateGetter(ServiceCtx context)
        {
            _apmManagerServer       = new Apm.ManagerServer(context);
            _apmSystemManagerServer = new Apm.SystemManagerServer(context);
            _lblControllerServer    = new Lbl.LblControllerServer(context);
        }

        [Command(0)]
        // GetEventHandle() -> handle<copy>
        public ResultCode GetEventHandle(ServiceCtx context)
        {
            KEvent messageEvent = context.Device.System.AppletState.MessageEvent;

            if (_messageEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(messageEvent.ReadableEvent, out _messageEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_messageEventHandle);

            return ResultCode.Success;
        }

        [Command(1)]
        // ReceiveMessage() -> nn::am::AppletMessage
        public ResultCode ReceiveMessage(ServiceCtx context)
        {
            if (!context.Device.System.AppletState.Messages.TryDequeue(out MessageInfo message))
            {
                return ResultCode.NoMessages;
            }

            KEvent messageEvent = context.Device.System.AppletState.MessageEvent;

            // NOTE: Service checks if current states are different than the stored ones.
            //       Since we don't support any states for now, it's fine to check if there is still messages available.

            if (context.Device.System.AppletState.Messages.IsEmpty)
            {
                messageEvent.ReadableEvent.Clear();
            }
            else
            {
                messageEvent.ReadableEvent.Signal();
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
        // GetPerformanceMode() -> nn::apm::PerformanceMode
        public ResultCode GetPerformanceMode(ServiceCtx context)
        {
            return (ResultCode)_apmManagerServer.GetPerformanceMode(context);
        }

        [Command(8)]
        // GetBootMode() -> u8
        public ResultCode GetBootMode(ServiceCtx context)
        {
            context.ResponseData.Write((byte)0); //Unknown value.

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(9)]
        // GetCurrentFocusState() -> u8
        public ResultCode GetCurrentFocusState(ServiceCtx context)
        {
            context.ResponseData.Write((byte)context.Device.System.AppletState.FocusState);

            return ResultCode.Success;
        }

        [Command(50)] // 3.0.0+
        // IsVrModeEnabled() -> b8
        public ResultCode IsVrModeEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(_vrModeEnabled);

            return ResultCode.Success;
        }

        [Command(51)] // 3.0.0+
        // SetVrModeEnabled(b8)
        public ResultCode SetVrModeEnabled(ServiceCtx context)
        {
            bool vrModeEnabled = context.RequestData.ReadBoolean();

            UpdateVrMode(vrModeEnabled);

            return ResultCode.Success;
        }

        [Command(52)] // 4.0.0+
        // SetLcdBacklighOffEnabled(b8)
        public ResultCode SetLcdBacklighOffEnabled(ServiceCtx context)
        {
            // NOTE: Service sets a private field here, maybe this field is used somewhere else to turned off the backlight.
            //       Since we don't support backlight, it's fine to do nothing.

            _lcdBacklighOffEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(53)] // 7.0.0+
        // BeginVrModeEx()
        public ResultCode BeginVrModeEx(ServiceCtx context)
        {
            UpdateVrMode(true);

            return ResultCode.Success;
        }

        [Command(54)] // 7.0.0+
        // EndVrModeEx()
        public ResultCode EndVrModeEx(ServiceCtx context)
        {
            UpdateVrMode(false);

            return ResultCode.Success;
        }

        private void UpdateVrMode(bool vrModeEnabled)
        {
            if (_vrModeEnabled == vrModeEnabled)
            {
                return;
            }

            _vrModeEnabled = vrModeEnabled;

            if (vrModeEnabled)
            {
                _lblControllerServer.EnableVrMode();
            }
            else
            {
                _lblControllerServer.DisableVrMode();
            }

            // TODO: It signals an internal event of ICommonStateGetter. We have to determine where this event is used.
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
            if (_displayResolutionChangedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.System.DisplayResolutionChangeEvent.ReadableEvent, out _displayResolutionChangedEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_displayResolutionChangedEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(66)] // 6.0.0+
        // SetCpuBoostMode(u32 cpu_boost_mode)
        public ResultCode SetCpuBoostMode(ServiceCtx context)
        {
            uint cpuBoostMode = context.RequestData.ReadUInt32();

            if (cpuBoostMode > 1)
            {
                return ResultCode.InvalidParameters;
            }

            _apmSystemManagerServer.SetCpuBoostMode((Apm.CpuBoostMode)cpuBoostMode);

            // TODO: It signals an internal event of ICommonStateGetter. We have to determine where this event is used.

            return ResultCode.Success;
        }

        [Command(91)] // 7.0.0+
        // GetCurrentPerformanceConfiguration() -> nn::apm::PerformanceConfiguration
        public ResultCode GetCurrentPerformanceConfiguration(ServiceCtx context)
        {
            return (ResultCode)_apmSystemManagerServer.GetCurrentPerformanceConfiguration(context);
        }
    }
}