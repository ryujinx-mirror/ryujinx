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

        public uint EstimatedProcessingTime { get; set; }

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

            // NOTE: Nintendo only implement single and double biquad filters but no generic path when the command definition suggests it could be done.
            // As such we currently only implement a generic path for simplicity for double biquad.
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