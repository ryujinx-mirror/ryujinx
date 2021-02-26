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

using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using System;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class BiquadFilterCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.BiquadFilter;

        public ulong EstimatedProcessingTime { get; set; }

        public BiquadFilterParameter Parameter { get; }
        public Memory<BiquadFilterState> BiquadFilterState { get; }
        public int InputBufferIndex { get; }
        public int OutputBufferIndex { get; }
        public bool NeedInitialization { get; }

        public BiquadFilterCommand(int baseIndex, ref BiquadFilterParameter filter, Memory<BiquadFilterState> biquadFilterStateMemory, int inputBufferOffset, int outputBufferOffset, bool needInitialization, int nodeId)
        {
            Parameter = filter;
            BiquadFilterState = biquadFilterStateMemory;
            InputBufferIndex = baseIndex + inputBufferOffset;
            OutputBufferIndex = baseIndex + outputBufferOffset;
            NeedInitialization = needInitialization;

            Enabled = true;
            NodeId = nodeId;
        }

        private void ProcessBiquadFilter(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, uint sampleCount)
        {
            const int fixedPointPrecisionForParameter = 14;

            float a0 = FixedPointHelper.ToFloat(Parameter.Numerator[0], fixedPointPrecisionForParameter);
            float a1 = FixedPointHelper.ToFloat(Parameter.Numerator[1], fixedPointPrecisionForParameter);
            float a2 = FixedPointHelper.ToFloat(Parameter.Numerator[2], fixedPointPrecisionForParameter);

            float b1 = FixedPointHelper.ToFloat(Parameter.Denominator[0], fixedPointPrecisionForParameter);
            float b2 = FixedPointHelper.ToFloat(Parameter.Denominator[1], fixedPointPrecisionForParameter);

            ref BiquadFilterState state = ref BiquadFilterState.Span[0];

            for (int i = 0; i < sampleCount; i++)
            {
                float input = inputBuffer[i];
                float output = input * a0 + state.Z1;

                state.Z1 = input * a1 + output * b1 + state.Z2;
                state.Z2 = input * a2 + output * b2;

                outputBuffer[i] = output;
            }
        }

        public void Process(CommandList context)
        {
            Span<float> outputBuffer = context.GetBuffer(InputBufferIndex);

            if (NeedInitialization)
            {
                BiquadFilterState.Span[0] = new BiquadFilterState();
            }

            ProcessBiquadFilter(outputBuffer, outputBuffer, context.SampleCount);
        }
    }
}
