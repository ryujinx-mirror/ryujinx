namespace Ryujinx.HLE.HOS.Services.Sm
{
    [Service("spl:")]
    [Service("spl:es")]
    [Service("spl:fs")]
    [Service("spl:manu")]
    [Service("spl:mig")]
    [Service("spl:ssl")] 
    class IGeneralInterface : IpcService
    {
        public IGeneralInterface(ServiceCtx context) { }
    }
}