namespace Ryujinx.Cpu.LightningJit.Table
{
    readonly struct InstEncoding
    {
        public readonly uint Encoding;
        public readonly uint EncodingMask;

        public InstEncoding(uint encoding, uint encodingMask)
        {
            Encoding = encoding;
            EncodingMask = encodingMask;
        }
    }
}
