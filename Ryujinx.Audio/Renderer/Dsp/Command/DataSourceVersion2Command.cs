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

using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using System;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DataSourceVersion2Command : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType { get; }

        public ulong EstimatedProcessingTime { get; set; }

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

        public DataSourceVersion2Command(ref Server.Voice.VoiceState serverState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, ushort channelIndex, int nodeId)
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
            switch (sampleFormat)
            {
                case SampleFormat.Adpcm:
                    return CommandType.AdpcmDataSourceVersion2;
                case SampleFormat.PcmInt16:
                    return CommandType.PcmInt16DataSourceVersion2;
                case SampleFormat.PcmFloat:
                    return CommandType.PcmFloatDataSourceVersion2;
                default:
                    throw new NotImplementedException($"{sampleFormat}");
            }
        }

        public void Process(CommandList context)
        {
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            DataSourceHelper.WaveBufferInformation info = new DataSourceHelper.WaveBufferInformation()
            {
                State = State,
                SourceSampleRate = SampleRate,
                SampleFormat = SampleFormat,
                Pitch = Pitch,
                DecodingBehaviour = DecodingBehaviour,
                WaveBuffers = WaveBuffers,
                ExtraParameter = ExtraParameter,
                ExtraParameterSize = ExtraParameterSize,
                ChannelIndex = (int)ChannelIndex,
                ChannelCount = (int)ChannelCount,
                SrcQuality = SrcQuality
            };

            DataSourceHelper.ProcessWaveBuffers(context.MemoryManager, outputBuffer, info, context.SampleRate, (int)context.SampleCount);
        }
    }
}
