using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IFriendService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IFriendService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 10601, DeclareCloseOnlinePlaySession },
                { 10610, UpdateUserPresence            }
            };
        }

        public long DeclareCloseOnlinePlaySession(ServiceCtx Context)
        {
            UserId Uuid = new UserId(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            if (Context.Device.System.State.TryGetUser(Uuid, out UserProfile Profile))
            {
                Profile.OnlinePlayState = OpenCloseState.Closed;
            }

            return 0;
        }

        public long UpdateUserPresence(ServiceCtx Context)
        {
            UserId Uuid = new UserId(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            //TODO.
            Context.Device.Log.PrintStub(LogClass.ServiceFriend, "Stubbed.");

            return 0;
        }
    }
}