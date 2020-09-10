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
        public static FormatInfo Default { get; } = new FormatInfo(Format.R8G8B8A8Unorm, 1, 1, 4, 4);

        /// <summary>
        /// The format of the texture data.
        /// </summary>
        public Format Format { get; }

        /// <summary>
        /// The block width for compressed formats.
        /// </summary>
        /// <remarks>
        /// Must be 1 for non-compressed formats.
        /// </remarks>
        public int BlockWidth { get; }

        /// <summary>
        /// The block height for compressed formats.
        /// </summary>
        /// <remarks>
        /// Must be 1 for non-compressed formats.
        /// </remarks>
        public int BlockHeight { get; }

        /// <summary>
        /// The number of bytes occupied by a single pixel in memory of the texture data.
        /// </summary>
        public int BytesPerPixel { get; }

        /// <summary>
        /// The maximum number of components this format has defined (in RGBA order).
        /// </summary>
        public int Components { get; }

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
            int    bytesPerPixel,
            int    components)
        {
            Format        = format;
            BlockWidth    = blockWidth;
            BlockHeight   = blockHeight;
            BytesPerPixel = bytesPerPixel;
            Components    = components;
        }
    }
}