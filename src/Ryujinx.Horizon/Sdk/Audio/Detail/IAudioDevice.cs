using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    interface IAudioDevice : IServiceObject
    {
        Result ListAudioDeviceName(Span<DeviceName> names, out int nameCount);
        Result SetAudioDeviceOutputVolume(ReadOnlySpan<DeviceName> name, float volume);
        Result GetAudioDeviceOutputVolume(ReadOnlySpan<DeviceName> name, out float volume);
        Result GetActiveAudioDeviceName(Span<DeviceName> name);
        Result QueryAudioDeviceSystemEvent(out int eventHandle);
        Result GetActiveChannelCount(out int channelCount);
        Result ListAudioDeviceNameAuto(Span<DeviceName> names, out int nameCount);
        Result SetAudioDeviceOutputVolumeAuto(ReadOnlySpan<DeviceName> name, float volume);
        Result GetAudioDeviceOutputVolumeAuto(ReadOnlySpan<DeviceName> name, out float volume);
        Result GetActiveAudioDeviceNameAuto(Span<DeviceName> name);
        Result QueryAudioDeviceInputEvent(out int eventHandle);
        Result QueryAudioDeviceOutputEvent(out int eventHandle);
        Result GetActiveAudioOutputDeviceName(Span<DeviceName> name);
        Result ListAudioOutputDeviceName(Span<DeviceName> names, out int nameCount);
    }
}
