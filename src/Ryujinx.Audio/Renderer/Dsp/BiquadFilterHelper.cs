using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp
{
    public static class BiquadFilterHelper
    {
        private const int FixedPointPrecisionForParameter = 14;

        /// <summary>
        /// Apply a single biquad filter.
        /// </summary>
        /// <remarks>This is implemented with a direct form 2.</remarks>
        /// <param name="parameter">The biquad filter parameter</param>
        /// <param name="state">The biquad filter state</param>
        /// <param name="outputBuffer">The output buffer to write the result</param>
        /// <param name="inputBuffer">The input buffer to write the result</param>
        /// <param name="sampleCount">The count of samples to process</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessBiquadFilter(ref BiquadFilterParameter parameter, ref BiquadFilterState state, Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, uint sampleCount)
        {
            float a0 = FixedPointHelper.ToFloat(parameter.Numerator[0], FixedPointPrecisionForParameter);
            float a1 = FixedPointHelper.ToFloat(parameter.Numerator[1], FixedPointPrecisionForParameter);
            float a2 = FixedPointHelper.ToFloat(parameter.Numerator[2], FixedPointPrecisionForParameter);

            float b1 = FixedPointHelper.ToFloat(parameter.Denominator[0], FixedPointPrecisionForParameter);
            float b2 = FixedPointHelper.ToFloat(parameter.Denominator[1], FixedPointPrecisionForParameter);

            for (int i = 0; i < sampleCount; i++)
            {
                float input = inputBuffer[i];
                float output = input * a0 + state.State0;

                state.State0 = input * a1 + output * b1 + state.State1;
                state.State1 = input * a2 + output * b2;

                outputBuffer[i] = output;
            }
        }

        /// <summary>
        /// Apply multiple biquad filter.
        /// </summary>
        /// <remarks>This is implemented with a direct form 1.</remarks>
        /// <param name="parameters">The biquad filter parameter</param>
        /// <param name="states">The biquad filter state</param>
        /// <param name="outputBuffer">The output buffer to write the result</param>
        /// <param name="inputBuffer">The input buffer to write the result</param>
        /// <param name="sampleCount">The count of samples to process</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessBiquadFilter(ReadOnlySpan<BiquadFilterParameter> parameters, Span<BiquadFilterState> states, Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, uint sampleCount)
        {
            for (int stageIndex = 0; stageIndex < parameters.Length; stageIndex++)
            {
                BiquadFilterParameter parameter = parameters[stageIndex];

                ref BiquadFilterState state = ref states[stageIndex];

                float a0 = FixedPointHelper.ToFloat(parameter.Numerator[0], FixedPointPrecisionForParameter);
                float a1 = FixedPointHelper.ToFloat(parameter.Numerator[1], FixedPointPrecisionForParameter);
                float a2 = FixedPointHelper.ToFloat(parameter.Numerator[2], FixedPointPrecisionForParameter);

                float b1 = FixedPointHelper.ToFloat(parameter.Denominator[0], FixedPointPrecisionForParameter);
                float b2 = FixedPointHelper.ToFloat(parameter.Denominator[1], FixedPointPrecisionForParameter);

                for (int i = 0; i < sampleCount; i++)
                {
                    float input = inputBuffer[i];
                    float output = input * a0 + state.State0 * a1 + state.State1 * a2 + state.State2 * b1 + state.State3 * b2;

                    state.State1 = state.State0;
                    state.State0 = input;
                    state.State3 = state.State2;
                    state.State2 = output;

                    outputBuffer[i] = output;
                }
            }
        }
    }
}