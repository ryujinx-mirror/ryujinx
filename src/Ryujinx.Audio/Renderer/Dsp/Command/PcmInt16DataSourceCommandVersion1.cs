using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Server.Voice;
using System;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;
using WaveBuffer = Ryujinx.Audio.Renderer.Common.WaveBuffer;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class PcmInt16DataSourceCommandVersion1 : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.PcmInt16DataSourceVersion1;

        public uint EstimatedProcessingTime { get; set; }

        public ushort OutputBufferIndex { get; }
        public uint SampleRate { get; }
        public uint ChannelIndex { get; }

        public uint ChannelCount { get; }

        public float Pitch { get; }

        public WaveBuffer[] WaveBuffers { get; }

        public Memory<VoiceUpdateState> State { get; }
        public DecodingBehaviour DecodingBehaviour { get; }

        public PcmInt16DataSourceCommandVersion1(ref VoiceState serverState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, ushort channelIndex, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            OutputBufferIndex = (ushort)(channelIndex + outputBufferIndex);
            SampleRate = serverState.SampleRate;
            ChannelIndex = channelIndex;
            ChannelCount = serverState.ChannelsCount;
            Pitch = serverState.Pitch;

            WaveBuffers = new WaveBuffer[Constants.VoiceWaveBufferCount];

            for (int i = 0; i < WaveBuffers.Length; i++)
            {
                ref Server.Voice.WaveBuffer voiceWaveBuffer = ref serverState.WaveBuffers[i];

                WaveBuffers[i] = voiceWaveBuffer.ToCommon(1);
            }

            State = state;
            DecodingBehaviour = serverState.DecodingBehaviour;
        }

        public void Process(CommandList context)
        {
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            DataSourceHelper.WaveBufferInformation info = new()
            {
                SourceSampleRate = SampleRate,
                SampleFormat = SampleFormat.PcmInt16,
                Pitch = Pitch,
                DecodingBehaviour = DecodingBehaviour,
                ExtraParameter = 0,
                ExtraParameterSize = 0,
                ChannelIndex = (int)ChannelIndex,
                ChannelCount = (int)ChannelCount,
            };

            DataSourceHelper.ProcessWaveBuffers(context.MemoryManager, outputBuffer, ref info, WaveBuffers, ref State.Span[0], context.SampleRate, (int)context.SampleCount);
        }
    }
}
