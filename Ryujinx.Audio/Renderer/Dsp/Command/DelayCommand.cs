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

using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.Effect;
using Ryujinx.Audio.Renderer.Utils.Math;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DelayCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.Delay;

        public ulong EstimatedProcessingTime { get; set; }

        public DelayParameter Parameter => _parameter;
        public Memory<DelayState> State { get; }
        public ulong WorkBuffer { get; }
        public ushort[] OutputBufferIndices { get; }
        public ushort[] InputBufferIndices { get; }
        public bool IsEffectEnabled { get; }

        private DelayParameter _parameter;

        private const int FixedPointPrecision = 14;

        public DelayCommand(uint bufferOffset, DelayParameter parameter, Memory<DelayState> state, bool isEnabled, ulong workBuffer, int nodeId, bool newEffectChannelMappingSupported)
        {
            Enabled = true;
            NodeId = nodeId;
            _parameter = parameter;
            State = state;
            WorkBuffer = workBuffer;

            IsEffectEnabled = isEnabled;

            InputBufferIndices = new ushort[Constants.VoiceChannelCountMax];
            OutputBufferIndices = new ushort[Constants.VoiceChannelCountMax];

            for (int i = 0; i < Parameter.ChannelCount; i++)
            {
                InputBufferIndices[i] = (ushort)(bufferOffset + Parameter.Input[i]);
                OutputBufferIndices[i] = (ushort)(bufferOffset + Parameter.Output[i]);
            }

            // NOTE: We do the opposite as Nintendo here for now to restore previous behaviour
            // TODO: Update delay processing and remove this to use RemapLegacyChannelEffectMappingToChannelResourceMapping.
            DataSourceHelper.RemapChannelResourceMappingToLegacy(newEffectChannelMappingSupported, InputBufferIndices);
            DataSourceHelper.RemapChannelResourceMappingToLegacy(newEffectChannelMappingSupported, OutputBufferIndices);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private unsafe void ProcessDelayMono(ref DelayState state, float* outputBuffer, float* inputBuffer, uint sampleCount)
        {
            float feedbackGain = FixedPointHelper.ToFloat(Parameter.FeedbackGain, FixedPointPrecision);
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            for (int i = 0; i < sampleCount; i++)
            {
                float input = inputBuffer[i] * 64;
                float delayLineValue = state.DelayLines[0].Read();

                float temp = input * inGain + delayLineValue * feedbackGain;

                state.UpdateLowPassFilter(ref temp, 1);

                outputBuffer[i] = (input * dryGain + delayLineValue * outGain) / 64;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private unsafe void ProcessDelayStereo(ref DelayState state, Span<IntPtr> outputBuffers, ReadOnlySpan<IntPtr> inputBuffers, uint sampleCount)
        {
            const ushort channelCount = 2;

            float delayFeedbackBaseGain = state.DelayFeedbackBaseGain;
            float delayFeedbackCrossGain = state.DelayFeedbackCrossGain;
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            Matrix2x2 delayFeedback = new Matrix2x2(delayFeedbackBaseGain , delayFeedbackCrossGain,
                                                    delayFeedbackCrossGain, delayFeedbackBaseGain);

            for (int i = 0; i < sampleCount; i++)
            {
                Vector2 channelInput = new Vector2
                {
                    X = *((float*)inputBuffers[0] + i) * 64,
                    Y = *((float*)inputBuffers[1] + i) * 64,
                };

                Vector2 delayLineValues = new Vector2()
                {
                    X = state.DelayLines[0].Read(),
                    Y = state.DelayLines[1].Read(),
                };

                Vector2 temp = MatrixHelper.Transform(ref channelInput, ref delayFeedback) + channelInput * inGain;

                state.UpdateLowPassFilter(ref Unsafe.As<Vector2, float>(ref temp), channelCount);

                *((float*)outputBuffers[0] + i) = (channelInput.X * dryGain + delayLineValues.X * outGain) / 64;
                *((float*)outputBuffers[1] + i) = (channelInput.Y * dryGain + delayLineValues.Y * outGain) / 64;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private unsafe void ProcessDelayQuadraphonic(ref DelayState state, Span<IntPtr> outputBuffers, ReadOnlySpan<IntPtr> inputBuffers, uint sampleCount)
        {
            const ushort channelCount = 4;

            float delayFeedbackBaseGain = state.DelayFeedbackBaseGain;
            float delayFeedbackCrossGain = state.DelayFeedbackCrossGain;
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            Matrix4x4 delayFeedback = new Matrix4x4(delayFeedbackBaseGain , delayFeedbackCrossGain, delayFeedbackCrossGain, 0.0f,
                                                    delayFeedbackCrossGain, delayFeedbackBaseGain , 0.0f                  , delayFeedbackCrossGain,
                                                    delayFeedbackCrossGain, 0.0f                  , delayFeedbackBaseGain , delayFeedbackCrossGain,
                                                    0.0f                  , delayFeedbackCrossGain, delayFeedbackCrossGain, delayFeedbackBaseGain);


            for (int i = 0; i < sampleCount; i++)
            {
                Vector4 channelInput = new Vector4
                {
                    X = *((float*)inputBuffers[0] + i) * 64,
                    Y = *((float*)inputBuffers[1] + i) * 64,
                    Z = *((float*)inputBuffers[2] + i) * 64,
                    W = *((float*)inputBuffers[3] + i) * 64
                };

                Vector4 delayLineValues = new Vector4()
                {
                    X = state.DelayLines[0].Read(),
                    Y = state.DelayLines[1].Read(),
                    Z = state.DelayLines[2].Read(),
                    W = state.DelayLines[3].Read()
                };

                Vector4 temp = MatrixHelper.Transform(ref channelInput, ref delayFeedback) + channelInput * inGain;
                
                state.UpdateLowPassFilter(ref Unsafe.As<Vector4, float>(ref temp), channelCount);

                *((float*)outputBuffers[0] + i) = (channelInput.X * dryGain + delayLineValues.X * outGain) / 64;
                *((float*)outputBuffers[1] + i) = (channelInput.Y * dryGain + delayLineValues.Y * outGain) / 64;
                *((float*)outputBuffers[2] + i) = (channelInput.Z * dryGain + delayLineValues.Z * outGain) / 64;
                *((float*)outputBuffers[3] + i) = (channelInput.W * dryGain + delayLineValues.W * outGain) / 64;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private unsafe void ProcessDelaySurround(ref DelayState state, Span<IntPtr> outputBuffers, ReadOnlySpan<IntPtr> inputBuffers, uint sampleCount)
        {
            const ushort channelCount = 6;

            float feedbackGain = FixedPointHelper.ToFloat(Parameter.FeedbackGain, FixedPointPrecision);
            float delayFeedbackBaseGain = state.DelayFeedbackBaseGain;
            float delayFeedbackCrossGain = state.DelayFeedbackCrossGain;
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            Matrix6x6 delayFeedback = new Matrix6x6(delayFeedbackBaseGain , 0.0f                  , 0.0f                  , 0.0f                  , delayFeedbackCrossGain, delayFeedbackCrossGain,
                                                    0.0f                  , delayFeedbackBaseGain , 0.0f                  , delayFeedbackCrossGain, delayFeedbackCrossGain, 0.0f                  ,
                                                    delayFeedbackCrossGain, 0.0f                  , delayFeedbackBaseGain , delayFeedbackCrossGain, 0.0f                  , 0.0f                  ,
                                                    0.0f                  , delayFeedbackCrossGain, delayFeedbackCrossGain, delayFeedbackBaseGain , 0.0f                  , 0.0f                  ,
                                                    delayFeedbackCrossGain, delayFeedbackCrossGain, 0.0f                  , 0.0f                  , delayFeedbackBaseGain , 0.0f                  ,
                                                    0.0f                  , 0.0f                  , 0.0f                  , 0.0f                  , 0.0f                  , feedbackGain);

            for (int i = 0; i < sampleCount; i++)
            {
                Vector6 channelInput = new Vector6
                {
                    X = *((float*)inputBuffers[0] + i) * 64,
                    Y = *((float*)inputBuffers[1] + i) * 64,
                    Z = *((float*)inputBuffers[2] + i) * 64,
                    W = *((float*)inputBuffers[3] + i) * 64,
                    V = *((float*)inputBuffers[4] + i) * 64,
                    U = *((float*)inputBuffers[5] + i) * 64
                };

                Vector6 delayLineValues = new Vector6
                {
                    X = state.DelayLines[0].Read(),
                    Y = state.DelayLines[1].Read(),
                    Z = state.DelayLines[2].Read(),
                    W = state.DelayLines[3].Read(),
                    V = state.DelayLines[4].Read(),
                    U = state.DelayLines[5].Read()
                };

                Vector6 temp = MatrixHelper.Transform(ref channelInput, ref delayFeedback) + channelInput * inGain;

                state.UpdateLowPassFilter(ref Unsafe.As<Vector6, float>(ref temp), channelCount);

                *((float*)outputBuffers[0] + i) = (channelInput.X * dryGain + delayLineValues.X * outGain) / 64;
                *((float*)outputBuffers[1] + i) = (channelInput.Y * dryGain + delayLineValues.Y * outGain) / 64;
                *((float*)outputBuffers[2] + i) = (channelInput.Z * dryGain + delayLineValues.Z * outGain) / 64;
                *((float*)outputBuffers[3] + i) = (channelInput.W * dryGain + delayLineValues.W * outGain) / 64;
                *((float*)outputBuffers[4] + i) = (channelInput.V * dryGain + delayLineValues.V * outGain) / 64;
                *((float*)outputBuffers[5] + i) = (channelInput.U * dryGain + delayLineValues.U * outGain) / 64;
            }
        }

        private unsafe void ProcessDelay(CommandList context, ref DelayState state)
        {
            Debug.Assert(Parameter.IsChannelCountValid());

            if (IsEffectEnabled && Parameter.IsChannelCountValid())
            {
                Span<IntPtr> inputBuffers = stackalloc IntPtr[Parameter.ChannelCount];
                Span<IntPtr> outputBuffers = stackalloc IntPtr[Parameter.ChannelCount];

                for (int i = 0; i < Parameter.ChannelCount; i++)
                {
                    inputBuffers[i] = context.GetBufferPointer(InputBufferIndices[i]);
                    outputBuffers[i] = context.GetBufferPointer(OutputBufferIndices[i]);
                }

                switch (Parameter.ChannelCount)
                {
                    case 1:
                        ProcessDelayMono(ref state, (float*)outputBuffers[0], (float*)inputBuffers[0], context.SampleCount);
                        break;
                    case 2:
                        ProcessDelayStereo(ref state, outputBuffers, inputBuffers, context.SampleCount);
                        break;
                    case 4:
                        ProcessDelayQuadraphonic(ref state, outputBuffers, inputBuffers, context.SampleCount);
                        break;
                    case 6:
                        ProcessDelaySurround(ref state, outputBuffers, inputBuffers, context.SampleCount);
                        break;
                    default:
                        throw new NotImplementedException(Parameter.ChannelCount.ToString());
                }
            }
            else
            {
                for (int i = 0; i < Parameter.ChannelCount; i++)
                {
                    if (InputBufferIndices[i] != OutputBufferIndices[i])
                    {
                        context.CopyBuffer(OutputBufferIndices[i], InputBufferIndices[i]);
                    }
                }
            }
        }

        public void Process(CommandList context)
        {
            ref DelayState state = ref State.Span[0];

            if (IsEffectEnabled)
            {
                if (Parameter.Status == UsageState.Invalid)
                {
                    state = new DelayState(ref _parameter, WorkBuffer);
                }
                else if (Parameter.Status == UsageState.New)
                {
                    state.UpdateParameter(ref _parameter);
                }
            }

            ProcessDelay(context, ref state);
        }
    }
}
