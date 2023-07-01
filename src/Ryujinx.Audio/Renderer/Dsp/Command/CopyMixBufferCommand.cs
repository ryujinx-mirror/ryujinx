namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class CopyMixBufferCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.CopyMixBuffer;

        public uint EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        public CopyMixBufferCommand(uint inputBufferIndex, uint outputBufferIndex, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)inputBufferIndex;
            OutputBufferIndex = (ushort)outputBufferIndex;
        }

        public void Process(CommandList context)
        {
            context.CopyBuffer(OutputBufferIndex, InputBufferIndex);
        }
    }
}
