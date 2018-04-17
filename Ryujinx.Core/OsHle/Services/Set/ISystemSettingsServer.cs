using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.Settings;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Set
{
    class ISystemSettingsServer : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISystemSettingsServer()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 23, GetColorSetId },
                { 24, SetColorSetId }
            };
        }

        public static long GetColorSetId(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)Context.Ns.Settings.ThemeColor);

            return 0;
        }

        public static long SetColorSetId(ServiceCtx Context)
        {
            int ColorSetId = Context.RequestData.ReadInt32();

            Context.Ns.Settings.ThemeColor = (ColorSet)ColorSetId;
            return 0;
        }
    }
}