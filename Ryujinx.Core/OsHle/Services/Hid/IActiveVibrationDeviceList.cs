using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Hid
{
    class IActiveApplicationDeviceList : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IActiveApplicationDeviceList()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ActivateVibrationDevice }
            };
        }

        public long ActivateVibrationDevice(ServiceCtx Context)
        {
            int VibrationDeviceHandle = Context.RequestData.ReadInt32();

            return 0;
        }
    }
}