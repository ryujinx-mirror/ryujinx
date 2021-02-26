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

using Ryujinx.Audio.Renderer.Common;
using System;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DepopPrepareCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.DepopPrepare;

        public ulong EstimatedProcessingTime { get; set; }

        public uint MixBufferCount { get; }

        public ushort[] OutputBufferIndices { get; }

        public Memory<VoiceUpdateState> State { get; }
        public Memory<float> DepopBuffer { get; }

        public DepopPrepareCommand(Memory<VoiceUpdateState> state, Memory<float> depopBuffer, uint mixBufferCount, uint bufferOffset, int nodeId, bool enabled)
        {
            Enabled = enabled;
            NodeId = nodeId;
            MixBufferCount = mixBufferCount;

            OutputBufferIndices = new ushort[Constants.MixBufferCountMax];

            for (int i = 0; i < Constants.MixBufferCountMax; i++)
            {
                OutputBufferIndices[i] = (ushort)(bufferOffset + i);
            }

            State = state;
            DepopBuffer = depopBuffer;
        }

        public void Process(CommandList context)
        {
            ref VoiceUpdateState state = ref State.Span[0];

            for (int i = 0; i < MixBufferCount; i++)
            {
                if (state.LastSamples[i] != 0)
                {
                    DepopBuffer.Span[OutputBufferIndices[i]] += state.LastSamples[i];

                    state.LastSamples[i] = 0;
                }
            }
        }
    }
}
