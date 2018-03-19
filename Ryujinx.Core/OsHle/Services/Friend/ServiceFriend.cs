using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Friend
{
    class ServiceFriend : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceFriend()
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