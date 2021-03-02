//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Common.Logging;
using Ryujinx.Memory;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;

namespace Ryujinx.Audio.Renderer.Dsp
{
    public static class DataSourceHelper
    {
        private const int FixedPointPrecision = 15;

        public class WaveBufferInformation
        {
            public Memory<VoiceUpdateState> State;
            public uint SourceSampleRate;
            public SampleFormat SampleFormat;
            public float Pitch;
            public DecodingBehaviour DecodingBehaviour;
            public WaveBuffer[] WaveBuffers;
            public ulong ExtraParameter;
            public ulong ExtraParameterSize;
            public int ChannelIndex;
            public int ChannelCount;
            public SampleRateConversionQuality SrcQuality;
        }

        private static int GetPitchLimitBySrcQuality(SampleRateConversionQuality quality)
        {
            switch (quality)
            {
                case SampleRateConversionQuality.Default:
                case SampleRateConversionQuality.Low:
                    return 4;
                case SampleRateConversionQuality.High:
                    return 8;
                default:
                    throw new ArgumentException($"{quality}");
            }
        }

        public static void ProcessWaveBuffers(IVirtualMemoryManager memoryManager, Span<float> outputBuffer, WaveBufferInformation info, uint targetSampleRate, int sampleCount)
        {
            const int tempBufferSize = 0x3F00;

            ref VoiceUpdateState state = ref info.State.Span[0];

            short[] tempBuffer = ArrayPool<short>.Shared.Rent(tempBufferSize);

            float sampleRateRatio = ((float)info.SourceSampleRate / targetSampleRate * info.Pitch);

            float fraction = state.Fraction;
            int waveBufferIndex = (int)state.WaveBufferIndex;
            ulong playedSampleCount = state.PlayedSampleCount;
            int offset = state.Offset;
            uint waveBufferConsumed = state.WaveBufferConsumed;

            int pitchMaxLength = GetPitchLimitBySrcQuality(info.SrcQuality);

            int totalNeededSize = (int)MathF.Truncate(fraction + sampleRateRatio * sampleCount);

            if (totalNeededSize + pitchMaxLength <= tempBufferSize && totalNeededSize >= 0)
            {
                int sourceSampleCountToProcess = sampleCount;

                int maxSampleCountPerIteration = Math.Min((int)MathF.Truncate((tempBufferSize - fraction) / sampleRateRatio), sampleCount);

                bool isStarving = false;

                int i = 0;

                while (i < sourceSampleCountToProcess)
                {
                    int tempBufferIndex = 0;

                    if (!info.DecodingBehaviour.HasFlag(DecodingBehaviour.SkipPitchAndSampleRateConversion))
                    {
                        state.Pitch.ToSpan().Slice(0, pitchMaxLength).CopyTo(tempBuffer.AsSpan());
                        tempBufferIndex += pitchMaxLength;
                    }

                    int sampleCountToProcess = Math.Min(sourceSampleCountToProcess, maxSampleCountPerIteration);

                    int y = 0;

                    int sampleCountToDecode = (int)MathF.Truncate(fraction + sampleRateRatio * sampleCountToProcess);

                    while (y < sampleCountToDecode)
                    {
                        if (waveBufferIndex >= Constants.VoiceWaveBufferCount)
                        {
                            Logger.Error?.Print(LogClass.AudioRenderer, $"Invalid WaveBuffer index {waveBufferIndex}");

                            waveBufferIndex = 0;
                            playedSampleCount = 0;
                        }

                        if (!state.IsWaveBufferValid[waveBufferIndex])
                        {
                            isStarving = true;
                            break;
                        }

                        ref WaveBuffer waveBuffer = ref info.WaveBuffers[waveBufferIndex];

                        if (offset == 0 && info.SampleFormat == SampleFormat.Adpcm && waveBuffer.Context != 0)
                        {
                            state.LoopContext = memoryManager.Read<AdpcmLoopContext>(waveBuffer.Context);
                        }

                        Span<short> tempSpan = tempBuffer.AsSpan().Slice(tempBufferIndex + y);

                        int decodedSampleCount = -1;

                        int targetSampleStartOffset;
                        int targetSampleEndOffset;

                        if (state.LoopCount > 0 && waveBuffer.LoopStartSampleOffset != 0 && waveBuffer.LoopEndSampleOffset != 0 && waveBuffer.LoopStartSampleOffset <= waveBuffer.LoopEndSampleOffset)
                        {
                            targetSampleStartOffset = (int)waveBuffer.LoopStartSampleOffset;
                            targetSampleEndOffset = (int)waveBuffer.LoopEndSampleOffset;
                        }
                        else
                        {
                            targetSampleStartOffset = (int)waveBuffer.StartSampleOffset;
                            targetSampleEndOffset = (int)waveBuffer.EndSampleOffset;
                        }

                        int targetWaveBufferSampleCount = targetSampleEndOffset - targetSampleStartOffset;

                        switch (info.SampleFormat)
                        {
                            case SampleFormat.Adpcm:
                                ReadOnlySpan<byte> waveBufferAdpcm = ReadOnlySpan<byte>.Empty;

                                if (waveBuffer.Buffer != 0 && waveBuffer.BufferSize != 0)
                                {
                                    // TODO: we are possibly copying a lot of unneeded data here, we should only take what we need.
                                    waveBufferAdpcm = memoryManager.GetSpan(waveBuffer.Buffer, (int)waveBuffer.BufferSize);
                                }

                                ReadOnlySpan<short> coefficients = MemoryMarshal.Cast<byte, short>(memoryManager.GetSpan(info.ExtraParameter, (int)info.ExtraParameterSize));
                                decodedSampleCount = AdpcmHelper.Decode(tempSpan, waveBufferAdpcm, targetSampleStartOffset, targetSampleEndOffset, offset, sampleCountToDecode - y, coefficients, ref state.LoopContext);
                                break;
                            case SampleFormat.PcmInt16:
                                ReadOnlySpan<short> waveBufferPcm16 = ReadOnlySpan<short>.Empty;

                                if (waveBuffer.Buffer != 0 && waveBuffer.BufferSize != 0)
                                {
                                    ulong bufferOffset = waveBuffer.Buffer + PcmHelper.GetBufferOffset<short>(targetSampleStartOffset, offset, info.ChannelCount);
                                    int bufferSize = PcmHelper.GetBufferSize<short>(targetSampleStartOffset, targetSampleEndOffset, offset, sampleCountToDecode - y) * info.ChannelCount;

                                    waveBufferPcm16 = MemoryMarshal.Cast<byte, short>(memoryManager.GetSpan(bufferOffset, bufferSize));
                                }

                                decodedSampleCount = PcmHelper.Decode(tempSpan, waveBufferPcm16, targetSampleStartOffset, targetSampleEndOffset, info.ChannelIndex, info.ChannelCount);
                                break;
                            case SampleFormat.PcmFloat:
                                ReadOnlySpan<float> waveBufferPcmFloat = ReadOnlySpan<float>.Empty;

                                if (waveBuffer.Buffer != 0 && waveBuffer.BufferSize != 0)
                                {
                                    ulong bufferOffset = waveBuffer.Buffer + PcmHelper.GetBufferOffset<float>(targetSampleStartOffset, offset, info.ChannelCount);
                                    int bufferSize = PcmHelper.GetBufferSize<float>(targetSampleStartOffset, targetSampleEndOffset, offset, sampleCountToDecode - y) * info.ChannelCount;

                                    waveBufferPcmFloat = MemoryMarshal.Cast<byte, float>(memoryManager.GetSpan(bufferOffset, bufferSize));
                                }

                                decodedSampleCount = PcmHelper.Decode(tempSpan, waveBufferPcmFloat, targetSampleStartOffset, targetSampleEndOffset, info.ChannelIndex, info.ChannelCount);
                                break;
                            default:
                                Logger.Warning?.Print(LogClass.AudioRenderer, $"Unsupported sample format {info.SampleFormat}");
                                break;
                        }

                        Debug.Assert(decodedSampleCount <= sampleCountToDecode);

                        if (decodedSampleCount < 0)
                        {
                            Logger.Warning?.Print(LogClass.AudioRenderer, $"Decoding failed, skipping WaveBuffer");

                            state.MarkEndOfBufferWaveBufferProcessing(ref waveBuffer, ref waveBufferIndex, ref waveBufferConsumed, ref playedSampleCount);
                            decodedSampleCount = 0;
                        }

                        y += decodedSampleCount;
                        offset += decodedSampleCount;
                        playedSampleCount += (uint)decodedSampleCount;

                        if (offset >= targetWaveBufferSampleCount || decodedSampleCount == 0)
                        {
                            offset = 0;

                            if (waveBuffer.Looping)
                            {
                                state.LoopCount++;

                                if (waveBuffer.LoopCount >= 0)
                                {
                                    if (decodedSampleCount == 0 || state.LoopCount > waveBuffer.LoopCount)
                                    {
                                        state.MarkEndOfBufferWaveBufferProcessing(ref waveBuffer, ref waveBufferIndex, ref waveBufferConsumed, ref playedSampleCount);
                                    }
                                }

                                if (decodedSampleCount == 0)
                                {
                                    isStarving = true;
                                    break;
                                }

                                if (info.DecodingBehaviour.HasFlag(DecodingBehaviour.PlayedSampleCountResetWhenLooping))
                                {
                                    playedSampleCount = 0;
                                }
                            }
                            else
                            {
                                state.MarkEndOfBufferWaveBufferProcessing(ref waveBuffer, ref waveBufferIndex, ref waveBufferConsumed, ref playedSampleCount);
                            }
                        }
                    }

                    Span<float> outputSpan = outputBuffer.Slice(i);
                    Span<int> outputSpanInt = MemoryMarshal.Cast<float, int>(outputSpan);

                    if (info.DecodingBehaviour.HasFlag(DecodingBehaviour.SkipPitchAndSampleRateConversion))
                    {
                        for (int j = 0; j < y; j++)
                        {
                            outputBuffer[j] = tempBuffer[j];
                        }
                    }
                    else
                    {
                        Span<short> tempSpan = tempBuffer.AsSpan().Slice(tempBufferIndex + y);

                        tempSpan.Slice(0, sampleCountToDecode - y).Fill(0);

                        ToFloat(outputBuffer, outputSpanInt, sampleCountToProcess);

                        ResamplerHelper.Resample(outputBuffer, tempBuffer, sampleRateRatio, ref fraction, sampleCountToProcess, info.SrcQuality, y != sourceSampleCountToProcess || info.Pitch != 1.0f);

                        tempBuffer.AsSpan().Slice(sampleCountToDecode, pitchMaxLength).CopyTo(state.Pitch.ToSpan());
                    }

                    i += sampleCountToProcess;
                }

                Debug.Assert(sourceSampleCountToProcess == i || !isStarving);

                state.WaveBufferConsumed = waveBufferConsumed;
                state.Offset = offset;
                state.PlayedSampleCount = playedSampleCount;
                state.WaveBufferIndex = (uint)waveBufferIndex;
                state.Fraction = fraction;
            }

            ArrayPool<short>.Shared.Return(tempBuffer);
        }

