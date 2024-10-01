namespace Ryujinx.Cpu.LightningJit.Table
{
    interface IInstInfo
    {
        public uint Encoding { get; }
        public uint EncodingMask { get; }
        public IsaVersion Version { get; }
        public IsaFeature Feature { get; }

        bool IsConstrained(uint encoding);
    }
}
