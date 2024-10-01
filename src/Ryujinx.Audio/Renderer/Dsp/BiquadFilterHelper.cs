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
        /// <param name="inputBuffer">The input buffer to read the samples from</param>
        /// <param name="sampleCount">The count of samples to process</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessBiquadFilter(
            ref BiquadFilterParameter parameter,
            ref BiquadFilterState state,
            Span<float> outputBuffer,
            ReadOnlySpan<float> inputBuffer,
            uint sampleCount)
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
        /// Apply a single biquad filter and mix the result into the output buffer.
        /// </summary>
        /// <remarks>This is implemented with a direct form 1.</remarks>
        /// <param name="parameter">The biquad filter parameter</param>
        /// <param name="state">The biquad filter state</param>
        /// <param name="outputBuffer">The output buffer to write the result</param>
        /// <param name="inputBuffer">The input buffer to read the samples from</param>
        /// <param name="sampleCount">The count of samples to process</param>
        /// <param name="volume">Mix volume</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessBiquadFilterAndMix(
            ref BiquadFilterParameter parameter,
            ref BiquadFilterState state,
            Span<float> outputBuffer,
            ReadOnlySpan<float> inputBuffer,
            uint sampleCount,
            float volume)
        {
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

                outputBuffer[i] += FloatingPointHelper.MultiplyRoundUp(output, volume);
            }
        }

        /// <summary>
        /// Apply a single biquad filter and mix the result into the output buffer with volume ramp.
        /// </summary>
        /// <remarks>This is implemented with a direct form 1.</remarks>
        /// <param name="parameter">The biquad filter parameter</param>
        /// <param name="state">The biquad filter state</param>
        /// <param name="outputBuffer">The output buffer to write the result</param>
        /// <param name="inputBuffer">The input buffer to read the samples from</param>
        /// <param name="sampleCount">The count of samples to process</param>
        /// <param name="volume">Initial mix volume</param>
        /// <param name="ramp">Volume increment step</param>
        /// <returns>Last filtered sample value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ProcessBiquadFilterAndMixRamp(
            ref BiquadFilterParameter parameter,
            ref BiquadFilterState state,
            Span<float> outputBuffer,
            ReadOnlySpan<float> inputBuffer,
            uint sampleCount,
            float volume,
            float ramp)
        {
            float a0 = FixedPointHelper.ToFloat(parameter.Numerator[0], FixedPointPrecisionForParameter);
            float a1 = FixedPointHelper.ToFloat(parameter.Numerator[1], FixedPointPrecisionForParameter);
            float a2 = FixedPointHelper.ToFloat(parameter.Numerator[2], FixedPointPrecisionForParameter);

            float b1 = FixedPointHelper.ToFloat(parameter.Denominator[0], FixedPointPrecisionForParameter);
            float b2 = FixedPointHelper.ToFloat(parameter.Denominator[1], FixedPointPrecisionForParameter);

            float mixState = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float input = inputBuffer[i];
                float output = input * a0 + state.State0 * a1 + state.State1 * a2 + state.State2 * b1 + state.State3 * b2;

                state.State1 = state.State0;
                state.State0 = input;
                state.State3 = state.State2;
                state.State2 = output;

                mixState = FloatingPointHelper.MultiplyRoundUp(output, volume);

                outputBuffer[i] += mixState;
                volume += ramp;
            }

            return mixState;
        }

        /// <summary>
        /// Apply multiple biquad filter.
        /// </summary>
        /// <remarks>This is implemented with a direct form 1.</remarks>
        /// <param name="parameters">The biquad filter parameter</param>
        /// <param name="states">The biquad filter state</param>
        /// <param name="outputBuffer">The output buffer to write the result</param>
        /// <param name="inputBuffer">The input buffer to read the samples from</param>
        /// <param name="sampleCount">The count of samples to process</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessBiquadFilter(
            ReadOnlySpan<BiquadFilterParameter> parameters,
            Span<BiquadFilterState> states,
            Span<float> outputBuffer,
            ReadOnlySpan<float> inputBuffer,
            uint sampleCount)
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
                    float input = stageIndex != 0 ? outputBuffer[i] : inputBuffer[i];
                    float output = input * a0 + state.State0 * a1 + state.State1 * a2 + state.State2 * b1 + state.State3 * b2;

                    state.State1 = state.State0;
                    state.State0 = input;
                    state.State3 = state.State2;
                    state.State2 = output;

                    outputBuffer[i] = output;
                }
            }
        }

        /// <summary>
        /// Apply double biquad filter and mix the result into the output buffer.
        /// </summary>
        /// <remarks>This is implemented with a direct form 1.</remarks>
        /// <param name="parameters">The biquad filter parameter</param>
        /// <param name="states">The biquad filter state</param>
        /// <param name="outputBuffer">The output buffer to write the result</param>
        /// <param name="inputBuffer">The input buffer to read the samples from</param>
        /// <param name="sampleCount">The count of samples to process</param>
        /// <param name="volume">Mix volume</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessDoubleBiquadFilterAndMix(
            ref BiquadFilterParameter parameter0,
            ref BiquadFilterParameter parameter1,
            ref BiquadFilterState state0,
            ref BiquadFilterState state1,
            Span<float> outputBuffer,
            ReadOnlySpan<float> inputBuffer,
            uint sampleCount,
            float volume)
        {
            float a00 = FixedPointHelper.ToFloat(parameter0.Numerator[0], FixedPointPrecisionForParameter);
            float a10 = FixedPointHelper.ToFloat(parameter0.Numerator[1], FixedPointPrecisionForParameter);
            float a20 = FixedPointHelper.ToFloat(parameter0.Numerator[2], FixedPointPrecisionForParameter);

            float b10 = FixedPointHelper.ToFloat(parameter0.Denominator[0], FixedPointPrecisionForParameter);
            float b20 = FixedPointHelper.ToFloat(parameter0.Denominator[1], FixedPointPrecisionForParameter);

            float a01 = FixedPointHelper.ToFloat(parameter1.Numerator[0], FixedPointPrecisionForParameter);
            float a11 = FixedPointHelper.ToFloat(parameter1.Numerator[1], FixedPointPrecisionForParameter);
            float a21 = FixedPointHelper.ToFloat(parameter1.Numerator[2], FixedPointPrecisionForParameter);

            float b11 = FixedPointHelper.ToFloat(parameter1.Denominator[0], FixedPointPrecisionForParameter);
            float b21 = FixedPointHelper.ToFloat(parameter1.Denominator[1], FixedPointPrecisionForParameter);

            for (int i = 0; i < sampleCount; i++)
            {
                float input = inputBuffer[i];
                float output = input * a00 + state0.State0 * a10 + state0.State1 * a20 + state0.State2 * b10 + state0.State3 * b20;

                state0.State1 = state0.State0;
                state0.State0 = input;
                state0.State3 = state0.State2;
                state0.State2 = output;

                input = output;
                output = input * a01 + state1.State0 * a11 + state1.State1 * a21 + state1.State2 * b11 + state1.State3 * b21;

                state1.State1 = state1.State0;
                state1.State0 = input;
                state1.State3 = state1.State2;
                state1.State2 = output;

                outputBuffer[i] += FloatingPointHelper.MultiplyRoundUp(output, volume);
            }
        }

        /// <summary>
        /// Apply double biquad filter and mix the result into the output buffer with volume ramp.
        /// </summary>
        /// <remarks>This is implemented with a direct form 1.</remarks>
        /// <param name="parameters">The biquad filter parameter</param>
        /// <param name="states">The biquad filter state</param>
        /// <param name="outputBuffer">The output buffer to write the result</param>
        /// <param name="inputBuffer">The input buffer to read the samples from</param>
        /// <param name="sampleCount">The count of samples to process</param>
        /// <param name="volume">Initial mix volume</param>
        /// <param name="ramp">Volume increment step</param>
        /// <returns>Last filtered sample value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ProcessDoubleBiquadFilterAndMixRamp(
            ref BiquadFilterParameter parameter0,
            ref BiquadFilterParameter parameter1,
            ref BiquadFilterState state0,
            ref BiquadFilterState state1,
            Span<float> outputBuffer,
            ReadOnlySpan<float> inputBuffer,
            uint sampleCount,
            float volume,
            float ramp)
        {
            float a00 = FixedPointHelper.ToFloat(parameter0.Numerator[0], FixedPointPrecisionForParameter);
            float a10 = FixedPointHelper.ToFloat(parameter0.Numerator[1], FixedPointPrecisionForParameter);
            float a20 = FixedPointHelper.ToFloat(parameter0.Numerator[2], FixedPointPrecisionForParameter);

            float b10 = FixedPointHelper.ToFloat(parameter0.Denominator[0], FixedPointPrecisionForParameter);
            float b20 = FixedPointHelper.ToFloat(parameter0.Denominator[1], FixedPointPrecisionForParameter);

            float a01 = FixedPointHelper.ToFloat(parameter1.Numerator[0], FixedPointPrecisionForParameter);
            float a11 = FixedPointHelper.ToFloat(parameter1.Numerator[1], FixedPointPrecisionForParameter);
            float a21 = FixedPointHelper.ToFloat(parameter1.Numerator[2], FixedPointPrecisionForParameter);

            float b11 = FixedPointHelper.ToFloat(parameter1.Denominator[0], FixedPointPrecisionForParameter);
            float b21 = FixedPointHelper.ToFloat(parameter1.Denominator[1], FixedPointPrecisionForParameter);

            float mixState = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float input = inputBuffer[i];
                float output = input * a00 + state0.State0 * a10 + state0.State1 * a20 + state0.State2 * b10 + state0.State3 * b20;

                state0.State1 = state0.State0;
                state0.State0 = input;
                state0.State3 = state0.State2;
                state0.State2 = output;

                input = output;
                output = input * a01 + state1.State0 * a11 + state1.State1 * a21 + state1.State2 * b11 + state1.State3 * b21;

                state1.State1 = state1.State0;
                state1.State0 = input;
                state1.State3 = state1.State2;
                state1.State2 = output;

                mixState = FloatingPointHelper.MultiplyRoundUp(output, volume);

                outputBuffer[i] += mixState;
                volume += ramp;
            }

            return mixState;
        }
    }
}
