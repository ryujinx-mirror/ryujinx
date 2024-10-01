using Ryujinx.Audio.Common;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    interface IAudioOut : IServiceObject
    {
        Result GetAudioOutState(out AudioDeviceState state);
        Result Start();
        Result Stop();
        Result AppendAudioOutBuffer(ulong bufferTag, ReadOnlySpan<AudioUserBuffer> buffer);
        Result RegisterBufferEvent(out int eventHandle);
        Result GetReleasedAudioOutBuffers(out uint count, Span<ulong> bufferTags);
        Result ContainsAudioOutBuffer(out bool contains, ulong bufferTag);
        Result AppendAudioOutBufferAuto(ulong bufferTag, ReadOnlySpan<AudioUserBuffer> buffer);
        Result GetReleasedAudioOutBuffersAuto(out uint count, Span<ulong> bufferTags);
        Result GetAudioOutBufferCount(out uint bufferCount);
        Result GetAudioOutPlayedSampleCount(out ulong sampleCount);
        Result FlushAudioOutBuffers(out bool pending);
        Result SetAudioOutVolume(float volume);
        Result GetAudioOutVolume(out float volume);
    }
}
