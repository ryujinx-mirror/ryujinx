using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    class IAccountService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IAccountService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,   GetUserCount                        },
                { 1,   GetUserExistence                    },
                { 2,   ListAllUsers                        },
                { 3,   ListOpenUsers                       },
                { 4,   GetLastOpenedUser                   },
                { 5,   GetProfile                          },
                { 50,  IsUserRegistrationRequestPermitted  },
                { 51,  TrySelectUserWithoutInteraction     },
                { 100, InitializeApplicationInfo           },
                { 101, GetBaasAccountManagerForApplication }
            };
        }

        // GetUserCount() -> i32
        public long GetUserCount(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.GetUserCount());

            return 0;
        }

        // GetUserExistence(nn::account::Uid) -> bool
        public long GetUserExistence(ServiceCtx context)
        {
            UInt128 uuid = new UInt128(
                context.RequestData.ReadInt64(),
                context.RequestData.ReadInt64());

            context.ResponseData.Write(context.Device.System.State.TryGetUser(uuid, out _));

            return 0;
        }

        // ListAllUsers() -> array<nn::account::Uid, 0xa>
        public long ListAllUsers(ServiceCtx context)
        {
            return WriteUserList(context, context.Device.System.State.GetAllUsers());
        }

        // ListOpenUsers() -> array<nn::account::Uid, 0xa>
        public long ListOpenUsers(ServiceCtx context)
        {
            return WriteUserList(context, context.Device.System.State.GetOpenUsers());
        }

        private long WriteUserList(ServiceCtx context, IEnumerable<UserProfile> profiles)
        {
            long outputPosition = context.Request.RecvListBuff[0].Position;
            long outputSize     = context.Request.RecvListBuff[0].Size;

            long offset = 0;

            foreach (UserProfile profile in profiles)
            {
                if ((ulong)offset + 16 > (ulong)outputSize)
                {
                    break;
                }

                context.Memory.WriteInt64(outputPosition, profile.Uuid.Low);
                context.Memory.WriteInt64(outputPosition + 8, profile.Uuid.High);
            }

            return 0;
        }

        // GetLastOpenedUser() -> nn::account::Uid
        public long GetLastOpenedUser(ServiceCtx context)
        {
            UserProfile lastOpened = context.Device.System.State.LastOpenUser;

            lastOpened.Uuid.Write(context.ResponseData);

            return 0;
        }

        // GetProfile(nn::account::Uid) -> object<nn::account::profile::IProfile>
        public long GetProfile(ServiceCtx context)
        {
            UInt128 uuid = new UInt128(
                context.RequestData.ReadInt64(),
                context.RequestData.ReadInt64());

            if (!context.Device.System.State.TryGetUser(uuid, out UserProfile profile))
            {
                Logger.PrintWarning(LogClass.ServiceAcc, $"User 0x{uuid} not found!");

                return MakeError(ErrorModule.Account, AccErr.UserNotFound);
            }

            MakeObject(context, new IProfile(profile));

            return 0;
        }

        // IsUserRegistrationRequestPermitted(u64, pid) -> bool
        public long IsUserRegistrationRequestPermitted(ServiceCtx context)
        {
            long unknown = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceAcc, $"Stubbed. Unknown: {unknown}");

            context.ResponseData.Write(false);

            return 0;
        }

        // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
        public long TrySelectUserWithoutInteraction(ServiceCtx context)
        {
            bool unknown = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServiceAcc, $"Stubbed. Unknown: {unknown}");

            UserProfile profile = context.Device.System.State.LastOpenUser;

            profile.Uuid.Write(context.ResponseData);

            return 0;
        }

        // InitializeApplicationInfo(u64, pid)
        public long InitializeApplicationInfo(ServiceCtx context)
        {
            long unknown = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceAcc, $"Stubbed. Unknown: {unknown}");

            return 0;
        }

        //  GetBaasAccountManagerForApplication(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
        public long GetBaasAccountManagerForApplication(ServiceCtx context)
        {
            UInt128 uuid = new UInt128(
                context.RequestData.ReadInt64(),
                context.RequestData.ReadInt64());

            MakeObject(context, new IManagerForApplication(uuid));

            return 0;
        }
    }
}
