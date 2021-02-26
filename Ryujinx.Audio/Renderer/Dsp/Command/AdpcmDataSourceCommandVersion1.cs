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
    public class AdpcmDataSourceCommandVersion1 : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.AdpcmDataSourceVersion1;

        public ulong EstimatedProcessingTime { get; set; }

        public ushort OutputBufferIndex { get; }
        public uint SampleRate { get; }

        public float Pitch { get; }

        public WaveBuffer[] WaveBuffers { get; }

        public Memory<VoiceUpdateState> State { get; }

        public ulong AdpcmParameter { get; }
        public ulong AdpcmParameterSize { get; }

        public DecodingBehaviour DecodingBehaviour { get; }

        public AdpcmDataSourceCommandVersion1(ref Server.Voice.VoiceState serverState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, int nodeId)
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

            DataSourceHelper.WaveBufferInformation info = new DataSourceHelper.WaveBufferInformation()
            {
                State = State,
                SourceSampleRate = SampleRate,
                SampleFormat = SampleFormat.Adpcm,
                Pitch = Pitch,
                DecodingBehaviour = DecodingBehaviour,
                WaveBuffers = WaveBuffers,
                ExtraParameter = AdpcmParameter,
                ExtraParameterSize = AdpcmParameterSize,
                ChannelIndex = 0,
                ChannelCount = 1,
            };

            DataSourceHelper.ProcessWaveBuffers(context.MemoryManager, outputBuffer, info, context.SampleRate, (int)context.SampleCount);
        }
    }
}
