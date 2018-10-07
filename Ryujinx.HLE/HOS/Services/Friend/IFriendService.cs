using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.Utilities;
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
                { 10101, GetFriendList                 },
                { 10601, DeclareCloseOnlinePlaySession },
                { 10610, UpdateUserPresence            }
            };
        }

        // nn::friends::GetFriendListGetFriendListIds(nn::account::Uid, int Unknown0, nn::friends::detail::ipc::SizedFriendFilter, ulong Unknown1) -> int CounterIds,  array<nn::account::NetworkServiceAccountId>
        public long GetFriendList(ServiceCtx Context)
        {
            UInt128 Uuid = new UInt128(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            int Unknown0 = Context.RequestData.ReadInt32();

            FriendFilter Filter = new FriendFilter()
            {
                PresenceStatus           = (PresenceStatusFilter)Context.RequestData.ReadInt32(),
                IsFavoriteOnly           = Context.RequestData.ReadBoolean(),
                IsSameAppPresenceOnly    = Context.RequestData.ReadBoolean(),
                IsSameAppPlayedOnly      = Context.RequestData.ReadBoolean(),
                IsArbitraryAppPlayedOnly = Context.RequestData.ReadBoolean(),
                PresenceGroupId          = Context.RequestData.ReadInt64()
            };

            long Unknown1 = Context.RequestData.ReadInt64();

            // There are no friends online, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            Context.ResponseData.Write(0);

            Context.Device.Log.PrintStub(LogClass.ServiceFriend, $"Stubbed. UserId: {Uuid.ToString()} - " +
                                                                 $"Unknown0: {Unknown0} - " +
                                                                 $"PresenceStatus: {Filter.PresenceStatus} - " +
                                                                 $"IsFavoriteOnly: {Filter.IsFavoriteOnly} - " +
                                                                 $"IsSameAppPresenceOnly: {Filter.IsSameAppPresenceOnly} - " +
                                                                 $"IsSameAppPlayedOnly: {Filter.IsSameAppPlayedOnly} - " +
                                                                 $"IsArbitraryAppPlayedOnly: {Filter.IsArbitraryAppPlayedOnly} - " +
                                                                 $"PresenceGroupId: {Filter.PresenceGroupId} - " +
                                                                 $"Unknown1: {Unknown1}");

            return 0;
        }

        // DeclareCloseOnlinePlaySession(nn::account::Uid)
        public long DeclareCloseOnlinePlaySession(ServiceCtx Context)
        {
            UInt128 Uuid = new UInt128(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            if (Context.Device.System.State.TryGetUser(Uuid, out UserProfile Profile))
            {
                Profile.OnlinePlayState = OpenCloseState.Closed;
            }

            Context.Device.Log.PrintStub(LogClass.ServiceFriend, $"Stubbed. Uuid: {Uuid.ToString()} - " +
                                                                 $"OnlinePlayState: {Profile.OnlinePlayState}");

            return 0;
        }

        // UpdateUserPresence(nn::account::Uid, ulong Unknown0) -> buffer<Unknown1, type: 0x19, size: 0xe0>
        public long UpdateUserPresence(ServiceCtx Context)
        {
            UInt128 Uuid = new UInt128(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            long Unknown0 = Context.RequestData.ReadInt64();

            long Position = Context.Request.PtrBuff[0].Position;
            long Size     = Context.Request.PtrBuff[0].Size;

            //Todo: Write the buffer content.

            Context.Device.Log.PrintStub(LogClass.ServiceFriend, $"Stubbed. Uuid: {Uuid.ToString()} - " +
                                                                 $"Unknown0: {Unknown0}");

            return 0;
        }
    }
}