        private static void ToFloatAvx(Span<float> output, ReadOnlySpan<int> input, int sampleCount)
        {
            ReadOnlySpan<Vector256<int>> inputVec = MemoryMarshal.Cast<int, Vector256<int>>(input);
            Span<Vector256<float>> outputVec = MemoryMarshal.Cast<float, Vector256<float>>(output);

            int sisdStart = inputVec.Length * 8;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Avx.ConvertToVector256Single(inputVec[i]);
            }

            for (int i = sisdStart; i < sampleCount; i++)
            {
                output[i] = input[i];
            }
        }

        private static void ToFloatSse2(Span<float> output, ReadOnlySpan<int> input, int sampleCount)
        {
            ReadOnlySpan<Vector128<int>> inputVec = MemoryMarshal.Cast<int, Vector128<int>>(input);
            Span<Vector128<float>> outputVec = MemoryMarshal.Cast<float, Vector128<float>>(output);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Sse2.ConvertToVector128Single(inputVec[i]);
            }

            for (int i = sisdStart; i < sampleCount; i++)
            {
                output[i] = input[i];
            }
        }

        private static void ToFloatAdvSimd(Span<float> output, ReadOnlySpan<int> input, int sampleCount)
        {
            ReadOnlySpan<Vector128<int>> inputVec = MemoryMarshal.Cast<int, Vector128<int>>(input);
            Span<Vector128<float>> outputVec = MemoryMarshal.Cast<float, Vector128<float>>(output);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = AdvSimd.ConvertToSingle(inputVec[i]);
            }

            for (int i = sisdStart; i < sampleCount; i++)
            {
                output[i] = input[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToFloatSlow(Span<float> output, ReadOnlySpan<int> input, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                output[i] = input[i];
            }
        }

        public static void ToFloat(Span<float> output, ReadOnlySpan<int> input, int sampleCount)
        {
            if (Avx.IsSupported)
            {
                ToFloatAvx(output, input, sampleCount);
            }
            else if (Sse2.IsSupported)
            {
                ToFloatSse2(output, input, sampleCount);
            }
            else if (AdvSimd.IsSupported)
            {
                ToFloatAdvSimd(output, input, sampleCount);
            }
            else
            {
                ToFloatSlow(output, input, sampleCount);
            }
        }

        public static void ToIntAvx(Span<int> output, ReadOnlySpan<float> input, int sampleCount)
        {
            ReadOnlySpan<Vector256<float>> inputVec = MemoryMarshal.Cast<float, Vector256<float>>(input);
            Span<Vector256<int>> outputVec = MemoryMarshal.Cast<int, Vector256<int>>(output);

            int sisdStart = inputVec.Length * 8;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Avx.ConvertToVector256Int32(inputVec[i]);
            }

            for (int i = sisdStart; i < sampleCount; i++)
            {
                output[i] = (int)input[i];
            }
        }

        public static void ToIntSse2(Span<int> output, ReadOnlySpan<float> input, int sampleCount)
        {
            ReadOnlySpan<Vector128<float>> inputVec = MemoryMarshal.Cast<float, Vector128<float>>(input);
            Span<Vector128<int>> outputVec = MemoryMarshal.Cast<int, Vector128<int>>(output);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Sse2.ConvertToVector128Int32(inputVec[i]);
            }

            for (int i = sisdStart; i < sampleCount; i++)
            {
                output[i] = (int)input[i];
            }
        }

        public static void ToIntAdvSimd(Span<int> output, ReadOnlySpan<float> input, int sampleCount)
        {
            ReadOnlySpan<Vector128<float>> inputVec = MemoryMarshal.Cast<float, Vector128<float>>(input);
            Span<Vector128<int>> outputVec = MemoryMarshal.Cast<int, Vector128<int>>(output);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = AdvSimd.ConvertToInt32RoundToZero(inputVec[i]);
            }

            for (int i = sisdStart; i < sampleCount; i++)
            {
                output[i] = (int)input[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToIntSlow(Span<int> output, ReadOnlySpan<float> input, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                output[i] = (int)input[i];
            }
        }

        public static void ToInt(Span<int> output, ReadOnlySpan<float> input, int sampleCount)
        {
            if (Avx.IsSupported)
            {
                ToIntAvx(output, input, sampleCount);
            }
            else if (Sse2.IsSupported)
            {
                ToIntSse2(output, input, sampleCount);
            }
            else if (AdvSimd.IsSupported)
            {
                ToIntAdvSimd(output, input, sampleCount);
            }
            else
            {
                ToIntSlow(output, input, sampleCount);
            }
        }
    }
}
