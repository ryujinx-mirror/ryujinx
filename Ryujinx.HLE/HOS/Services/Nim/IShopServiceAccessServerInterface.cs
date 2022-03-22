using LibHac.Ncm;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Arp;
using Ryujinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface;

namespace Ryujinx.HLE.HOS.Services.Nim
{
    [Service("nim:eca")] // 5.0.0+
    class IShopServiceAccessServerInterface : IpcService
    {
        public IShopServiceAccessServerInterface(ServiceCtx context) { }

        [CommandHipc(0)]
        // CreateServerInterface(pid, handle<unknown>, u64) -> object<nn::ec::IShopServiceAccessServer>
        public ResultCode CreateServerInterface(ServiceCtx context)
        {
            MakeObject(context, new IShopServiceAccessServer());

            Logger.Stub?.PrintStub(LogClass.ServiceNim);

            return ResultCode.Success;
        }

        [CommandHipc(4)] // 10.0.0+
        // IsLargeResourceAvailable(pid) -> b8
        public ResultCode IsLargeResourceAvailable(ServiceCtx context)
        {
            // TODO: Service calls arp:r GetApplicationInstanceId (10.0.0+) then if it fails it calls arp:r GetMicroApplicationInstanceId (10.0.0+)
            //       then if it fails it returns the arp:r result code.

            // NOTE: Firmare 10.0.0+ don't use the Pid here anymore, but the returned InstanceId. We don't support that for now so we can just use the Pid instead.
            StorageId baseStorageId = (StorageId)ApplicationLaunchProperty.GetByPid(context).BaseGameStorageId;

            // NOTE: Service returns ResultCode.InvalidArgument if baseStorageId is null, doesn't occur in our case.

            context.ResponseData.Write(baseStorageId == StorageId.Host);

            return ResultCode.Success;
        }
    }
}