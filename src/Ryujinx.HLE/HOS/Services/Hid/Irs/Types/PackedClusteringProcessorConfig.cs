using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Hid.Irs.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x28)]
    struct PackedClusteringProcessorConfig
    {
        public long ExposureTime;
        public byte LightTarget;
        public byte Gain;
        public byte IsNegativeImageUsed;
        public byte Reserved1;
        public uint Reserved2;
        public ushort WindowOfInterestX;
        public ushort WindowOfInterestY;
        public ushort WindowOfInterestWidth;
        public ushort WindowOfInterestHeight;
        public uint RequiredMcuVersion;
        public uint ObjectPixelCountMin;
        public uint ObjectPixelCountMax;
        public byte ObjectIntensityMin;
        public byte IsExternalLightFilterEnabled;
        public ushort Reserved3;
    }
}
