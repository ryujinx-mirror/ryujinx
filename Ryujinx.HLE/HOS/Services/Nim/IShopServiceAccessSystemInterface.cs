namespace Ryujinx.HLE.HOS.Services.Ldr
{
    [Service("nim:ecas")] // 7.0.0+
    class IShopServiceAccessSystemInterface : IpcService
    {
        public IShopServiceAccessSystemInterface(ServiceCtx context) { }
    }
}