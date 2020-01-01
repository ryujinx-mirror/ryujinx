namespace Ryujinx.Graphics.VDec
{
    unsafe struct FFmpegFrame
    {
        public int Width;
        public int Height;

        public byte* LumaPtr;
        public byte* ChromaBPtr;
        public byte* ChromaRPtr;

        public byte[] Data;
    }
}