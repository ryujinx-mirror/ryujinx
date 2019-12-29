using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Represents texture format information.
    /// </summary>
    struct FormatInfo
    {
        /// <summary>
        /// A default, generic RGBA8 texture format.
        /// </summary>
        public static FormatInfo Default { get; } = new FormatInfo(Format.R8G8B8A8Unorm, 1, 1, 4);

        /// <summary>
        /// The format of the texture data.
        /// </summary>
        public Format Format { get; }

        /// <summary>
        /// The block width for compressed formats. Must be 1 for non-compressed formats.
        /// </summary>
        public int BlockWidth { get; }

        /// <summary>
        /// The block height for compressed formats. Must be 1 for non-compressed formats.
        /// </summary>
        public int BlockHeight { get; }

        /// <summary>
        /// The number of bytes occupied by a single pixel in memory of the texture data.
        /// </summary>
        public int BytesPerPixel { get; }

        /// <summary>
        /// Whenever or not the texture format is a compressed format. Determined from block size.
        /// </summary>
        public bool IsCompressed => (BlockWidth | BlockHeight) != 1;

        /// <summary>
        /// Constructs the texture format info structure.
        /// </summary>
        /// <param name="format">The format of the texture data</param>
        /// <param name="blockWidth">The block width for compressed formats. Must be 1 for non-compressed formats</param>
        /// <param name="blockHeight">The block height for compressed formats. Must be 1 for non-compressed formats</param>
        /// <param name="bytesPerPixel">The number of bytes occupied by a single pixel in memory of the texture data</param>
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