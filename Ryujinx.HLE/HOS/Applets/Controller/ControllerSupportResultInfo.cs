using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets
{
#pragma warning disable CS0649
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    unsafe struct ControllerSupportResultInfo
    {
        public sbyte PlayerCount;
        fixed byte _padding[3];
        public uint SelectedId;
        public uint Result;
    }
#pragma warning restore CS0649
}