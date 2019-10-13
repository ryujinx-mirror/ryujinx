using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Image
{
    struct FormatInfo
    {
        private static FormatInfo _rgba8 = new FormatInfo(Format.R8G8B8A8Unorm, 1, 1, 4);

        public static FormatInfo Default => _rgba8;

        public Format Format { get; }

        public int BlockWidth    { get; }
        public int BlockHeight   { get; }
        public int BytesPerPixel { get; }

        public bool IsCompressed => (BlockWidth | BlockHeight) != 1;

        public FormatInfo(
            Format format,
            int    blockWidth,
            int    blockHeight,
            int    bytesPerPixel)
        {
            Format        = format;
            BlockWidth    = blockWidth;
            BlockHeight   = blockHeight;
            BytesPerPixel = bytesPerPixel;
        }
    }
}