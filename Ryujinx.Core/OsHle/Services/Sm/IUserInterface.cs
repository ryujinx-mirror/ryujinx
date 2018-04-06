using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Sm
{
    class IUserInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private bool IsInitialized;

        public IUserInterface()
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
            IsInitialized = true;

            return 0;
        }

        public long GetService(ServiceCtx Context)
        {
            //Only for kernel version > 3.0.0.
            if (!IsInitialized)
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

            KSession Session = new KSession(ServiceFactory.MakeService(Name));

            int Handle = Context.Process.HandleTable.OpenHandle(Session);

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

            return 0;
        }
    }
}