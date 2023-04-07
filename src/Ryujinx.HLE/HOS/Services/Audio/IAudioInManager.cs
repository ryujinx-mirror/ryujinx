using Ryujinx.Audio.Common;
using Ryujinx.HLE.HOS.Services.Audio.AudioIn;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    interface IAudioInManager
    {
        public string[] ListAudioIns(bool filtered);

        public ResultCode OpenAudioIn(ServiceCtx context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, string inputDeviceName, ref AudioInputConfiguration parameter, ulong appletResourceUserId, uint processHandle);
    }
}