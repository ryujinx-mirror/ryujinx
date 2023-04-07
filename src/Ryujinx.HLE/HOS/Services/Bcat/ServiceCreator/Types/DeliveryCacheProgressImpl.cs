using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x200)]
    public struct DeliveryCacheProgressImpl
    {
        public enum Status
        {
            // TODO: determine other values
            Done = 9
        }

        public Status State;
        public uint   Result;
        // TODO: reverse the rest of the structure
    }
}
