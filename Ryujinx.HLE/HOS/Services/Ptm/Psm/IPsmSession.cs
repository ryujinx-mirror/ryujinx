using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Ptm.Psm
{
    class IPsmSession : IpcService
    {
        private KEvent _stateChangeEvent;
        private int    _stateChangeEventHandle;

        public IPsmSession(Horizon system)
        {
            _stateChangeEvent       = new KEvent(system.KernelContext);
            _stateChangeEventHandle = -1;
        }

        [Command(0)]
        // BindStateChangeEvent() -> KObject
        public ResultCode BindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle == -1)
            {
                KernelResult resultCode = context.Process.HandleTable.GenerateHandle(_stateChangeEvent.ReadableEvent, out int stateChangeEventHandle);

                if (resultCode != KernelResult.Success)
                {
                    return (ResultCode)resultCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_stateChangeEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServicePsm);

            return ResultCode.Success;
        }

        [Command(1)]
        // UnbindStateChangeEvent()
        public ResultCode UnbindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle != -1)
            {
                context.Process.HandleTable.CloseHandle(_stateChangeEventHandle);
                _stateChangeEventHandle = -1;
            }

            Logger.Stub?.PrintStub(LogClass.ServicePsm);

            return ResultCode.Success;
        }

        [Command(2)]
        // SetChargerTypeChangeEventEnabled(u8)
        public ResultCode SetChargerTypeChangeEventEnabled(ServiceCtx context)
        {
            bool chargerTypeChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServicePsm, new { chargerTypeChangeEventEnabled });

            return ResultCode.Success;
        }

        [Command(3)]
        // SetPowerSupplyChangeEventEnabled(u8)
        public ResultCode SetPowerSupplyChangeEventEnabled(ServiceCtx context)
        {
            bool powerSupplyChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServicePsm, new { powerSupplyChangeEventEnabled });

            return ResultCode.Success;
        }

        [Command(4)]
        // SetBatteryVoltageStateChangeEventEnabled(u8)
        public ResultCode SetBatteryVoltageStateChangeEventEnabled(ServiceCtx context)
        {
            bool batteryVoltageStateChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServicePsm, new { batteryVoltageStateChangeEventEnabled });

            return ResultCode.Success;
        }
    }
}