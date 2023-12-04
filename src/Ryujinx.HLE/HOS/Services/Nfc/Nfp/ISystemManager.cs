using Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    [Service("nfp:sys")]
    class ISystemManager : IpcService
    {
        public ISystemManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateSystemInterface() -> object<nn::nfp::detail::ISystem>
        public ResultCode CreateSystemInterface(ServiceCtx context)
        {
            MakeObject(context, new INfp(NfpPermissionLevel.System));

            return ResultCode.Success;
        }
    }
}
