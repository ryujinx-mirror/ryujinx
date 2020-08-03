using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface.ShopServiceAccessServer;

namespace Ryujinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface
{
    class IShopServiceAccessServer : IpcService
    {
        public IShopServiceAccessServer() { }

        [Command(0)]
        // CreateAccessorInterface(u8) -> object<nn::ec::IShopServiceAccessor>
        public ResultCode CreateAccessorInterface(ServiceCtx context)
        {
            MakeObject(context, new IShopServiceAccessor(context.Device.System));

            Logger.Stub?.PrintStub(LogClass.ServiceNim);

            return ResultCode.Success;
        }
    }
}