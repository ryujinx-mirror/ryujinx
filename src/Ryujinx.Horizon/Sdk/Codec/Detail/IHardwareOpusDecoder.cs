using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Codec.Detail
{
    interface IHardwareOpusDecoder : IServiceObject
    {
        Result DecodeInterleavedOld(out int outConsumed, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input);
        Result SetContext(ReadOnlySpan<byte> context);
        Result DecodeInterleavedForMultiStreamOld(out int outConsumed, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input);
        Result SetContextForMultiStream(ReadOnlySpan<byte> context);
        Result DecodeInterleavedWithPerfOld(out int outConsumed, out long timeTaken, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input);
        Result DecodeInterleavedForMultiStreamWithPerfOld(out int outConsumed, out long timeTaken, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input);
        Result DecodeInterleavedWithPerfAndResetOld(out int outConsumed, out long timeTaken, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input, bool reset);
        Result DecodeInterleavedForMultiStreamWithPerfAndResetOld(out int outConsumed, out long timeTaken, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input, bool reset);
        Result DecodeInterleaved(out int outConsumed, out long timeTaken, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input, bool reset);
        Result DecodeInterleavedForMultiStream(out int outConsumed, out long timeTaken, out int outSamples, Span<byte> output, ReadOnlySpan<byte> input, bool reset);
    }
}
