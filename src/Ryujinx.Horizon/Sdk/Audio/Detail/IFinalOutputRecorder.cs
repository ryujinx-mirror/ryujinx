using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    interface IFinalOutputRecorder : IServiceObject
    {
        Result GetFinalOutputRecorderState(out uint state);
        Result Start();
        Result Stop();
        Result AppendFinalOutputRecorderBuffer(ReadOnlySpan<byte> buffer, ulong bufferClientPtr);
        Result RegisterBufferEvent(out int eventHandle);
        Result GetReleasedFinalOutputRecorderBuffers(Span<byte> buffer, out uint count, out ulong released);
        Result ContainsFinalOutputRecorderBuffer(ulong bufferPointer, out bool contains);
        Result GetFinalOutputRecorderBufferEndTime(ulong bufferPointer, out ulong released);
        Result AppendFinalOutputRecorderBufferAuto(ReadOnlySpan<byte> buffer, ulong bufferClientPtr);
        Result GetReleasedFinalOutputRecorderBuffersAuto(Span<byte> buffer, out uint count, out ulong released);
        Result FlushFinalOutputRecorderBuffers(out bool pending);
        Result AttachWorkBuffer(FinalOutputRecorderParameterInternal parameter);
    }
}
