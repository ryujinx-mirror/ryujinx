using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Server.Voice;
using System;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;
using WaveBuffer = Ryujinx.Audio.Renderer.Common.WaveBuffer;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class AdpcmDataSourceCommandVersion1 : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.AdpcmDataSourceVersion1;

        public uint EstimatedProcessingTime { get; set; }

        public ushort OutputBufferIndex { get; }
        public uint SampleRate { get; }

        public float Pitch { get; }

        public WaveBuffer[] WaveBuffers { get; }

        public Memory<VoiceUpdateState> State { get; }

        public ulong AdpcmParameter { get; }
        public ulong AdpcmParameterSize { get; }

        public DecodingBehaviour DecodingBehaviour { get; }

        public AdpcmDataSourceCommandVersion1(ref VoiceState serverState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            OutputBufferIndex = outputBufferIndex;
            SampleRate = serverState.SampleRate;
            Pitch = serverState.Pitch;

            WaveBuffers = new WaveBuffer[Constants.VoiceWaveBufferCount];

            for (int i = 0; i < WaveBuffers.Length; i++)
            {
                ref Server.Voice.WaveBuffer voiceWaveBuffer = ref serverState.WaveBuffers[i];

                WaveBuffers[i] = voiceWaveBuffer.ToCommon(1);
            }

            AdpcmParameter = serverState.DataSourceStateAddressInfo.GetReference(true);
            AdpcmParameterSize = serverState.DataSourceStateAddressInfo.Size;
            State = state;
            DecodingBehaviour = serverState.DecodingBehaviour;
        }

        public void Process(CommandList context)
        {
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            DataSourceHelper.WaveBufferInformation info = new()
            {
                SourceSampleRate = SampleRate,
                SampleFormat = SampleFormat.Adpcm,
                Pitch = Pitch,
                DecodingBehaviour = DecodingBehaviour,
                ExtraParameter = AdpcmParameter,
                ExtraParameterSize = AdpcmParameterSize,
                ChannelIndex = 0,
                ChannelCount = 1,
            };

            DataSourceHelper.ProcessWaveBuffers(context.MemoryManager, outputBuffer, ref info, WaveBuffers, ref State.Span[0], context.SampleRate, (int)context.SampleCount);
        }
    }
}
