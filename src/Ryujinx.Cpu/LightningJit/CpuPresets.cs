namespace Ryujinx.Cpu.LightningJit
{
    public static class CpuPresets
    {
        public static CpuPreset CortexA57 => new(
            IsaVersion.v80,
            IsaFeature.FeatAes |
            IsaFeature.FeatCrc32 |
            IsaFeature.FeatSha1 |
            IsaFeature.FeatSha256 |
            IsaFeature.FeatPmull);
    }
}
