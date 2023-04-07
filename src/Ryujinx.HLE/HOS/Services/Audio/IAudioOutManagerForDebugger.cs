namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("audout:d")]
    class IAudioOutManagerForDebugger : IpcService
    {
        public IAudioOutManagerForDebugger(ServiceCtx context) { }
    }
}