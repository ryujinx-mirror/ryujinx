namespace Ryujinx.Cpu.LightningJit
{
    public readonly struct CpuPreset
    {
        public readonly IsaVersion Version;
        public readonly IsaFeature Features;

        public CpuPreset(IsaVersion version, IsaFeature features)
        {
            Version = version;
            Features = features;
        }
    }
}
