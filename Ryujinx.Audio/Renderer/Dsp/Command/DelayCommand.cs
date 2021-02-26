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
using System;
using System.Diagnostics;

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

        public DelayCommand(uint bufferOffset, DelayParameter parameter, Memory<DelayState> state, bool isEnabled, ulong workBuffer, int nodeId)
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
        }

        private void ProcessDelayMono(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, uint sampleCount)
        {
            ref DelayState state = ref State.Span[0];

            float feedbackGain = FixedPointHelper.ToFloat(Parameter.FeedbackGain, FixedPointPrecision);
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            for (int i = 0; i < sampleCount; i++)
            {
                float input = inputBuffer[i] * 64;
                float delayLineValue = state.DelayLines[0].Read();

                float lowPassResult = input * inGain + delayLineValue * feedbackGain * state.LowPassBaseGain + state.LowPassZ[0] * state.LowPassFeedbackGain;

                state.LowPassZ[0] = lowPassResult;

                state.DelayLines[0].Update(lowPassResult);

                outputBuffer[i] = (input * dryGain + delayLineValue * outGain) / 64;
            }
        }

        private void ProcessDelayStereo(Memory<float>[] outputBuffers, ReadOnlyMemory<float>[] inputBuffers, uint sampleCount)
        {
            ref DelayState state = ref State.Span[0];

            float[] channelInput = new float[Parameter.ChannelCount];
            float[] delayLineValues = new float[Parameter.ChannelCount];
            float[] temp = new float[Parameter.ChannelCount];

            float delayFeedbackBaseGain = state.DelayFeedbackBaseGain;
            float delayFeedbackCrossGain = state.DelayFeedbackCrossGain;
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            for (int i = 0; i < sampleCount; i++)
            {
                for (int j = 0; j < Parameter.ChannelCount; j++)
                {
                    channelInput[j] = inputBuffers[j].Span[i] * 64;
                    delayLineValues[j] = state.DelayLines[j].Read();
                }

                temp[0] = channelInput[0] * inGain + delayLineValues[1] * delayFeedbackCrossGain + delayLineValues[0] * delayFeedbackBaseGain;
                temp[1] = channelInput[1] * inGain + delayLineValues[0] * delayFeedbackCrossGain + delayLineValues[1] * delayFeedbackBaseGain;

                for (int j = 0; j < Parameter.ChannelCount; j++)
                {
                    float lowPassResult = state.LowPassFeedbackGain * state.LowPassZ[j] + temp[j] * state.LowPassBaseGain;

                    state.LowPassZ[j] = lowPassResult;
                    state.DelayLines[j].Update(lowPassResult);

                    outputBuffers[j].Span[i] = (channelInput[j] * dryGain + delayLineValues[j] * outGain) / 64;
                }
            }
        }

        private void ProcessDelayQuadraphonic(Memory<float>[] outputBuffers, ReadOnlyMemory<float>[] inputBuffers, uint sampleCount)
        {
            ref DelayState state = ref State.Span[0];

            float[] channelInput = new float[Parameter.ChannelCount];
            float[] delayLineValues = new float[Parameter.ChannelCount];
            float[] temp = new float[Parameter.ChannelCount];

            float delayFeedbackBaseGain = state.DelayFeedbackBaseGain;
            float delayFeedbackCrossGain = state.DelayFeedbackCrossGain;
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            for (int i = 0; i < sampleCount; i++)
            {
                for (int j = 0; j < Parameter.ChannelCount; j++)
                {
                    channelInput[j] = inputBuffers[j].Span[i] * 64;
                    delayLineValues[j] = state.DelayLines[j].Read();
                }

                temp[0] = channelInput[0] * inGain + (delayLineValues[2] + delayLineValues[1]) * delayFeedbackCrossGain + delayLineValues[0] * delayFeedbackBaseGain;
                temp[1] = channelInput[1] * inGain + (delayLineValues[0] + delayLineValues[3]) * delayFeedbackCrossGain + delayLineValues[1] * delayFeedbackBaseGain;
                temp[2] = channelInput[2] * inGain + (delayLineValues[3] + delayLineValues[0]) * delayFeedbackCrossGain + delayLineValues[2] * delayFeedbackBaseGain;
                temp[3] = channelInput[3] * inGain + (delayLineValues[1] + delayLineValues[2]) * delayFeedbackCrossGain + delayLineValues[3] * delayFeedbackBaseGain;

                for (int j = 0; j < Parameter.ChannelCount; j++)
                {
                    float lowPassResult = state.LowPassFeedbackGain * state.LowPassZ[j] + temp[j] * state.LowPassBaseGain;

                    state.LowPassZ[j] = lowPassResult;
                    state.DelayLines[j].Update(lowPassResult);

                    outputBuffers[j].Span[i] = (channelInput[j] * dryGain + delayLineValues[j] * outGain) / 64;
                }
            }
        }

        private void ProcessDelaySurround(Memory<float>[] outputBuffers, ReadOnlyMemory<float>[] inputBuffers, uint sampleCount)
        {
            ref DelayState state = ref State.Span[0];

            float[] channelInput = new float[Parameter.ChannelCount];
            float[] delayLineValues = new float[Parameter.ChannelCount];
            float[] temp = new float[Parameter.ChannelCount];

            float delayFeedbackBaseGain = state.DelayFeedbackBaseGain;
            float delayFeedbackCrossGain = state.DelayFeedbackCrossGain;
            float inGain = FixedPointHelper.ToFloat(Parameter.InGain, FixedPointPrecision);
            float dryGain = FixedPointHelper.ToFloat(Parameter.DryGain, FixedPointPrecision);
            float outGain = FixedPointHelper.ToFloat(Parameter.OutGain, FixedPointPrecision);

            for (int i = 0; i < sampleCount; i++)
            {
                for (int j = 0; j < Parameter.ChannelCount; j++)
                {
                    channelInput[j] = inputBuffers[j].Span[i] * 64;
                    delayLineValues[j] = state.DelayLines[j].Read();
                }

                temp[0] = channelInput[0] * inGain + (delayLineValues[2] + delayLineValues[4]) * delayFeedbackCrossGain + delayLineValues[0] * delayFeedbackBaseGain;
                temp[1] = channelInput[1] * inGain + (delayLineValues[4] + delayLineValues[3]) * delayFeedbackCrossGain + delayLineValues[1] * delayFeedbackBaseGain;
                temp[2] = channelInput[2] * inGain + (delayLineValues[3] + delayLineValues[0]) * delayFeedbackCrossGain + delayLineValues[2] * delayFeedbackBaseGain;
                temp[3] = channelInput[3] * inGain + (delayLineValues[1] + delayLineValues[2]) * delayFeedbackCrossGain + delayLineValues[3] * delayFeedbackBaseGain;
                temp[4] = channelInput[4] * inGain + (delayLineValues[0] + delayLineValues[1]) * delayFeedbackCrossGain + delayLineValues[4] * delayFeedbackBaseGain;
                temp[5] = channelInput[5] * inGain + delayLineValues[5] * delayFeedbackBaseGain;

                for (int j = 0; j < Parameter.ChannelCount; j++)
                {
                    float lowPassResult = state.LowPassFeedbackGain * state.LowPassZ[j] + temp[j] * state.LowPassBaseGain;

                    state.LowPassZ[j] = lowPassResult;
                    state.DelayLines[j].Update(lowPassResult);

                    outputBuffers[j].Span[i] = (channelInput[j] * dryGain + delayLineValues[j] * outGain) / 64;
                }
            }
        }

        private void ProcessDelay(CommandList context)
        {
            Debug.Assert(Parameter.IsChannelCountValid());

            if (IsEffectEnabled && Parameter.IsChannelCountValid())
            {
                ReadOnlyMemory<float>[] inputBuffers = new ReadOnlyMemory<float>[Parameter.ChannelCount];
                Memory<float>[] outputBuffers = new Memory<float>[Parameter.ChannelCount];

                for (int i = 0; i < Parameter.ChannelCount; i++)
                {
                    inputBuffers[i] = context.GetBufferMemory(InputBufferIndices[i]);
                    outputBuffers[i] = context.GetBufferMemory(OutputBufferIndices[i]);
                }

                switch (Parameter.ChannelCount)
                {
                    case 1:
                        ProcessDelayMono(outputBuffers[0].Span, inputBuffers[0].Span, context.SampleCount);
                        break;
                    case 2:
                        ProcessDelayStereo(outputBuffers, inputBuffers, context.SampleCount);
                        break;
                    case 4:
                        ProcessDelayQuadraphonic(outputBuffers, inputBuffers, context.SampleCount);
                        break;
                    case 6:
                        ProcessDelaySurround(outputBuffers, inputBuffers, context.SampleCount);
                        break;
                    default:
                        throw new NotImplementedException($"{Parameter.ChannelCount}");
                }
            }
            else
            {
                for (int i = 0; i < Parameter.ChannelCount; i++)
                {
                    if (InputBufferIndices[i] != OutputBufferIndices[i])
                    {
                        context.GetBufferMemory(InputBufferIndices[i]).CopyTo(context.GetBufferMemory(OutputBufferIndices[i]));
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

            ProcessDelay(context);
        }
    }
}
