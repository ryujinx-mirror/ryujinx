using Ryujinx.HLE.HOS.Services.BluetoothManager.BtmUser;

namespace Ryujinx.HLE.HOS.Services.BluetoothManager
{
    [Service("btm:u")] // 5.0.0+
    class IBtmUser : IpcService
    {
        public IBtmUser(ServiceCtx context) { }

        [CommandCmif(0)] // 5.0.0+
        // GetCore() -> object<nn::btm::IBtmUserCore>
        public ResultCode GetCore(ServiceCtx context)
        {
            MakeObject(context, new IBtmUserCore());

            return ResultCode.Success;
        }
    }
}
