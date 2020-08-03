using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface;

namespace Ryujinx.HLE.HOS.Services.Nim
{
    [Service("nim:eca")] // 5.0.0+
    class IShopServiceAccessServerInterface : IpcService
    {
        public IShopServiceAccessServerInterface(ServiceCtx context) { }

        [Command(0)]
        // CreateServerInterface(pid, handle<unknown>, u64) -> object<nn::ec::IShopServiceAccessServer>
        public ResultCode CreateServerInterface(ServiceCtx context)
        {
            MakeObject(context, new IShopServiceAccessServer());

            Logger.Stub?.PrintStub(LogClass.ServiceNim);

            return ResultCode.Success;
        }
    }
}