using Ryujinx.HLE.HOS.Services.Nfc.NfcManager;

namespace Ryujinx.HLE.HOS.Services.Nfc
{
    [Service("nfc:sys")]
    class ISystemManager : IpcService
    {
        public ISystemManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateSystemInterface() -> object<nn::nfc::detail::ISystem>
        public ResultCode CreateSystemInterface(ServiceCtx context)
        {
            MakeObject(context, new INfc(NfcPermissionLevel.System));

            return ResultCode.Success;
        }
    }
}
