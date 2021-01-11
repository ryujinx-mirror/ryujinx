using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct SoftwareKeyboardDictSet
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public uint[] Entries;
    }
}
