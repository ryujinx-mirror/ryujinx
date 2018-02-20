using System.Runtime.InteropServices;

namespace Ryujinx.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public struct HidKeyboardHeader
    {
        public ulong TimestampTicks;
        public ulong NumEntries;
        public ulong LatestEntry;
        public ulong MaxEntryIndex;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x38)]
    public struct HidKeyboardEntry
    {
        public ulong Timestamp;
        public ulong Timestamp_2;
        public ulong Modifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] Keys;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x400)]
    public struct HidKeyboard
    {
        public HidKeyboardHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public HidKeyboardEntry[] Entries;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x28)]
        public byte[] Padding;
    }
}
