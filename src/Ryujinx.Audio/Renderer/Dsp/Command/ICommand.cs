namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public interface ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType { get; }

        public uint EstimatedProcessingTime { get; }

        public void Process(CommandList context);

        public bool ShouldMeter()
        {
            return false;
        }
    }
}
