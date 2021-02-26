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
    public class MixRampGroupedCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.MixRampGrouped;

        public ulong EstimatedProcessingTime { get; set; }

        public uint MixBufferCount { get; }

        public ushort[] InputBufferIndices { get; }
        public ushort[] OutputBufferIndices { get; }

        public float[] Volume0 { get; }
        public float[] Volume1 { get; }

        public Memory<VoiceUpdateState> State { get; }

        public MixRampGroupedCommand(uint mixBufferCount, uint inputBufferIndex, uint outputBufferIndex, Span<float> volume0, Span<float> volume1, Memory<VoiceUpdateState> state, int nodeId)
        {
            Enabled = true;
            MixBufferCount = mixBufferCount;
            NodeId = nodeId;

            InputBufferIndices = new ushort[Constants.MixBufferCountMax];
            OutputBufferIndices = new ushort[Constants.MixBufferCountMax];
            Volume0 = new float[Constants.MixBufferCountMax];
            Volume1 = new float[Constants.MixBufferCountMax];

            for (int i = 0; i < mixBufferCount; i++)
            {
                InputBufferIndices[i] = (ushort)inputBufferIndex;
                OutputBufferIndices[i] = (ushort)(outputBufferIndex + i);

                Volume0[i] = volume0[i];
                Volume1[i] = volume1[i];
            }

            State = state;
        }

        private float ProcessMixRampGrouped(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, float volume0, float volume1, int sampleCount)
        {
            float ramp = (volume1 - volume0) / sampleCount;
            float volume = volume0;
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
            for (int i = 0; i < MixBufferCount; i++)
            {
                ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndices[i]);
                Span<float> outputBuffer = context.GetBuffer(OutputBufferIndices[i]);

                float volume0 = Volume0[i];
                float volume1 = Volume1[i];

                ref VoiceUpdateState state = ref State.Span[0];

                if (volume0 != 0 || volume1 != 0)
                {
                    state.LastSamples[i] = ProcessMixRampGrouped(outputBuffer, inputBuffer, volume0, volume1, (int)context.SampleCount);
                }
                else
                {
                    state.LastSamples[i] = 0;
                }
            }
        }
    }
}
