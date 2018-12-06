using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    class IActiveApplicationDeviceList : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IActiveApplicationDeviceList()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, ActivateVibrationDevice }
            };
        }

        public long ActivateVibrationDevice(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();

            return 0;
        }
    }
}