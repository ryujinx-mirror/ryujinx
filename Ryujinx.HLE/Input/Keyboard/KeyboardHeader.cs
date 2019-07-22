using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardHeader
    {
        public long Timestamp;
        public long EntryCount;
        public long CurrentEntryIndex;
        public long MaxEntries;
    }
}
