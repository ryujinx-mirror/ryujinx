using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [StructLayout(LayoutKind.Sequential, Size = 0x18, Pack = 0x4)]
    struct PtmFuelGaugeParameter
    {
        public ushort Rcomp0;
        public ushort TempCo;
        public ushort FullCap;
        public ushort FullCapNom;
        public ushort IavgEmpty;
        public ushort QrTable00;
        public ushort QrTable10;
        public ushort QrTable20;
        public ushort QrTable30;
        public ushort Reserved;
        public uint Cycles;
    }
}
