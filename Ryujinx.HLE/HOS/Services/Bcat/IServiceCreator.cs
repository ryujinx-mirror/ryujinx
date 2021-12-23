using LibHac;
using LibHac.Common;
using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator;
using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Bcat
{
    [Service("bcat:a", "bcat:a")]
    [Service("bcat:m", "bcat:m")]
    [Service("bcat:u", "bcat:u")]
    [Service("bcat:s", "bcat:s")]
    class IServiceCreator : DisposableIpcService
    {
        private SharedRef<LibHac.Bcat.Impl.Ipc.IServiceCreator> _base;

        public IServiceCreator(ServiceCtx context, string serviceName)
        {
            var applicationClient = context.Device.System.LibHacHorizonManager.ApplicationClient;
            applicationClient.Sm.GetService(ref _base, serviceName).ThrowIfFailure();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _base.Destroy();
            }
        }

        [CommandHipc(0)]
        // CreateBcatService(pid) -> object<nn::bcat::detail::ipc::IBcatService>
        public ResultCode CreateBcatService(ServiceCtx context)
        {
            // TODO: Call arp:r GetApplicationLaunchProperty with the pid to get the TitleId.
            //       Add an instance of nn::bcat::detail::service::core::PassphraseManager.
            //       Add an instance of nn::bcat::detail::service::ServiceMemoryManager.
            //       Add an instance of nn::bcat::detail::service::core::TaskManager who load "bcat-sys:/" system save data and open "dc/task.bin". 
            //       If the file don't exist, create a new one (size of 0x800) and write 2 empty struct with a size of 0x400.

            MakeObject(context, new IBcatService(ApplicationLaunchProperty.GetByPid(context)));

            // NOTE: If the IBcatService is null this error is returned, Doesn't occur in our case. 
            //       return ResultCode.NullObject;

            return ResultCode.Success;
        }

        [CommandHipc(1)]
        // CreateDeliveryCacheStorageService(pid) -> object<nn::bcat::detail::ipc::IDeliveryCacheStorageService>
        public ResultCode CreateDeliveryCacheStorageService(ServiceCtx context)
        {
            ulong pid = context.RequestData.ReadUInt64();

            using var serv = new SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService>();

            Result rc = _base.Get.CreateDeliveryCacheStorageService(ref serv.Ref(), pid);

            if (rc.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheStorageService(context, ref serv.Ref()));
            }

            return (ResultCode)rc.Value;
        }

        [CommandHipc(2)]
        // CreateDeliveryCacheStorageServiceWithApplicationId(nn::ApplicationId) -> object<nn::bcat::detail::ipc::IDeliveryCacheStorageService>
        public ResultCode CreateDeliveryCacheStorageServiceWithApplicationId(ServiceCtx context)
        {
            ApplicationId applicationId = context.RequestData.ReadStruct<ApplicationId>();

            using var service = new SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService>();

            Result rc = _base.Get.CreateDeliveryCacheStorageServiceWithApplicationId(ref service.Ref(), applicationId);

            if (rc.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheStorageService(context, ref service.Ref()));
            }

            return (ResultCode)rc.Value;
        }
    }
}
