using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Sm
{
    class ServiceSm : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceSm()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Initialize },
                { 1, GetService }
            };
        }

        private const int SmNotInitialized = 0x415;

        public long Initialize(ServiceCtx Context)
        {
            Context.Session.Initialize();

            return 0;
        }

        public long GetService(ServiceCtx Context)
        {
            //Only for kernel version > 3.0.0.
            if (!Context.Session.IsInitialized)
            {
                //return SmNotInitialized;
            }

            string Name = string.Empty;

            for (int Index = 0; Index < 8 &&
                Context.RequestData.BaseStream.Position <
                Context.RequestData.BaseStream.Length; Index++)
            {
                byte Chr = Context.RequestData.ReadByte();

                if (Chr >= 0x20 && Chr < 0x7f)
                {
                    Name += (char)Chr;
                }
            }

            if (Name == string.Empty)
            {
                return 0;
            }

            HSession Session = new HSession(Context.Process.Services.GetService(Name));

            int Handle = Context.Process.HandleTable.OpenHandle(Session);

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

            return 0;
        }
    }
}