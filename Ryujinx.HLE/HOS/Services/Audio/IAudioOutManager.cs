using Ryujinx.Audio.Common;
using Ryujinx.HLE.HOS.Services.Audio.AudioOut;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    interface IAudioOutManager
    {
        public string[] ListAudioOuts();

        public ResultCode OpenAudioOut(ServiceCtx context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioOut obj, string inputDeviceName, ref AudioInputConfiguration parameter, ulong appletResourceUserId, uint processHandle);
    }
}
