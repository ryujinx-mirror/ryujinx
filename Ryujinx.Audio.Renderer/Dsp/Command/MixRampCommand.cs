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
    public class MixRampCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.MixRamp;

        public ulong EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        public float Volume0 { get; }
        public float Volume1 { get; }

        public Memory<VoiceUpdateState> State { get; }

        public int LastSampleIndex { get; }

        public MixRampCommand(float volume0, float volume1, uint inputBufferIndex, uint outputBufferIndex, int lastSampleIndex, Memory<VoiceUpdateState> state, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)inputBufferIndex;
            OutputBufferIndex = (ushort)outputBufferIndex;

            Volume0 = volume0;
            Volume1 = volume1;

            State = state;
            LastSampleIndex = lastSampleIndex;
        }

        private float ProcessMixRamp(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, int sampleCount)
        {
            float ramp = (Volume1 - Volume0) / sampleCount;
            float volume = Volume0;
            float state = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                state = FloatingPointHelper.MultiplyRoundUp(inputBuffer[i], volume);

                outputBuffer[i] += state;
                volume += ramp;
            }

            return state;
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            State.Span[0].LastSamples[LastSampleIndex] = ProcessMixRamp(outputBuffer, inputBuffer, (int)context.SampleCount);
        }
    }
}
