using ARMeilleure.State;

namespace ARMeilleure.Translation
{
    struct RejitRequest
    {
        public ulong Address;
        public ExecutionMode Mode;

        public RejitRequest(ulong address, ExecutionMode mode)
        {
            Address = address;
            Mode = mode;
        }
    }
}
