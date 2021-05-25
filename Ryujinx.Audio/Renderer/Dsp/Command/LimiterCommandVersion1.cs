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
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class LimiterCommandVersion1 : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.LimiterVersion1;

        public ulong EstimatedProcessingTime { get; set; }

        public LimiterParameter Parameter => _parameter;
        public Memory<LimiterState> State { get; }
        public ulong WorkBuffer { get; }
        public ushort[] OutputBufferIndices { get; }
        public ushort[] InputBufferIndices { get; }
        public bool IsEffectEnabled { get; }

        private LimiterParameter _parameter;

        public LimiterCommandVersion1(uint bufferOffset, LimiterParameter parameter, Memory<LimiterState> state, bool isEnabled, ulong workBuffer, int nodeId)
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

        public void Process(CommandList context)
        {
            ref LimiterState state = ref State.Span[0];

            if (IsEffectEnabled)
            {
                if (Parameter.Status == Server.Effect.UsageState.Invalid)
                {
                    state = new LimiterState(ref _parameter, WorkBuffer);
                }
                else if (Parameter.Status == Server.Effect.UsageState.New)
                {
                    state.UpdateParameter(ref _parameter);
                }
            }

            ProcessLimiter(context);
        }

        private void ProcessLimiter(CommandList context)
        {
            Debug.Assert(Parameter.IsChannelCountValid());

            if (IsEffectEnabled && Parameter.IsChannelCountValid())
            {
                ref LimiterState state = ref State.Span[0];

                ReadOnlyMemory<float>[] inputBuffers = new ReadOnlyMemory<float>[Parameter.ChannelCount];
                Memory<float>[] outputBuffers = new Memory<float>[Parameter.ChannelCount];

                for (int i = 0; i < Parameter.ChannelCount; i++)
                {
                    inputBuffers[i] = context.GetBufferMemory(InputBufferIndices[i]);
                    outputBuffers[i] = context.GetBufferMemory(OutputBufferIndices[i]);
                }

                for (int channelIndex = 0; channelIndex < Parameter.ChannelCount; channelIndex++)
                {
                    for (int sampleIndex = 0; sampleIndex < context.SampleCount; sampleIndex++)
                    {
                        float inputSample = inputBuffers[channelIndex].Span[sampleIndex];

                        float sampleInputMax = Math.Abs(inputSample * Parameter.InputGain);

                        float inputCoefficient = Parameter.ReleaseCoefficient;

                        if (sampleInputMax > state.DectectorAverage[channelIndex])
                        {
                            inputCoefficient = Parameter.AttackCoefficient;
                        }

                        state.DectectorAverage[channelIndex] += inputCoefficient * (sampleInputMax - state.DectectorAverage[channelIndex]);

                        float attenuation = 1.0f;

                        if (state.DectectorAverage[channelIndex] > Parameter.Threshold)
                        {
                            attenuation = Parameter.Threshold / state.DectectorAverage[channelIndex];
                        }

                        float outputCoefficient = Parameter.ReleaseCoefficient;

                        if (state.CompressionGain[channelIndex] > attenuation)
                        {
                            outputCoefficient = Parameter.AttackCoefficient;
                        }

                        state.CompressionGain[channelIndex] += outputCoefficient * (attenuation - state.CompressionGain[channelIndex]);

                        ref float delayedSample = ref state.DelayedSampleBuffer[channelIndex * Parameter.DelayBufferSampleCountMax + state.DelayedSampleBufferPosition[channelIndex]];

                        outputBuffers[channelIndex].Span[sampleIndex] = delayedSample * state.CompressionGain[channelIndex] * Parameter.OutputGain;

                        delayedSample = inputSample;

                        state.DelayedSampleBufferPosition[channelIndex]++;

                        while (state.DelayedSampleBufferPosition[channelIndex] >= Parameter.DelayBufferSampleCountMin)
                        {
                            state.DelayedSampleBufferPosition[channelIndex] -= Parameter.DelayBufferSampleCountMin;
                        }
                    }
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
    }
}
