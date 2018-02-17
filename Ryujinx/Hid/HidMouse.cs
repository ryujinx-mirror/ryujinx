using System.Runtime.InteropServices;

namespace Ryujinx
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public struct HidMouseHeader
    {
        public ulong TimestampTicks;
        public ulong NumEntries;
        public ulong LatestEntry;
        public ulong MaxEntryIndex;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x30)]
    public struct HidMouseEntry
    {
        public ulong Timestamp;
        public ulong Timestamp_2;
        public uint X;
        public uint Y;
        public uint VelocityX;
        public uint VelocityY;
        public uint ScrollVelocityX;
        public uint ScrollVelocityY;
        public ulong Buttons;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x400)]
    public struct HidMouse
    {
        public HidMouseHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public HidMouseEntry[] Entries;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xB0)]
        public byte[] Padding;
    }
}
