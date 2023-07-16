using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Account.Acc.AccountService;
using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    [Service("acc:u0", AccountServiceFlag.Application)] // Max Sessions: 4
    class IAccountServiceForApplication : IpcService
    {
        private readonly ApplicationServiceServer _applicationServiceServer;

        public IAccountServiceForApplication(ServiceCtx context, AccountServiceFlag serviceFlag)
        {
            _applicationServiceServer = new ApplicationServiceServer(serviceFlag);
        }

        [CommandCmif(0)]
        // GetUserCount() -> i32
        public ResultCode GetUserCount(ServiceCtx context)
        {
            return _applicationServiceServer.GetUserCountImpl(context);
        }

        [CommandCmif(1)]
        // GetUserExistence(nn::account::Uid) -> bool
        public ResultCode GetUserExistence(ServiceCtx context)
        {
            return _applicationServiceServer.GetUserExistenceImpl(context);
        }

        [CommandCmif(2)]
        // ListAllUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListAllUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListAllUsers(context);
        }

        [CommandCmif(3)]
        // ListOpenUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListOpenUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListOpenUsers(context);
        }

        [CommandCmif(4)]
        // GetLastOpenedUser() -> nn::account::Uid
        public ResultCode GetLastOpenedUser(ServiceCtx context)
        {
            return _applicationServiceServer.GetLastOpenedUser(context);
        }

        [CommandCmif(5)]
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

        [CommandCmif(50)]
        // IsUserRegistrationRequestPermitted(pid) -> bool
        public ResultCode IsUserRegistrationRequestPermitted(ServiceCtx context)
        {
            // NOTE: pid is unused.
            return _applicationServiceServer.IsUserRegistrationRequestPermitted(context);
        }

        [CommandCmif(51)]
        // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
        public ResultCode TrySelectUserWithoutInteraction(ServiceCtx context)
        {
            return _applicationServiceServer.TrySelectUserWithoutInteraction(context);
        }

        [CommandCmif(100)]
        [CommandCmif(140)] // 6.0.0+
        [CommandCmif(160)] // 13.0.0+
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

        [CommandCmif(101)]
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

        [CommandCmif(103)] // 4.0.0+
        // CheckNetworkServiceAvailabilityAsync() -> object<nn::account::detail::IAsyncContext>
        public ResultCode CheckNetworkServiceAvailabilityAsync(ServiceCtx context)
        {
            ResultCode resultCode = _applicationServiceServer.CheckNetworkServiceAvailabilityAsync(context, out IAsyncContext asyncContext);

            if (resultCode == ResultCode.Success)
            {
                MakeObject(context, asyncContext);
            }

            return resultCode;
        }

        [CommandCmif(110)]
        // StoreSaveDataThumbnail(nn::account::Uid, buffer<bytes, 5>)
        public ResultCode StoreSaveDataThumbnail(ServiceCtx context)
        {
            return _applicationServiceServer.StoreSaveDataThumbnail(context);
        }

        [CommandCmif(111)]
        // ClearSaveDataThumbnail(nn::account::Uid)
        public ResultCode ClearSaveDataThumbnail(ServiceCtx context)
        {
            return _applicationServiceServer.ClearSaveDataThumbnail(context);
        }

        [CommandCmif(130)] // 5.0.0+
        // LoadOpenContext(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
        public ResultCode LoadOpenContext(ServiceCtx context)
        {
            ResultCode resultCode = _applicationServiceServer.CheckUserId(context, out UserId userId);

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            MakeObject(context, new IManagerForApplication(userId));

            return ResultCode.Success;
        }

        [CommandCmif(60)] // 5.0.0-5.1.0
        [CommandCmif(131)] // 6.0.0+
        // ListOpenContextStoredUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListOpenContextStoredUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListOpenContextStoredUsers(context);
        }

        [CommandCmif(141)] // 6.0.0+
        // ListQualifiedUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListQualifiedUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListQualifiedUsers(context);
        }

        [CommandCmif(150)] // 6.0.0+
        // IsUserAccountSwitchLocked() -> bool
        public ResultCode IsUserAccountSwitchLocked(ServiceCtx context)
        {
            // TODO: Account actually calls nn::arp::detail::IReader::GetApplicationControlProperty() with the current Pid and store the result (NACP file) internally.
            //       But since we use LibHac and we load one Application at a time, it's not necessary.

            context.ResponseData.Write((byte)context.Device.Processes.ActiveApplication.ApplicationControlProperties.UserAccountSwitchLock);

            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }
    }
}
