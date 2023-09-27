using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Ngc
{
    [StructLayout(LayoutKind.Sequential, Size = 0x14, Pack = 0x4)]
    readonly struct ProfanityFilterOption
    {
        public readonly SkipMode SkipAtSignCheck;
        public readonly MaskMode MaskMode;
        public readonly ProfanityFilterFlags Flags;
        public readonly uint SystemRegionMask;
        public readonly uint Reserved;

        public ProfanityFilterOption(SkipMode skipAtSignCheck, MaskMode maskMode, ProfanityFilterFlags flags, uint systemRegionMask)
        {
            SkipAtSignCheck = skipAtSignCheck;
            MaskMode = maskMode;
            Flags = flags;
            SystemRegionMask = systemRegionMask;
            Reserved = 0;
        }
    }
}
