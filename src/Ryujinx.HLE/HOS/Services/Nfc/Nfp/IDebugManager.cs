using Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    [Service("nfp:dbg")]
    class IAmManager : IpcService
    {
        public IAmManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateDebugInterface() -> object<nn::nfp::detail::IDebug>
        public ResultCode CreateDebugInterface(ServiceCtx context)
        {
            MakeObject(context, new INfp(NfpPermissionLevel.Debug));

            return ResultCode.Success;
        }
    }
}
