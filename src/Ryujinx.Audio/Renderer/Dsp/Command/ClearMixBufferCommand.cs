namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class ClearMixBufferCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.ClearMixBuffer;

        public uint EstimatedProcessingTime { get; set; }

        public ClearMixBufferCommand(int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;
        }

        public void Process(CommandList context)
        {
            context.ClearBuffers();
        }
    }
}
