using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Psm
{
    class IPsmSession : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private KEvent _stateChangeEvent;
        private int    _stateChangeEventHandle;

        public IPsmSession(Horizon system)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, BindStateChangeEvent                     },
                { 1, UnbindStateChangeEvent                   },
                { 2, SetChargerTypeChangeEventEnabled         },
                { 3, SetPowerSupplyChangeEventEnabled         },
                { 4, SetBatteryVoltageStateChangeEventEnabled }
            };

            _stateChangeEvent       = new KEvent(system);
            _stateChangeEventHandle = -1;
        }

        // BindStateChangeEvent() -> KObject
        public long BindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle == -1)
            {
                KernelResult resultCode = context.Process.HandleTable.GenerateHandle(_stateChangeEvent, out int stateChangeEventHandle);

                if (resultCode != KernelResult.Success)
                {
                    return (long)resultCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_stateChangeEventHandle);

            Logger.PrintStub(LogClass.ServicePsm, "Stubbed.");

            return 0;
        }

        // UnbindStateChangeEvent()
        public long UnbindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle != -1)
            {
                context.Process.HandleTable.CloseHandle(_stateChangeEventHandle);
                _stateChangeEventHandle = -1;
            }

            Logger.PrintStub(LogClass.ServicePsm, "Stubbed.");

            return 0;
        }

        // SetChargerTypeChangeEventEnabled(u8)
        public long SetChargerTypeChangeEventEnabled(ServiceCtx context)
        {
            bool chargerTypeChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. ChargerTypeChangeEventEnabled: {chargerTypeChangeEventEnabled}");

            return 0;
        }

        // SetPowerSupplyChangeEventEnabled(u8)
        public long SetPowerSupplyChangeEventEnabled(ServiceCtx context)
        {
            bool powerSupplyChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. PowerSupplyChangeEventEnabled: {powerSupplyChangeEventEnabled}");

            return 0;
        }

        // SetBatteryVoltageStateChangeEventEnabled(u8)
        public long SetBatteryVoltageStateChangeEventEnabled(ServiceCtx context)
        {
            bool batteryVoltageStateChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. BatteryVoltageStateChangeEventEnabled: {batteryVoltageStateChangeEventEnabled}");

            return 0;
        }
    }
}