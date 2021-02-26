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

using System;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class VolumeRampCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.VolumeRamp;

        public ulong EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        public float Volume0 { get; }
        public float Volume1 { get; }

        public VolumeRampCommand(float volume0, float volume1, uint bufferIndex, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)bufferIndex;
            OutputBufferIndex = (ushort)bufferIndex;

            Volume0 = volume0;
            Volume1 = volume1;
        }

        private void ProcessVolumeRamp(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, int sampleCount)
        {
            float ramp = (Volume1 - Volume0) / sampleCount;

            float volume = Volume0;

            for (int i = 0; i < sampleCount; i++)
            {
                outputBuffer[i] = FloatingPointHelper.MultiplyRoundUp(inputBuffer[i], volume);
                volume += ramp;
            }
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            ProcessVolumeRamp(outputBuffer, inputBuffer, (int)context.SampleCount);
        }
    }
}
