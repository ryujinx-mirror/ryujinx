using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Logging;
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
                { 100, InitializeApplicationInfo           },
                { 101, GetBaasAccountManagerForApplication }
            };
        }

        public long GetUserCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(Context.Device.System.State.GetUserCount());

            return 0;
        }

        public long GetUserExistence(ServiceCtx Context)
        {
            UserId Uuid = new UserId(
                Context.RequestData.ReadInt64(),
                Context.RequestData.ReadInt64());

            Context.ResponseData.Write(Context.Device.System.State.TryGetUser(Uuid, out _) ? 1 : 0);

            return 0;
        }

        public long ListAllUsers(ServiceCtx Context)
        {
            return WriteUserList(Context, Context.Device.System.State.GetAllUsers());
        }

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

                byte[] Uuid = Profile.Uuid.Bytes;

                for (int Index = Uuid.Length - 1; Index >= 0; Index--)
                {
                    Context.Memory.WriteByte(OutputPosition + Offset++, Uuid[Index]);
                }
            }

            return 0;
        }

        public long GetLastOpenedUser(ServiceCtx Context)
        {
            UserProfile LastOpened = Context.Device.System.State.LastOpenUser;

            LastOpened.Uuid.Write(Context.ResponseData);

            return 0;
        }

        public long GetProfile(ServiceCtx Context)
        {
            UserId Uuid = new UserId(
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

        public long InitializeApplicationInfo(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long GetBaasAccountManagerForApplication(ServiceCtx Context)
        {
            MakeObject(Context, new IManagerForApplication());

            return 0;
        }
    }
}
