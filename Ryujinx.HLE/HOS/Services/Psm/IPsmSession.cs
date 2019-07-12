using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Psm
{
    class IPsmSession : IpcService
    {
        private KEvent _stateChangeEvent;
        private int    _stateChangeEventHandle;

        public IPsmSession(Horizon system)
        {
            _stateChangeEvent       = new KEvent(system);
            _stateChangeEventHandle = -1;
        }

        [Command(0)]
        // BindStateChangeEvent() -> KObject
        public long BindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle == -1)
            {
                KernelResult resultCode = context.Process.HandleTable.GenerateHandle(_stateChangeEvent.ReadableEvent, out int stateChangeEventHandle);

                if (resultCode != KernelResult.Success)
                {
                    return (long)resultCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_stateChangeEventHandle);

            Logger.PrintStub(LogClass.ServicePsm);

            return 0;
        }

        [Command(1)]
        // UnbindStateChangeEvent()
        public long UnbindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle != -1)
            {
                context.Process.HandleTable.CloseHandle(_stateChangeEventHandle);
                _stateChangeEventHandle = -1;
            }

            Logger.PrintStub(LogClass.ServicePsm);

            return 0;
        }

        [Command(2)]
        // SetChargerTypeChangeEventEnabled(u8)
        public long SetChargerTypeChangeEventEnabled(ServiceCtx context)
        {
            bool chargerTypeChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, new { chargerTypeChangeEventEnabled });

            return 0;
        }

        [Command(3)]
        // SetPowerSupplyChangeEventEnabled(u8)
        public long SetPowerSupplyChangeEventEnabled(ServiceCtx context)
        {
            bool powerSupplyChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, new { powerSupplyChangeEventEnabled });

            return 0;
        }

        [Command(4)]
        // SetBatteryVoltageStateChangeEventEnabled(u8)
        public long SetBatteryVoltageStateChangeEventEnabled(ServiceCtx context)
        {
            bool batteryVoltageStateChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, new { batteryVoltageStateChangeEventEnabled });

            return 0;
        }
    }
}