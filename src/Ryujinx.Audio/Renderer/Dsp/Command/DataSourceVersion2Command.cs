using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Server.Voice;
using System;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;
using WaveBuffer = Ryujinx.Audio.Renderer.Common.WaveBuffer;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DataSourceVersion2Command : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType { get; }

        public uint EstimatedProcessingTime { get; set; }

        public ushort OutputBufferIndex { get; }
        public uint SampleRate { get; }

        public float Pitch { get; }

        public WaveBuffer[] WaveBuffers { get; }

        public Memory<VoiceUpdateState> State { get; }

        public ulong ExtraParameter { get; }
        public ulong ExtraParameterSize { get; }

        public uint ChannelIndex { get; }

        public uint ChannelCount { get; }

        public DecodingBehaviour DecodingBehaviour { get; }

        public SampleFormat SampleFormat { get; }

        public SampleRateConversionQuality SrcQuality { get; }

        public DataSourceVersion2Command(ref VoiceState serverState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, ushort channelIndex, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;
            ChannelIndex = channelIndex;
            ChannelCount = serverState.ChannelsCount;
            SampleFormat = serverState.SampleFormat;
            SrcQuality = serverState.SrcQuality;
            CommandType = GetCommandTypeBySampleFormat(SampleFormat);

            OutputBufferIndex = (ushort)(channelIndex + outputBufferIndex);
            SampleRate = serverState.SampleRate;
            Pitch = serverState.Pitch;

            WaveBuffers = new WaveBuffer[Constants.VoiceWaveBufferCount];

            for (int i = 0; i < WaveBuffers.Length; i++)
            {
                ref Server.Voice.WaveBuffer voiceWaveBuffer = ref serverState.WaveBuffers[i];

                WaveBuffers[i] = voiceWaveBuffer.ToCommon(2);
            }

            if (SampleFormat == SampleFormat.Adpcm)
            {
                ExtraParameter = serverState.DataSourceStateAddressInfo.GetReference(true);
                ExtraParameterSize = serverState.DataSourceStateAddressInfo.Size;
            }

            State = state;
            DecodingBehaviour = serverState.DecodingBehaviour;
        }

        private static CommandType GetCommandTypeBySampleFormat(SampleFormat sampleFormat)
        {
            return sampleFormat switch
            {
                SampleFormat.Adpcm => CommandType.AdpcmDataSourceVersion2,
                SampleFormat.PcmInt16 => CommandType.PcmInt16DataSourceVersion2,
                SampleFormat.PcmFloat => CommandType.PcmFloatDataSourceVersion2,
                _ => throw new NotImplementedException($"{sampleFormat}"),
            };
        }

        public void Process(CommandList context)
        {
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            DataSourceHelper.WaveBufferInformation info = new()
            {
                SourceSampleRate = SampleRate,
                SampleFormat = SampleFormat,
                Pitch = Pitch,
                DecodingBehaviour = DecodingBehaviour,
                ExtraParameter = ExtraParameter,
                ExtraParameterSize = ExtraParameterSize,
                ChannelIndex = (int)ChannelIndex,
                ChannelCount = (int)ChannelCount,
                SrcQuality = SrcQuality,
            };

            DataSourceHelper.ProcessWaveBuffers(context.MemoryManager, outputBuffer, ref info, WaveBuffers, ref State.Span[0], context.SampleRate, (int)context.SampleCount);
        }
    }
}
