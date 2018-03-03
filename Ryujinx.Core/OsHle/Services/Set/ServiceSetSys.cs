using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Set
{
    class ServiceSetSys : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceSetSys()
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
            return 0;
        }
    }
}