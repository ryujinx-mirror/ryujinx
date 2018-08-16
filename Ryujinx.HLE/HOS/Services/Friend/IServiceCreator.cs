using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IServiceCreator : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IServiceCreator()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, CreateFriendService }
            };
        }

        public static long CreateFriendService(ServiceCtx Context)
        {
            MakeObject(Context, new IFriendService());

            return 0;
        }
    }
}