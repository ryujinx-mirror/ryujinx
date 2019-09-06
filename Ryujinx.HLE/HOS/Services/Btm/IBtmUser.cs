namespace Ryujinx.HLE.HOS.Services.Btm
{
    [Service("btm:u")]
    class IBtmUser : IpcService
    {
        public IBtmUser(ServiceCtx context) { }

        [Command(0)] // 5.0.0+
        // GetCore() -> object<nn::btm::IBtmUserCore>
        public ResultCode GetCore(ServiceCtx context)
        {
            MakeObject(context, new IBtmUserCore());

            return ResultCode.Success;
        }
    }
}