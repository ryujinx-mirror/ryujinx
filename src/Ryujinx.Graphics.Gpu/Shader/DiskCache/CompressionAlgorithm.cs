namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Algorithm used to compress the cache.
    /// </summary>
    enum CompressionAlgorithm : byte
    {
        /// <summary>
        /// No compression, the data is stored as-is.
        /// </summary>
        None,

        /// <summary>
        /// Deflate compression (RFC 1951).
        /// </summary>
        Deflate,

        /// <summary>
        /// Brotli compression (RFC 7932).
        /// </summary>
        Brotli,
    }
}
