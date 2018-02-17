using System.Runtime.InteropServices;

namespace Ryujinx
{
    [StructLayout(LayoutKind.Sequential, Size = 0x400)]
    public struct HidSharedMemHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x400)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x400)]
    public struct HidUnknownSection1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x400)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x400)]
    public struct HidUnknownSection2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x400)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x400)]
    public struct HidUnknownSection3
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x400)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x400)]
    public struct HidUnknownSection4
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x400)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x200)]
    public struct HidUnknownSection5
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x200)]
    public struct HidUnknownSection6
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x200)]
    public struct HidUnknownSection7
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x800)]
    public struct HidUnknownSection8
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x800)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x4000)]
    public struct HidControllerSerials
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4000)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x4600)]
    public struct HidUnknownSection9
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4600)]
        public byte[] Padding;
    }
}
