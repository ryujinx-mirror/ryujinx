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
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class BiquadFilterCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.BiquadFilter;

        public ulong EstimatedProcessingTime { get; set; }

        public Memory<BiquadFilterState> BiquadFilterState { get; }
        public int InputBufferIndex { get; }
        public int OutputBufferIndex { get; }
        public bool NeedInitialization { get; }

        private BiquadFilterParameter _parameter;

        public BiquadFilterCommand(int baseIndex, ref BiquadFilterParameter filter, Memory<BiquadFilterState> biquadFilterStateMemory, int inputBufferOffset, int outputBufferOffset, bool needInitialization, int nodeId)
        {
            _parameter = filter;
            BiquadFilterState = biquadFilterStateMemory;
            InputBufferIndex = baseIndex + inputBufferOffset;
            OutputBufferIndex = baseIndex + outputBufferOffset;
            NeedInitialization = needInitialization;

            Enabled = true;
            NodeId = nodeId;
        }

        public void Process(CommandList context)
        {
            ref BiquadFilterState state = ref BiquadFilterState.Span[0];

            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            if (NeedInitialization)
            {
                state = new BiquadFilterState();
            }

            BiquadFilterHelper.ProcessBiquadFilter(ref _parameter, ref state, outputBuffer, inputBuffer, context.SampleCount);
        }
    }
}
