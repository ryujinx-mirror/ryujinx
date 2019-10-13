namespace Ryujinx.Graphics
{
    struct MethodParams
    {
        public int Method      { get; private set; }
        public int Argument    { get; private set; }
        public int SubChannel  { get; private set; }
        public int MethodCount { get; private set; }

        public bool IsLastCall => MethodCount <= 1;

        public MethodParams(
            int method,
            int argument,
            int subChannel  = 0,
            int methodCount = 0)
        {
            Method      = method;
            Argument    = argument;
            SubChannel  = subChannel;
            MethodCount = methodCount;
        }
    }
}