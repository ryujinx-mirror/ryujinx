using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRenderer
{
    interface IAudioDevice
    {
        string[] ListAudioDeviceName();
        ResultCode SetAudioDeviceOutputVolume(string name, float volume);
        ResultCode GetAudioDeviceOutputVolume(string name,  out float volume);
        string GetActiveAudioDeviceName();
        KEvent QueryAudioDeviceSystemEvent();
        uint GetActiveChannelCount();
        KEvent QueryAudioDeviceInputEvent();
        KEvent QueryAudioDeviceOutputEvent();
        ResultCode GetAudioSystemMasterVolumeSetting(string name,  out float systemMasterVolume);
    }
}
