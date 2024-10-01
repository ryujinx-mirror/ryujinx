using Ryujinx.Audio.Common;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    interface IAudioIn : IServiceObject
    {
        Result GetAudioInState(out AudioDeviceState state);
        Result Start();
        Result Stop();
        Result AppendAudioInBuffer(ulong bufferTag, ReadOnlySpan<AudioUserBuffer> buffer);
        Result RegisterBufferEvent(out int eventHandle);
        Result GetReleasedAudioInBuffers(out uint count, Span<ulong> bufferTags);
        Result ContainsAudioInBuffer(out bool contains, ulong bufferTag);
        Result AppendUacInBuffer(ulong bufferTag, ReadOnlySpan<AudioUserBuffer> buffer, int eventHandle);
        Result AppendAudioInBufferAuto(ulong bufferTag, ReadOnlySpan<AudioUserBuffer> buffer);
        Result GetReleasedAudioInBuffersAuto(out uint count, Span<ulong> bufferTags);
        Result AppendUacInBufferAuto(ulong bufferTag, ReadOnlySpan<AudioUserBuffer> buffer, int eventHandle);
        Result GetAudioInBufferCount(out uint bufferCount);
        Result SetDeviceGain(float gain);
        Result GetDeviceGain(out float gain);
        Result FlushAudioInBuffers(out bool pending);
    }
}
