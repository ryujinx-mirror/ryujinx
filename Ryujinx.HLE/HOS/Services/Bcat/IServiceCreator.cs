using LibHac;
using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator;
using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Bcat
{
    [Service("bcat:a", "bcat:a")]
    [Service("bcat:m", "bcat:m")]
    [Service("bcat:u", "bcat:u")]
    [Service("bcat:s", "bcat:s")]
    class IServiceCreator : IpcService
    {
        private LibHac.Bcat.Detail.Ipc.IServiceCreator _base;

        public IServiceCreator(ServiceCtx context, string serviceName)
        {
            context.Device.System.LibHacHorizonClient.Sm.GetService(out _base, serviceName).ThrowIfFailure();
        }

        [Command(0)]
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

        [Command(1)]
        // CreateDeliveryCacheStorageService(pid) -> object<nn::bcat::detail::ipc::IDeliveryCacheStorageService>
        public ResultCode CreateDeliveryCacheStorageService(ServiceCtx context)
        {
            ulong pid = context.RequestData.ReadUInt64();

            Result rc = _base.CreateDeliveryCacheStorageService(out LibHac.Bcat.Detail.Ipc.IDeliveryCacheStorageService serv, pid);

            if (rc.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheStorageService(context, serv));
            }

            return (ResultCode)rc.Value;
        }

        [Command(2)]
        // CreateDeliveryCacheStorageServiceWithApplicationId(nn::ApplicationId) -> object<nn::bcat::detail::ipc::IDeliveryCacheStorageService>
        public ResultCode CreateDeliveryCacheStorageServiceWithApplicationId(ServiceCtx context)
        {
            ApplicationId applicationId = context.RequestData.ReadStruct<ApplicationId>();

            Result rc = _base.CreateDeliveryCacheStorageServiceWithApplicationId(out LibHac.Bcat.Detail.Ipc.IDeliveryCacheStorageService serv,
               applicationId);

            if (rc.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheStorageService(context, serv));
            }

            return (ResultCode)rc.Value;
        }
    }
}
