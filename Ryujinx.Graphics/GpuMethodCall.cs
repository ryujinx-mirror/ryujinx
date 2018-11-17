namespace Ryujinx.Graphics
{
    struct GpuMethodCall
    {
        public int Method      { get; private set; }
        public int Argument    { get; private set; }
        public int SubChannel  { get; private set; }
        public int MethodCount { get; private set; }

        public bool IsLastCall => MethodCount <= 1;

        public GpuMethodCall(
            int Method,
            int Argument,
            int SubChannel  = 0,
            int MethodCount = 0)
        {
            this.Method      = Method;
            this.Argument    = Argument;
            this.SubChannel  = SubChannel;
            this.MethodCount = MethodCount;
        }
    }
}