using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Bcat.Ipc.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x200)]
    public struct DeliveryCacheProgressImpl
    {
        public enum Status
        {
            // TODO: determine other values
            Done = 9,
        }

        public Status State;
        public uint Result;
        // TODO: reverse the rest of the structure
    }
}
