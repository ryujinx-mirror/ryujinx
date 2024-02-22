using Ryujinx.Audio.Common;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Applet;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    interface IAudioInManager : IServiceObject
    {
        Result ListAudioIns(out int count, Span<DeviceName> names);
        Result OpenAudioIn(
            out AudioOutputConfiguration outputConfig,
            out IAudioIn audioIn,
            Span<DeviceName> outName,
            AudioInputConfiguration parameter,
            AppletResourceUserId appletResourceId,
            int processHandle,
            ReadOnlySpan<DeviceName> name,
            ulong pid);
        Result ListAudioInsAuto(out int count, Span<DeviceName> names);
        Result OpenAudioInAuto(
            out AudioOutputConfiguration outputConfig,
            out IAudioIn audioIn,
            Span<DeviceName> outName,
            AudioInputConfiguration parameter,
            AppletResourceUserId appletResourceId,
            int processHandle,
            ReadOnlySpan<DeviceName> name,
            ulong pid);
        Result ListAudioInsAutoFiltered(out int count, Span<DeviceName> names);
        Result OpenAudioInProtocolSpecified(
            out AudioOutputConfiguration outputConfig,
            out IAudioIn audioIn,
            Span<DeviceName> outName,
            AudioInProtocol protocol,
            AudioInputConfiguration parameter,
            AppletResourceUserId appletResourceId,
            int processHandle,
            ReadOnlySpan<DeviceName> name,
            ulong pid);
    }
}
