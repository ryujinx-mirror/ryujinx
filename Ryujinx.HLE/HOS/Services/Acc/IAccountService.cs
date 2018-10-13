using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    class IAccountService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAccountService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
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
        public long GetUserCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(Context.Device.System.State.GetUserCount());

            return 0;
        }

        // GetUserExistence(nn::account::Uid) -> bool
        public long GetUserExistence(ServiceCtx Context)
        {
            UInt128 Uuid = new UInt128(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            Context.ResponseData.Write(Context.Device.System.State.TryGetUser(Uuid, out _));

            return 0;
        }

        // ListAllUsers() -> array<nn::account::Uid, 0xa>
        public long ListAllUsers(ServiceCtx Context)
        {
            return WriteUserList(Context, Context.Device.System.State.GetAllUsers());
        }

        // ListOpenUsers() -> array<nn::account::Uid, 0xa>
        public long ListOpenUsers(ServiceCtx Context)
        {
            return WriteUserList(Context, Context.Device.System.State.GetOpenUsers());
        }

        private long WriteUserList(ServiceCtx Context, IEnumerable<UserProfile> Profiles)
        {
            long OutputPosition = Context.Request.RecvListBuff[0].Position;
            long OutputSize     = Context.Request.RecvListBuff[0].Size;

            long Offset = 0;

            foreach (UserProfile Profile in Profiles)
            {
                if ((ulong)Offset + 16 > (ulong)OutputSize)
                {
                    break;
                }

                Context.Memory.WriteInt64(OutputPosition, Profile.Uuid.High);
                Context.Memory.WriteInt64(OutputPosition + 8, Profile.Uuid.Low);
            }

            return 0;
        }

        // GetLastOpenedUser() -> nn::account::Uid
        public long GetLastOpenedUser(ServiceCtx Context)
        {
            UserProfile LastOpened = Context.Device.System.State.LastOpenUser;

            LastOpened.Uuid.Write(Context.ResponseData);

            return 0;
        }

        // GetProfile(nn::account::Uid) -> object<nn::account::profile::IProfile>
        public long GetProfile(ServiceCtx Context)
        {
            UInt128 Uuid = new UInt128(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            if (!Context.Device.System.State.TryGetUser(Uuid, out UserProfile Profile))
            {
                Context.Device.Log.PrintWarning(LogClass.ServiceAcc, $"User 0x{Uuid} not found!");

                return MakeError(ErrorModule.Account, AccErr.UserNotFound);
            }

            MakeObject(Context, new IProfile(Profile));

            return 0;
        }

        // IsUserRegistrationRequestPermitted(u64, pid) -> bool
        public long IsUserRegistrationRequestPermitted(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceAcc, $"Stubbed. Unknown: {Unknown}");

            Context.ResponseData.Write(false);

            return 0;
        }

        // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
        public long TrySelectUserWithoutInteraction(ServiceCtx Context)
        {
            bool Unknown = Context.RequestData.ReadBoolean();

            Context.Device.Log.PrintStub(LogClass.ServiceAcc, $"Stubbed. Unknown: {Unknown}");

            UserProfile Profile = Context.Device.System.State.LastOpenUser;

            Profile.Uuid.Write(Context.ResponseData);

            return 0;
        }

        // InitializeApplicationInfo(u64, pid)
        public long InitializeApplicationInfo(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt64();

            Context.Device.Log.PrintStub(LogClass.ServiceAcc, $"Stubbed. Unknown: {Unknown}");

            return 0;
        }

        //  GetBaasAccountManagerForApplication(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
        public long GetBaasAccountManagerForApplication(ServiceCtx Context)
        {
            UInt128 Uuid = new UInt128(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            MakeObject(Context, new IManagerForApplication(Uuid));

            return 0;
        }
    }
}
