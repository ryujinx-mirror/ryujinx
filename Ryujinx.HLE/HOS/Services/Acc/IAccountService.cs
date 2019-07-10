using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services.Arp;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    [Service("acc:u0")]
    [Service("acc:u1")]
    class IAccountService : IpcService
    {
        private bool _userRegistrationRequestPermitted = false;

        private ApplicationLaunchProperty _applicationLaunchProperty;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IAccountService(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,   GetUserCount                         },
                { 1,   GetUserExistence                     },
                { 2,   ListAllUsers                         },
                { 3,   ListOpenUsers                        },
                { 4,   GetLastOpenedUser                    },
                { 5,   GetProfile                           },
              //{ 6,   GetProfileDigest                     }, // 3.0.0+
                { 50,  IsUserRegistrationRequestPermitted   },
                { 51,  TrySelectUserWithoutInteraction      },
              //{ 60,  ListOpenContextStoredUsers           }, // 5.0.0-5.1.0
              //{ 99,  DebugActivateOpenContextRetention    }, // 6.0.0+
                { 100, InitializeApplicationInfo            },
                { 101, GetBaasAccountManagerForApplication  },
              //{ 102, AuthenticateApplicationAsync         },
              //{ 103, CheckNetworkServiceAvailabilityAsync }, // 4.0.0+
                { 110, StoreSaveDataThumbnail               },
                { 111, ClearSaveDataThumbnail               },
              //{ 120, CreateGuestLoginRequest              },
              //{ 130, LoadOpenContext                      }, // 6.0.0+
              //{ 131, ListOpenContextStoredUsers           }, // 6.0.0+
                { 140, InitializeApplicationInfo            }, // 6.0.0+
              //{ 141, ListQualifiedUsers                   }, // 6.0.0+
                { 150, IsUserAccountSwitchLocked            }, // 6.0.0+
            };
        }

        // GetUserCount() -> i32
        public long GetUserCount(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.Account.GetUserCount());

            return 0;
        }

        // GetUserExistence(nn::account::Uid) -> bool
        public long GetUserExistence(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return MakeError(ErrorModule.Account, AccErr.NullArgument);
            }

            context.ResponseData.Write(context.Device.System.State.Account.TryGetUser(userId, out _));

            return 0;
        }

        // ListAllUsers() -> array<nn::account::Uid, 0xa>
        public long ListAllUsers(ServiceCtx context)
        {
            return WriteUserList(context, context.Device.System.State.Account.GetAllUsers());
        }

        // ListOpenUsers() -> array<nn::account::Uid, 0xa>
        public long ListOpenUsers(ServiceCtx context)
        {
            return WriteUserList(context, context.Device.System.State.Account.GetOpenedUsers());
        }

        private long WriteUserList(ServiceCtx context, IEnumerable<UserProfile> profiles)
        {
            if (context.Request.RecvListBuff.Count == 0)
            {
                return MakeError(ErrorModule.Account, AccErr.InvalidInputBuffer);
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

            return 0;
        }

        // GetLastOpenedUser() -> nn::account::Uid
        public long GetLastOpenedUser(ServiceCtx context)
        {
            context.Device.System.State.Account.LastOpenedUser.UserId.Write(context.ResponseData);

            return 0;
        }

        // GetProfile(nn::account::Uid) -> object<nn::account::profile::IProfile>
        public long GetProfile(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (!context.Device.System.State.Account.TryGetUser(userId, out UserProfile userProfile))
            {
                Logger.PrintWarning(LogClass.ServiceAcc, $"User 0x{userId} not found!");

                return MakeError(ErrorModule.Account, AccErr.UserNotFound);
            }

            MakeObject(context, new IProfile(userProfile));

            // Doesn't occur in our case.
            // return MakeError(ErrorModule.Account, AccErr.NullObject);

            return 0;
        }

        // IsUserRegistrationRequestPermitted(u64, pid) -> bool
        public long IsUserRegistrationRequestPermitted(ServiceCtx context)
        {
            // The u64 argument seems to be unused by account.
            context.ResponseData.Write(_userRegistrationRequestPermitted);

            return 0;
        }

        // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
        public long TrySelectUserWithoutInteraction(ServiceCtx context)
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

            return 0;
        }

        // InitializeApplicationInfo(u64, pid)
        // Both calls (100, 140) use the same submethod, maybe there's something different further along when arp:r is called?
        public long InitializeApplicationInfo(ServiceCtx context)
        {
            if (_applicationLaunchProperty != null)
            {
                return MakeError(ErrorModule.Account, AccErr.ApplicationLaunchPropertyAlreadyInit);
            }

            // The u64 argument seems to be unused by account.
            long unknown = context.RequestData.ReadInt64();

            // TODO: Account actually calls nn::arp::detail::IReader::GetApplicationLaunchProperty() with the current PID and store the result (ApplicationLaunchProperty) internally.
            //       For now we can hardcode values, and fix it after GetApplicationLaunchProperty is implemented.

            /*
            if (nn::arp::detail::IReader::GetApplicationLaunchProperty() == 0xCC9D) // InvalidProcessId
            {
                _applicationLaunchProperty = new ApplicationLaunchProperty
                {
                    TitleId             = 0x00;
                    Version             = 0x00;
                    BaseGameStorageId   = 0x03;
                    UpdateGameStorageId = 0x00;
                }

                return MakeError(ErrorModule.Account, AccErr.InvalidArgument);
            }
            else
            */
            {
                _applicationLaunchProperty = new ApplicationLaunchProperty
                {
                    TitleId             = BitConverter.ToInt64(StringUtils.HexToBytes(context.Device.System.TitleID), 0),
                    Version             = 0x00,
                    BaseGameStorageId   = (byte)StorageId.NandSystem,
                    UpdateGameStorageId = (byte)StorageId.None
                };
            }

            Logger.PrintStub(LogClass.ServiceAcc, new { unknown });

            return 0;
        }

        // GetBaasAccountManagerForApplication(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
        public long GetBaasAccountManagerForApplication(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return MakeError(ErrorModule.Account, AccErr.NullArgument);
            }

            if (_applicationLaunchProperty == null)
            {
                return MakeError(ErrorModule.Account, AccErr.InvalidArgument);
            }

            MakeObject(context, new IManagerForApplication(userId, _applicationLaunchProperty));

            // Doesn't occur in our case.
            // return MakeError(ErrorModule.Account, AccErr.NullObject);

            return 0;
        }

        // StoreSaveDataThumbnail(nn::account::Uid, buffer<bytes, 5>)
        public long StoreSaveDataThumbnail(ServiceCtx context)
        {
            if (_applicationLaunchProperty == null)
            {
                return MakeError(ErrorModule.Account, AccErr.InvalidArgument);
            }

            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return MakeError(ErrorModule.Account, AccErr.NullArgument);
            }

            if (context.Request.SendBuff.Count == 0)
            {
                return MakeError(ErrorModule.Account, AccErr.InvalidInputBuffer);
            }

            long inputPosition = context.Request.SendBuff[0].Position;
            long inputSize     = context.Request.SendBuff[0].Size;

            if (inputSize != 0x24000)
            {
                return MakeError(ErrorModule.Account, AccErr.InvalidInputBufferSize);
            }

            byte[] thumbnailBuffer = context.Memory.ReadBytes(inputPosition, inputSize);

            // TODO: Store thumbnailBuffer somewhere, in save data 0x8000000000000010 ?

            Logger.PrintStub(LogClass.ServiceAcc);

            return 0;
        }

        // ClearSaveDataThumbnail(nn::account::Uid)
        public long ClearSaveDataThumbnail(ServiceCtx context)
        {
            if (_applicationLaunchProperty == null)
            {
                return MakeError(ErrorModule.Account, AccErr.InvalidArgument);
            }

            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return MakeError(ErrorModule.Account, AccErr.NullArgument);
            }

            // TODO: Clear the Thumbnail somewhere, in save data 0x8000000000000010 ?

            Logger.PrintStub(LogClass.ServiceAcc);

            return 0;
        }

        // IsUserAccountSwitchLocked() -> bool
        public long IsUserAccountSwitchLocked(ServiceCtx context)
        {
            // TODO : Validate the following check.
            /*
            if (_applicationLaunchProperty != null)
            {
                return MakeError(ErrorModule.Account, AccErr.ApplicationLaunchPropertyAlreadyInit);
            }
            */

            // Account actually calls nn::arp::detail::IReader::GetApplicationControlProperty() with the current PID and store the result (NACP File) internally.
            // But since we use LibHac and we load one Application at a time, it's not necessary.

            context.ResponseData.Write(context.Device.System.ControlData.UserAccountSwitchLock);

            Logger.PrintStub(LogClass.ServiceAcc);

            return 0;
        }
    }
}
