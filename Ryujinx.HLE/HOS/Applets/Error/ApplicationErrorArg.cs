using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.Error
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct ApplicationErrorArg
    {
        public uint       ErrorNumber;
        public ulong      LanguageCode;
        public fixed byte MessageText[0x800];
        public fixed byte DetailsText[0x800];
    }
} 