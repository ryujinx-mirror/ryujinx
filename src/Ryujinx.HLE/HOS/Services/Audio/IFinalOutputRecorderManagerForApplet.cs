namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("audrec:a")]
    class IFinalOutputRecorderManagerForApplet : IpcService
    {
        public IFinalOutputRecorderManagerForApplet(ServiceCtx context) { }
    }
}