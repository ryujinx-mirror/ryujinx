using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Glue;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    [Service("acc:u0")]
    [Service("acc:u1")]
    class IAccountService : IpcService
    {
        private bool _userRegistrationRequestPermitted = false;

        private ApplicationLaunchProperty _applicationLaunchProperty;

        public IAccountService(ServiceCtx context) { }

        [Command(0)]
        // GetUserCount() -> i32
        public ResultCode GetUserCount(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.Account.GetUserCount());

            return ResultCode.Success;
        }

        [Command(1)]
        // GetUserExistence(nn::account::Uid) -> bool
        public ResultCode GetUserExistence(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return ResultCode.NullArgument;
            }

            context.ResponseData.Write(context.Device.System.State.Account.TryGetUser(userId, out _));

            return ResultCode.Success;
        }

        [Command(2)]
        // ListAllUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListAllUsers(ServiceCtx context)
        {
            return WriteUserList(context, context.Device.System.State.Account.GetAllUsers());
        }

        [Command(3)]
        // ListOpenUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListOpenUsers(ServiceCtx context)
        {
            return WriteUserList(context, context.Device.System.State.Account.GetOpenedUsers());
        }

        private ResultCode WriteUserList(ServiceCtx context, IEnumerable<UserProfile> profiles)
        {
            if (context.Request.RecvListBuff.Count == 0)
            {
                return ResultCode.InvalidInputBuffer;
            }

            long outputPosition = context.Request.RecvListBuff[0].Position;
            long outputSize     = context.Request.RecvListBuff[0].Size;

            ulong offset = 0;

            foreach (UserProfile userProfile in profiles)
            {
                if (offset + 0x10 > (ulong)outputSize)
                {
                    break;
                }

                context.Memory.WriteInt64(outputPosition + (long)offset,     userProfile.UserId.Low);
                context.Memory.WriteInt64(outputPosition + (long)offset + 8, userProfile.UserId.High);

                offset += 0x10;
            }

            return ResultCode.Success;
        }

        [Command(4)]
        // GetLastOpenedUser() -> nn::account::Uid
        public ResultCode GetLastOpenedUser(ServiceCtx context)
        {
            context.Device.System.State.Account.LastOpenedUser.UserId.Write(context.ResponseData);

            return ResultCode.Success;
        }

        [Command(5)]
        // GetProfile(nn::account::Uid) -> object<nn::account::profile::IProfile>
        public ResultCode GetProfile(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (!context.Device.System.State.Account.TryGetUser(userId, out UserProfile userProfile))
            {
                Logger.PrintWarning(LogClass.ServiceAcc, $"User 0x{userId} not found!");

                return ResultCode.UserNotFound;
            }

            MakeObject(context, new IProfile(userProfile));

            // Doesn't occur in our case.
            // return ResultCode.NullObject;

            return ResultCode.Success;
        }

        [Command(50)]
        // IsUserRegistrationRequestPermitted(u64, pid) -> bool
        public ResultCode IsUserRegistrationRequestPermitted(ServiceCtx context)
        {
            // The u64 argument seems to be unused by account.
            context.ResponseData.Write(_userRegistrationRequestPermitted);

            return ResultCode.Success;
        }

        [Command(51)]
        // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
        public ResultCode TrySelectUserWithoutInteraction(ServiceCtx context)
        {
            if (context.Device.System.State.Account.GetUserCount() != 1)
            {
                // Invalid UserId.
                new UInt128(0, 0).Write(context.ResponseData);

                return 0;
            }

            bool baasCheck = context.RequestData.ReadBoolean();

            if (baasCheck)
            {
                // This checks something related to baas (online), and then return an invalid UserId if the check in baas returns an error code.
                // In our case, we can just log it for now.

                Logger.PrintStub(LogClass.ServiceAcc, new { baasCheck });
            }

            // As we returned an invalid UserId if there is more than one user earlier, now we can return only the first one.
            context.Device.System.State.Account.GetFirst().UserId.Write(context.ResponseData);

            return ResultCode.Success;
        }

        [Command(100)]
        [Command(140)] // 6.0.0+
        // InitializeApplicationInfo(u64, pid)
        // Both calls (100, 140) use the same submethod, maybe there's something different further along when arp:r is called?
        public ResultCode InitializeApplicationInfo(ServiceCtx context)
        {
            if (_applicationLaunchProperty != null)
            {
                return ResultCode.ApplicationLaunchPropertyAlreadyInit;
            }

            // The u64 argument seems to be unused by account.
            long unknown = context.RequestData.ReadInt64();

            // TODO: Account actually calls nn::arp::detail::IReader::GetApplicationLaunchProperty() with the current PID and store the result (ApplicationLaunchProperty) internally.
            //       For now we can hardcode values, and fix it after GetApplicationLaunchProperty is implemented.

            /*
            if (nn::arp::detail::IReader::GetApplicationLaunchProperty() == 0xCC9D) // InvalidProcessId
            {
                _applicationLaunchProperty = ApplicationLaunchProperty.Default;

                return ResultCode.InvalidArgument;
            }
            else
            */
            {
                _applicationLaunchProperty = ApplicationLaunchProperty.GetByPid(context);
            }

            Logger.PrintStub(LogClass.ServiceAcc, new { unknown });

            return ResultCode.Success;
        }

        [Command(101)]
        // GetBaasAccountManagerForApplication(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
        public ResultCode GetBaasAccountManagerForApplication(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return ResultCode.NullArgument;
            }

            if (_applicationLaunchProperty == null)
            {
                return ResultCode.InvalidArgument;
            }

            MakeObject(context, new IManagerForApplication(userId, _applicationLaunchProperty));

            // Doesn't occur in our case.
            // return ResultCode.NullObject;

            return ResultCode.Success;
        }

        [Command(110)]
        // StoreSaveDataThumbnail(nn::account::Uid, buffer<bytes, 5>)
        public ResultCode StoreSaveDataThumbnail(ServiceCtx context)
        {
            if (_applicationLaunchProperty == null)
            {
                return ResultCode.InvalidArgument;
            }

            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return ResultCode.NullArgument;
            }

            if (context.Request.SendBuff.Count == 0)
            {
                return ResultCode.InvalidInputBuffer;
            }

            long inputPosition = context.Request.SendBuff[0].Position;
            long inputSize     = context.Request.SendBuff[0].Size;

            if (inputSize != 0x24000)
            {
                return ResultCode.InvalidInputBufferSize;
            }

            byte[] thumbnailBuffer = context.Memory.ReadBytes(inputPosition, inputSize);

            // TODO: Store thumbnailBuffer somewhere, in save data 0x8000000000000010 ?

            Logger.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }

        [Command(111)]
        // ClearSaveDataThumbnail(nn::account::Uid)
        public ResultCode ClearSaveDataThumbnail(ServiceCtx context)
        {
            if (_applicationLaunchProperty == null)
            {
                return ResultCode.InvalidArgument;
            }

            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return ResultCode.NullArgument;
            }

            // TODO: Clear the Thumbnail somewhere, in save data 0x8000000000000010 ?

            Logger.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }

        [Command(150)] // 6.0.0+
        // IsUserAccountSwitchLocked() -> bool
        public ResultCode IsUserAccountSwitchLocked(ServiceCtx context)
        {
            // TODO : Validate the following check.
            /*
            if (_applicationLaunchProperty != null)
            {
                return ResultCode.ApplicationLaunchPropertyAlreadyInit;
            }
            */

            // Account actually calls nn::arp::detail::IReader::GetApplicationControlProperty() with the current PID and store the result (NACP File) internally.
            // But since we use LibHac and we load one Application at a time, it's not necessary.

            context.ResponseData.Write(context.Device.System.ControlData.UserAccountSwitchLock);

            Logger.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }
    }
}
