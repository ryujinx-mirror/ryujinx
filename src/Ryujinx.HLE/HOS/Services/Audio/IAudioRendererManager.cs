using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Services.Audio.AudioRenderer;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    interface IAudioRendererManager
    {
        // TODO: Remove ServiceCtx argument
        // BODY: This is only needed by the legacy backend. Refactor this when removing the legacy backend.
        ResultCode GetAudioDeviceServiceWithRevisionInfo(ServiceCtx context, out IAudioDevice outObject, int revision, ulong appletResourceUserId);

        // TODO: Remove ServiceCtx argument
        // BODY: This is only needed by the legacy backend. Refactor this when removing the legacy backend.
        ResultCode OpenAudioRenderer(ServiceCtx context, out IAudioRenderer obj, ref AudioRendererConfiguration parameter, ulong workBufferSize, ulong appletResourceUserId, KTransferMemory workBufferTransferMemory, uint processHandle);

        ulong GetWorkBufferSize(ref AudioRendererConfiguration parameter);
    }
}
