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
    public class GroupedBiquadFilterCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.GroupedBiquadFilter;

        public ulong EstimatedProcessingTime { get; set; }

        private BiquadFilterParameter[] _parameters;
        private Memory<BiquadFilterState> _biquadFilterStates;
        private int _inputBufferIndex;
        private int _outputBufferIndex;
        private bool[] _isInitialized;

        public GroupedBiquadFilterCommand(int baseIndex, ReadOnlySpan<BiquadFilterParameter> filters, Memory<BiquadFilterState> biquadFilterStateMemory, int inputBufferOffset, int outputBufferOffset, ReadOnlySpan<bool> isInitialized, int nodeId)
        {
            _parameters = filters.ToArray();
            _biquadFilterStates = biquadFilterStateMemory;
            _inputBufferIndex = baseIndex + inputBufferOffset;
            _outputBufferIndex = baseIndex + outputBufferOffset;
            _isInitialized = isInitialized.ToArray();

            Enabled = true;
            NodeId = nodeId;
        }

        public void Process(CommandList context)
        {
            Span<BiquadFilterState> states = _biquadFilterStates.Span;

            ReadOnlySpan<float> inputBuffer = context.GetBuffer(_inputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(_outputBufferIndex);

            for (int i = 0; i < _parameters.Length; i++)
            {
                if (!_isInitialized[i])
                {
                    states[i] = new BiquadFilterState();
                }
            }

            // NOTE: Nintendo also implements a hot path for double biquad filters, but no generic path when the command definition suggests it could be done.
            // As such we currently only implement a generic path for simplicity.
            // TODO: Implement double biquad filters fast path.
            if (_parameters.Length == 1)
            {
                BiquadFilterHelper.ProcessBiquadFilter(ref _parameters[0], ref states[0], outputBuffer, inputBuffer, context.SampleCount);
            }
            else
            {
                BiquadFilterHelper.ProcessBiquadFilter(_parameters, states, outputBuffer, inputBuffer, context.SampleCount);
            }
        }
    }
}
