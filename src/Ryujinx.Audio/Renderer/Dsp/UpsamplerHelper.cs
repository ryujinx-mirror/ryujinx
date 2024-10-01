using Ryujinx.Audio.Renderer.Server.Upsampler;
using Ryujinx.Common.Memory;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp
{
    public class UpsamplerHelper
    {
        private const int HistoryLength = UpsamplerBufferState.HistoryLength;
        private const int FilterBankLength = 20;
        // Bank0 = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        private const int Bank0CenterIndex = 9;
        private static readonly Array20<float> _bank1 = PrecomputeFilterBank(1.0f / 6.0f);
        private static readonly Array20<float> _bank2 = PrecomputeFilterBank(2.0f / 6.0f);
        private static readonly Array20<float> _bank3 = PrecomputeFilterBank(3.0f / 6.0f);
        private static readonly Array20<float> _bank4 = PrecomputeFilterBank(4.0f / 6.0f);
        private static readonly Array20<float> _bank5 = PrecomputeFilterBank(5.0f / 6.0f);

        private static Array20<float> PrecomputeFilterBank(float offset)
        {
            float Sinc(float x)
            {
                if (x == 0)
                {
                    return 1.0f;
                }
                return (MathF.Sin(MathF.PI * x) / (MathF.PI * x));
            }

            float BlackmanWindow(float x)
            {
                const float A = 0.18f;
                const float A0 = 0.5f - 0.5f * A;
                const float A1 = -0.5f;
                const float A2 = 0.5f * A;
                return A0 + A1 * MathF.Cos(2 * MathF.PI * x) + A2 * MathF.Cos(4 * MathF.PI * x);
            }

            Array20<float> result = new();

            for (int i = 0; i < FilterBankLength; i++)
            {
                float x = (Bank0CenterIndex - i) + offset;
                result[i] = Sinc(x) * BlackmanWindow(x / FilterBankLength + 0.5f);
            }

            return result;
        }

        // Polyphase upsampling algorithm
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Upsample(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, int outputSampleCount, int inputSampleCount, ref UpsamplerBufferState state)
        {
            if (!state.Initialized)
            {
                state.Scale = inputSampleCount switch
                {
                    40 => 6.0f,
                    80 => 3.0f,
                    160 => 1.5f,
                    _ => throw new ArgumentOutOfRangeException(nameof(inputSampleCount), inputSampleCount, null),
                };
                state.Initialized = true;
            }

            if (outputSampleCount == 0)
            {
                return;
            }

            float DoFilterBank(ref UpsamplerBufferState state, in Array20<float> bank)
            {
                float result = 0.0f;

                Debug.Assert(state.History.Length == HistoryLength);
                Debug.Assert(bank.Length == FilterBankLength);

                int curIdx = 0;
                if (Vector.IsHardwareAccelerated)
                {
                    // Do SIMD-accelerated block operations where possible.
                    // Only about a 2x speedup since filter bank length is short
                    int stopIdx = FilterBankLength - (FilterBankLength % Vector<float>.Count);
                    while (curIdx < stopIdx)
                    {
                        result += Vector.Dot(
                            new Vector<float>(bank.AsSpan().Slice(curIdx, Vector<float>.Count)),
                            new Vector<float>(state.History.AsSpan().Slice(curIdx, Vector<float>.Count)));
                        curIdx += Vector<float>.Count;
                    }
                }

                while (curIdx < FilterBankLength)
                {
                    result += bank[curIdx] * state.History[curIdx];
                    curIdx++;
                }

                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void NextInput(ref UpsamplerBufferState state, float input)
            {
                state.History.AsSpan()[1..].CopyTo(state.History.AsSpan());
                state.History[HistoryLength - 1] = input;
            }

            int inputBufferIndex = 0;

            switch (state.Scale)
            {
                case 6.0f:
                    for (int i = 0; i < outputSampleCount; i++)
                    {
                        switch (state.Phase)
                        {
                            case 0:
                                NextInput(ref state, inputBuffer[inputBufferIndex++]);
                                outputBuffer[i] = state.History[Bank0CenterIndex];
                                break;
                            case 1:
                                outputBuffer[i] = DoFilterBank(ref state, _bank1);
                                break;
                            case 2:
                                outputBuffer[i] = DoFilterBank(ref state, _bank2);
                                break;
                            case 3:
                                outputBuffer[i] = DoFilterBank(ref state, _bank3);
                                break;
                            case 4:
                                outputBuffer[i] = DoFilterBank(ref state, _bank4);
                                break;
                            case 5:
                                outputBuffer[i] = DoFilterBank(ref state, _bank5);
                                break;
                        }

                        state.Phase = (state.Phase + 1) % 6;
                    }
                    break;
                case 3.0f:
                    for (int i = 0; i < outputSampleCount; i++)
                    {
                        switch (state.Phase)
                        {
                            case 0:
                                NextInput(ref state, inputBuffer[inputBufferIndex++]);
                                outputBuffer[i] = state.History[Bank0CenterIndex];
                                break;
                            case 1:
                                outputBuffer[i] = DoFilterBank(ref state, _bank2);
                                break;
                            case 2:
                                outputBuffer[i] = DoFilterBank(ref state, _bank4);
                                break;
                        }

                        state.Phase = (state.Phase + 1) % 3;
                    }
                    break;
                case 1.5f:
                    // Upsample by 3 then decimate by 2.
                    for (int i = 0; i < outputSampleCount; i++)
                    {
                        switch (state.Phase)
                        {
                            case 0:
                                NextInput(ref state, inputBuffer[inputBufferIndex++]);
                                outputBuffer[i] = state.History[Bank0CenterIndex];
                                break;
                            case 1:
                                outputBuffer[i] = DoFilterBank(ref state, _bank4);
                                break;
                            case 2:
                                NextInput(ref state, inputBuffer[inputBufferIndex++]);
                                outputBuffer[i] = DoFilterBank(ref state, _bank2);
                                break;
                        }

                        state.Phase = (state.Phase + 1) % 3;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state.Scale, null);
            }
        }
    }
}
