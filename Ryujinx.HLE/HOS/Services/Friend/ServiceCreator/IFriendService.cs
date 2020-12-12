using LibHac.Ns;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.FriendService;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator
{
    class IFriendService : IpcService
    {
        private FriendServicePermissionLevel _permissionLevel;
        private KEvent                       _completionEvent;

        public IFriendService(FriendServicePermissionLevel permissionLevel)
        {
            _permissionLevel = permissionLevel;
        }

        [Command(0)]
        // GetCompletionEvent() -> handle<copy>
        public ResultCode GetCompletionEvent(ServiceCtx context)
        {
            if (_completionEvent == null)
            {
                _completionEvent = new KEvent(context.Device.System.KernelContext);
            }

            if (context.Process.HandleTable.GenerateHandle(_completionEvent.ReadableEvent, out int completionEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(completionEventHandle);

            return ResultCode.Success;
        }

        [Command(10100)]
        // nn::friends::GetFriendListIds(int offset, nn::account::Uid userId, nn::friends::detail::ipc::SizedFriendFilter friendFilter, ulong pidPlaceHolder, pid)
        // -> int outCount, array<nn::account::NetworkServiceAccountId, 0xa>
        public ResultCode GetFriendListIds(ServiceCtx context)
        {
            int offset = context.RequestData.ReadInt32();

            // Padding
            context.RequestData.ReadInt32();

            UserId       userId = context.RequestData.ReadStruct<UserId>();
            FriendFilter filter = context.RequestData.ReadStruct<FriendFilter>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            if (userId.IsNull)
            {
                return ResultCode.InvalidArgument;
            }

            // There are no friends online, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceFriend, new
            {
                UserId = userId.ToString(),
                offset,
                filter.PresenceStatus,
                filter.IsFavoriteOnly,
                filter.IsSameAppPresenceOnly,
                filter.IsSameAppPlayedOnly,
                filter.IsArbitraryAppPlayedOnly,
                filter.PresenceGroupId,
            });

            return ResultCode.Success;
        }

        [Command(10101)]
        // nn::friends::GetFriendList(int offset, nn::account::Uid userId, nn::friends::detail::ipc::SizedFriendFilter friendFilter, ulong pidPlaceHolder, pid)
        // -> int outCount, array<nn::friends::detail::FriendImpl, 0x6>
        public ResultCode GetFriendList(ServiceCtx context)
        {
            int offset = context.RequestData.ReadInt32();

            // Padding
            context.RequestData.ReadInt32();

            UserId       userId   = context.RequestData.ReadStruct<UserId>();
            FriendFilter filter = context.RequestData.ReadStruct<FriendFilter>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            if (userId.IsNull)
            {
                return ResultCode.InvalidArgument;
            }

            // There are no friends online, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceFriend, new {
                UserId = userId.ToString(),
                offset,
                filter.PresenceStatus,
                filter.IsFavoriteOnly,
                filter.IsSameAppPresenceOnly,
                filter.IsSameAppPlayedOnly,
                filter.IsArbitraryAppPlayedOnly,
                filter.PresenceGroupId,
            });

            return ResultCode.Success;
        }

        [Command(10400)]
        // nn::friends::GetBlockedUserListIds(int offset, nn::account::Uid userId) -> (u32, buffer<nn::account::NetworkServiceAccountId, 0xa>)
        public ResultCode GetBlockedUserListIds(ServiceCtx context)
        {
            int offset = context.RequestData.ReadInt32();

            // Padding
            context.RequestData.ReadInt32();

            UserId userId = context.RequestData.ReadStruct<UserId>();

            // There are no friends blocked, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceFriend, new { offset, UserId = userId.ToString() });

            return ResultCode.Success;
        }

        [Command(10600)]
        // nn::friends::DeclareOpenOnlinePlaySession(nn::account::Uid userId)
        public ResultCode DeclareOpenOnlinePlaySession(ServiceCtx context)
        {
            UserId userId = context.RequestData.ReadStruct<UserId>();

            if (userId.IsNull)
            {
                return ResultCode.InvalidArgument;
            }

            if (context.Device.System.State.Account.TryGetUser(userId, out UserProfile profile))
            {
                profile.OnlinePlayState = AccountState.Open;
            }

            Logger.Stub?.PrintStub(LogClass.ServiceFriend, new { UserId = userId.ToString(), profile.OnlinePlayState });

            return ResultCode.Success;
        }

        [Command(10601)]
        // nn::friends::DeclareCloseOnlinePlaySession(nn::account::Uid userId)
        public ResultCode DeclareCloseOnlinePlaySession(ServiceCtx context)
        {
            UserId userId = context.RequestData.ReadStruct<UserId>();

            if (userId.IsNull)
            {
                return ResultCode.InvalidArgument;
            }

            if (context.Device.System.State.Account.TryGetUser(userId, out UserProfile profile))
            {
                profile.OnlinePlayState = AccountState.Closed;
            }

            Logger.Stub?.PrintStub(LogClass.ServiceFriend, new { UserId = userId.ToString(), profile.OnlinePlayState });

            return ResultCode.Success;
        }

        [Command(10610)]
        // nn::friends::UpdateUserPresence(nn::account::Uid, u64, pid, buffer<nn::friends::detail::UserPresenceImpl, 0x19>)
        public ResultCode UpdateUserPresence(ServiceCtx context)
        {
            UserId uuid = context.RequestData.ReadStruct<UserId>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            long position = context.Request.PtrBuff[0].Position;
            long size     = context.Request.PtrBuff[0].Size;

            byte[] bufferContent = new byte[size];

            context.Memory.Read((ulong)position, bufferContent);

            if (uuid.IsNull)
            {
                return ResultCode.InvalidArgument;
            }

            int elementCount = bufferContent.Length / Marshal.SizeOf<UserPresence>();

            using (BinaryReader bufferReader = new BinaryReader(new MemoryStream(bufferContent)))
            {
                UserPresence[] userPresenceInputArray = bufferReader.ReadStructArray<UserPresence>(elementCount);

                Logger.Stub?.PrintStub(LogClass.ServiceFriend, new { UserId = uuid.ToString(), userPresenceInputArray });
            }

            return ResultCode.Success;
        }

        [Command(10700)]
        // nn::friends::GetPlayHistoryRegistrationKey(b8 unknown, nn::account::Uid) -> buffer<nn::friends::PlayHistoryRegistrationKey, 0x1a>
        public ResultCode GetPlayHistoryRegistrationKey(ServiceCtx context)
        {
            bool   unknownBool = context.RequestData.ReadBoolean();
            UserId userId      = context.RequestData.ReadStruct<UserId>();

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(0x40L);

            long bufferPosition  = context.Request.RecvListBuff[0].Position;

            if (userId.IsNull)
            {
                return ResultCode.InvalidArgument;
            }

            // NOTE: Calls nn::friends::detail::service::core::PlayHistoryManager::GetInstance and stores the instance.

            byte[] randomBytes = new byte[8];
            Random random      = new Random();

            random.NextBytes(randomBytes);

            // NOTE: Calls nn::friends::detail::service::core::UuidManager::GetInstance and stores the instance.
            //       Then call nn::friends::detail::service::core::AccountStorageManager::GetInstance and store the instance.
            //       Then it checks if an Uuid is already stored for the UserId, if not it generates a random Uuid.
            //       And store it in the savedata 8000000000000080 in the friends:/uid.bin file.

            Array16<byte> randomGuid = new Array16<byte>();

            Guid.NewGuid().ToByteArray().AsSpan().CopyTo(randomGuid.ToSpan());

            PlayHistoryRegistrationKey playHistoryRegistrationKey = new PlayHistoryRegistrationKey
            {
                Type        = 0x101,
                KeyIndex    = (byte)(randomBytes[0] & 7),
                UserIdBool  = 0, // TODO: Find it.
                UnknownBool = (byte)(unknownBool ? 1 : 0), // TODO: Find it.
                Reserved    = new Array11<byte>(),
                Uuid        = randomGuid
            };

            ReadOnlySpan<byte> playHistoryRegistrationKeyBuffer = SpanHelpers.AsByteSpan(ref playHistoryRegistrationKey);

            /*

            NOTE: The service uses the KeyIndex to get a random key from a keys buffer (since the key index is stored in the returned buffer).
                  We currently don't support play history and online services so we can use a blank key for now.
                  Code for reference:

            byte[] hmacKey = new byte[0x20];

            HMACSHA256 hmacSha256 = new HMACSHA256(hmacKey);
            byte[]     hmacHash   = hmacSha256.ComputeHash(playHistoryRegistrationKeyBuffer);

            */

            context.Memory.Write((ulong)bufferPosition,        playHistoryRegistrationKeyBuffer);
            context.Memory.Write((ulong)bufferPosition + 0x20, new byte[0x20]); // HmacHash

            return ResultCode.Success;
        }

        [Command(10702)]
        // nn::friends::AddPlayHistory(nn::account::Uid, u64, pid, buffer<nn::friends::PlayHistoryRegistrationKey, 0x19>, buffer<nn::friends::InAppScreenName, 0x19>, buffer<nn::friends::InAppScreenName, 0x19>)
        public ResultCode AddPlayHistory(ServiceCtx context)
        {
            UserId userId = context.RequestData.ReadStruct<UserId>();

            // Pid placeholder
            context.RequestData.ReadInt64();
            long pid = context.Process.Pid;

            long playHistoryRegistrationKeyPosition = context.Request.PtrBuff[0].Position;
            long PlayHistoryRegistrationKeySize     = context.Request.PtrBuff[0].Size;

            long inAppScreenName1Position = context.Request.PtrBuff[1].Position;
            long inAppScreenName1Size     = context.Request.PtrBuff[1].Size;

            long inAppScreenName2Position = context.Request.PtrBuff[2].Position;
            long inAppScreenName2Size     = context.Request.PtrBuff[2].Size;

            if (userId.IsNull || inAppScreenName1Size > 0x48 || inAppScreenName2Size > 0x48)
            {
                return ResultCode.InvalidArgument;
            }

            // TODO: Call nn::arp::GetApplicationControlProperty here when implemented.
            ApplicationControlProperty controlProperty = context.Device.Application.ControlData.Value;

            /*

            NOTE: The service calls nn::friends::detail::service::core::PlayHistoryManager to store informations using the registration key computed in GetPlayHistoryRegistrationKey.
                  Then calls nn::friends::detail::service::core::FriendListManager to update informations on the friend list.
                  We currently don't support play history and online services so it's fine to do nothing.

            */

            Logger.Stub?.PrintStub(LogClass.ServiceFriend);

            return ResultCode.Success;
        }
    }
}