namespace Ryujinx.Graphics.Nvdec
{
    public readonly struct FrameDecodedEventArgs
    {
        public CodecId CodecId { get; }
        public uint LumaOffset { get; }
        public uint ChromaOffset { get; }

        internal FrameDecodedEventArgs(CodecId codecId, uint lumaOffset, uint chromaOffset)
        {
            CodecId = codecId;
            LumaOffset = lumaOffset;
            ChromaOffset = chromaOffset;
        }
    }
}
