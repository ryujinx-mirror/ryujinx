using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Account.Acc.AccountService;
using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    [Service("acc:u0", AccountServiceFlag.Application)] // Max Sessions: 4
    class IAccountServiceForApplication : IpcService
    {
        private ApplicationServiceServer _applicationServiceServer;

        public IAccountServiceForApplication(ServiceCtx context, AccountServiceFlag serviceFlag)
        {
            _applicationServiceServer = new ApplicationServiceServer(serviceFlag);
        }

        [Command(0)]
        // GetUserCount() -> i32
        public ResultCode GetUserCount(ServiceCtx context)
        {
            return _applicationServiceServer.GetUserCountImpl(context);
        }

        [Command(1)]
        // GetUserExistence(nn::account::Uid) -> bool
        public ResultCode GetUserExistence(ServiceCtx context)
        {
            return _applicationServiceServer.GetUserExistenceImpl(context);
        }

        [Command(2)]
        // ListAllUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListAllUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListAllUsers(context);
        }

        [Command(3)]
        // ListOpenUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListOpenUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListOpenUsers(context);
        }

        [Command(4)]
        // GetLastOpenedUser() -> nn::account::Uid
        public ResultCode GetLastOpenedUser(ServiceCtx context)
        {
            return _applicationServiceServer.GetLastOpenedUser(context);
        }

        [Command(5)]
        // GetProfile(nn::account::Uid) -> object<nn::account::profile::IProfile>
        public ResultCode GetProfile(ServiceCtx context)
        {
            ResultCode resultCode = _applicationServiceServer.GetProfile(context, out IProfile iProfile);

            if (resultCode == ResultCode.Success)
            {
                MakeObject(context, iProfile);
            }

            return resultCode;
        }

        [Command(50)]
        // IsUserRegistrationRequestPermitted(pid) -> bool
        public ResultCode IsUserRegistrationRequestPermitted(ServiceCtx context)
        {
            // NOTE: pid is unused.
            return _applicationServiceServer.IsUserRegistrationRequestPermitted(context);
        }

        [Command(51)]
        // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
        public ResultCode TrySelectUserWithoutInteraction(ServiceCtx context)
        {
            return _applicationServiceServer.TrySelectUserWithoutInteraction(context);
        }

        [Command(100)]
        [Command(140)] // 6.0.0+
        // InitializeApplicationInfo(u64 pid_placeholder, pid)
        public ResultCode InitializeApplicationInfo(ServiceCtx context)
        {
            // NOTE: In call 100, account service use the pid_placeholder instead of the real pid, which is wrong, call 140 fix that.

            /*

            // TODO: Account actually calls nn::arp::detail::IReader::GetApplicationLaunchProperty() with the current PID and store the result (ApplicationLaunchProperty) internally.
            //       For now we can hardcode values, and fix it after GetApplicationLaunchProperty is implemented.
            if (nn::arp::detail::IReader::GetApplicationLaunchProperty() == 0xCC9D) // ResultCode.InvalidProcessId
            {
                return ResultCode.InvalidArgument;
            }

            */

            // TODO: Determine where ApplicationLaunchProperty is used.
            ApplicationLaunchProperty applicationLaunchProperty = ApplicationLaunchProperty.GetByPid(context);

            Logger.Stub?.PrintStub(LogClass.ServiceAcc, new { applicationLaunchProperty.TitleId });

            return ResultCode.Success;
        }

        [Command(101)]
        // GetBaasAccountManagerForApplication(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
        public ResultCode GetBaasAccountManagerForApplication(ServiceCtx context)
        {
            ResultCode resultCode = _applicationServiceServer.CheckUserId(context, out UserId userId);

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            MakeObject(context, new IManagerForApplication(userId));

            // Doesn't occur in our case.
            // return ResultCode.NullObject;

            return ResultCode.Success;
        }

        [Command(110)]
        // StoreSaveDataThumbnail(nn::account::Uid, buffer<bytes, 5>)
        public ResultCode StoreSaveDataThumbnail(ServiceCtx context)
        {
            return _applicationServiceServer.StoreSaveDataThumbnail(context);
        }

        [Command(111)]
        // ClearSaveDataThumbnail(nn::account::Uid)
        public ResultCode ClearSaveDataThumbnail(ServiceCtx context)
        {
            return _applicationServiceServer.ClearSaveDataThumbnail(context);
        }

        [Command(131)] // 6.0.0+
        // ListOpenContextStoredUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListOpenContextStoredUsers(ServiceCtx context)
        {
            long outputPosition = context.Request.RecvListBuff[0].Position;
            long outputSize     = context.Request.RecvListBuff[0].Size;

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            // TODO: This seems to write stored userids of the OpenContext in the buffer. We needs to determine them.
            
            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }

        [Command(141)] // 6.0.0+
        // ListQualifiedUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListQualifiedUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListQualifiedUsers(context);
        }

        [Command(150)] // 6.0.0+
        // IsUserAccountSwitchLocked() -> bool
        public ResultCode IsUserAccountSwitchLocked(ServiceCtx context)
        {
            // TODO: Account actually calls nn::arp::detail::IReader::GetApplicationControlProperty() with the current Pid and store the result (NACP file) internally.
            //       But since we use LibHac and we load one Application at a time, it's not necessary.

            context.ResponseData.Write(context.Device.Application.ControlData.Value.UserAccountSwitchLock);

            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }
    }
}