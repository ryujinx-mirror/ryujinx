using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.Effect;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class LimiterCommandVersion1 : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.LimiterVersion1;

        public uint EstimatedProcessingTime { get; set; }

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
                if (Parameter.Status == UsageState.Invalid)
                {
                    state = new LimiterState(ref _parameter, WorkBuffer);
                }
                else if (Parameter.Status == UsageState.New)
                {
                    LimiterState.UpdateParameter(ref _parameter);
                }
            }

            ProcessLimiter(context, ref state);
        }

        private unsafe void ProcessLimiter(CommandList context, ref LimiterState state)
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

                for (int channelIndex = 0; channelIndex < Parameter.ChannelCount; channelIndex++)
                {
                    for (int sampleIndex = 0; sampleIndex < context.SampleCount; sampleIndex++)
                    {
                        float rawInputSample = *((float*)inputBuffers[channelIndex] + sampleIndex);

                        float inputSample = (rawInputSample / short.MaxValue) * Parameter.InputGain;

                        float sampleInputMax = Math.Abs(inputSample);

                        float inputCoefficient = Parameter.ReleaseCoefficient;

                        if (sampleInputMax > state.DetectorAverage[channelIndex].Read())
                        {
                            inputCoefficient = Parameter.AttackCoefficient;
                        }

                        float detectorValue = state.DetectorAverage[channelIndex].Update(sampleInputMax, inputCoefficient);
                        float attenuation = 1.0f;

                        if (detectorValue > Parameter.Threshold)
                        {
                            attenuation = Parameter.Threshold / detectorValue;
                        }

                        float outputCoefficient = Parameter.ReleaseCoefficient;

                        if (state.CompressionGainAverage[channelIndex].Read() > attenuation)
                        {
                            outputCoefficient = Parameter.AttackCoefficient;
                        }

                        float compressionGain = state.CompressionGainAverage[channelIndex].Update(attenuation, outputCoefficient);

                        ref float delayedSample = ref state.DelayedSampleBuffer[channelIndex * Parameter.DelayBufferSampleCountMax + state.DelayedSampleBufferPosition[channelIndex]];

                        float outputSample = delayedSample * compressionGain * Parameter.OutputGain;

                        *((float*)outputBuffers[channelIndex] + sampleIndex) = outputSample * short.MaxValue;

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
                        context.CopyBuffer(OutputBufferIndices[i], InputBufferIndices[i]);
                    }
                }
            }
        }
    }
}
