using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Psm
{
    class IPsmSession : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent StateChangeEvent;
        private int    StateChangeEventHandle;

        public IPsmSession(Horizon System)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, BindStateChangeEvent                     },
                { 1, UnbindStateChangeEvent                   },
                { 2, SetChargerTypeChangeEventEnabled         },
                { 3, SetPowerSupplyChangeEventEnabled         },
                { 4, SetBatteryVoltageStateChangeEventEnabled }
            };

            StateChangeEvent       = new KEvent(System);
            StateChangeEventHandle = -1;
        }

        // BindStateChangeEvent() -> KObject
        public long BindStateChangeEvent(ServiceCtx Context)
        {
            if (StateChangeEventHandle == -1)
            {
                KernelResult ResultCode = Context.Process.HandleTable.GenerateHandle(StateChangeEvent, out int StateChangeEventHandle);

                if (ResultCode != KernelResult.Success)
                {
                    return (long)ResultCode;
                }
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(StateChangeEventHandle);

            Logger.PrintStub(LogClass.ServicePsm, "Stubbed.");

            return 0;
        }

        // UnbindStateChangeEvent()
        public long UnbindStateChangeEvent(ServiceCtx Context)
        {
            if (StateChangeEventHandle != -1)
            {
                Context.Process.HandleTable.CloseHandle(StateChangeEventHandle);
                StateChangeEventHandle = -1;
            }

            Logger.PrintStub(LogClass.ServicePsm, "Stubbed.");

            return 0;
        }

        // SetChargerTypeChangeEventEnabled(u8)
        public long SetChargerTypeChangeEventEnabled(ServiceCtx Context)
        {
            bool ChargerTypeChangeEventEnabled = Context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. ChargerTypeChangeEventEnabled: {ChargerTypeChangeEventEnabled}");

            return 0;
        }

        // SetPowerSupplyChangeEventEnabled(u8)
        public long SetPowerSupplyChangeEventEnabled(ServiceCtx Context)
        {
            bool PowerSupplyChangeEventEnabled = Context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. PowerSupplyChangeEventEnabled: {PowerSupplyChangeEventEnabled}");

            return 0;
        }

        // SetBatteryVoltageStateChangeEventEnabled(u8)
        public long SetBatteryVoltageStateChangeEventEnabled(ServiceCtx Context)
        {
            bool BatteryVoltageStateChangeEventEnabled = Context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. BatteryVoltageStateChangeEventEnabled: {BatteryVoltageStateChangeEventEnabled}");

            return 0;
        }
    }
}