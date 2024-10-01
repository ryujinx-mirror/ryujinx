namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    struct MatchCheckState
    {
        public uint CheckMask;
        public readonly uint RegionMask;
        public readonly ProfanityFilterOption Option;

        public MatchCheckState(uint checkMask, uint regionMask, ProfanityFilterOption option)
        {
            CheckMask = checkMask;
            RegionMask = regionMask;
            Option = option;
        }
    }
}
