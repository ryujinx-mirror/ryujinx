using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager
{
    [StructLayout(LayoutKind.Sequential, Size = 0x40)]
    struct ModelInfo
    {
        public ushort CharacterId;
        public byte CharacterVariant;
        public byte Series;
        public ushort ModelNumber;
        public byte Type;
        public Array57<byte> Reserved;
    }
}
