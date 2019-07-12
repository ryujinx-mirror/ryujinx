using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Utilities;
using System.IO;
using System.Runtime.InteropServices;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IFriendService : IpcService
    {
        private FriendServicePermissionLevel _permissionLevel;

        public IFriendService(FriendServicePermissionLevel permissionLevel)
        {
            _permissionLevel = permissionLevel;
        }

        [Command(10100)]
        // nn::friends::GetFriendListIds(int offset, nn::account::Uid userUUID, nn::friends::detail::ipc::SizedFriendFilter friendFilter, ulong pidPlaceHolder, pid) 
        // -> int outCount, array<nn::account::NetworkServiceAccountId, 0xa>
        public long GetFriendListIds(ServiceCtx context)
        {
            int offset = context.RequestData.ReadInt32();

            // Padding
            context.RequestData.ReadInt32();

            UInt128      uuid   = context.RequestData.ReadStruct<UInt128>();
            FriendFilter filter = context.RequestData.ReadStruct<FriendFilter>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            // There are no friends online, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceFriend, new
            {
                UserId = uuid.ToString(),
                offset,
                filter.PresenceStatus,
                filter.IsFavoriteOnly,
                filter.IsSameAppPresenceOnly,
                filter.IsSameAppPlayedOnly,
                filter.IsArbitraryAppPlayedOnly,
                filter.PresenceGroupId,
            });

            return 0;
        }

        [Command(10101)]
        // nn::friends::GetFriendList(int offset, nn::account::Uid userUUID, nn::friends::detail::ipc::SizedFriendFilter friendFilter, ulong pidPlaceHolder, pid) 
        // -> int outCount, array<nn::friends::detail::FriendImpl, 0x6>
        public long GetFriendList(ServiceCtx context)
        {
            int offset = context.RequestData.ReadInt32();

            // Padding
            context.RequestData.ReadInt32();

            UInt128      uuid   = context.RequestData.ReadStruct<UInt128>();
            FriendFilter filter = context.RequestData.ReadStruct<FriendFilter>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            // There are no friends online, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceFriend, new {
                UserId = uuid.ToString(),
                offset,
                filter.PresenceStatus,
                filter.IsFavoriteOnly,
                filter.IsSameAppPresenceOnly,
                filter.IsSameAppPlayedOnly,
                filter.IsArbitraryAppPlayedOnly,
                filter.PresenceGroupId,
            });

            return 0;
        }

        [Command(10600)]
        // nn::friends::DeclareOpenOnlinePlaySession(nn::account::Uid)
        public long DeclareOpenOnlinePlaySession(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            if (context.Device.System.State.Account.TryGetUser(uuid, out UserProfile profile))
            {
                profile.OnlinePlayState = AccountState.Open;
            }

            Logger.PrintStub(LogClass.ServiceFriend, new { UserId = uuid.ToString(), profile.OnlinePlayState });

            return 0;
        }

        [Command(10601)]
        // nn::friends::DeclareCloseOnlinePlaySession(nn::account::Uid)
        public long DeclareCloseOnlinePlaySession(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            if (context.Device.System.State.Account.TryGetUser(uuid, out UserProfile profile))
            {
                profile.OnlinePlayState = AccountState.Closed;
            }

            Logger.PrintStub(LogClass.ServiceFriend, new { UserId = uuid.ToString(), profile.OnlinePlayState });

            return 0;
        }

        [Command(10610)]
        // nn::friends::UpdateUserPresence(nn::account::Uid, u64, pid, buffer<nn::friends::detail::UserPresenceImpl, 0x19>)
        public long UpdateUserPresence(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            long position = context.Request.PtrBuff[0].Position;
            long size     = context.Request.PtrBuff[0].Size;

            byte[] bufferContent = context.Memory.ReadBytes(position, size);

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            int elementCount = bufferContent.Length / Marshal.SizeOf<UserPresence>();

            using (BinaryReader bufferReader = new BinaryReader(new MemoryStream(bufferContent)))
            {
                UserPresence[] userPresenceInputArray = bufferReader.ReadStructArray<UserPresence>(elementCount);

                Logger.PrintStub(LogClass.ServiceFriend, new { UserId = uuid.ToString(), userPresenceInputArray });
            }

            return 0;
        }
    }
}