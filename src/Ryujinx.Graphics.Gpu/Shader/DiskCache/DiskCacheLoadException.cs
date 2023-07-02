using System;

namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Disk cache load exception.
    /// </summary>
    class DiskCacheLoadException : Exception
    {
        /// <summary>
        /// Result of the cache load operation.
        /// </summary>
        public DiskCacheLoadResult Result { get; }

        /// <summary>
        /// Creates a new instance of the disk cache load exception.
        /// </summary>
        public DiskCacheLoadException()
        {
        }

        /// <summary>
        /// Creates a new instance of the disk cache load exception.
        /// </summary>
        /// <param name="message">Exception message</param>
        public DiskCacheLoadException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the disk cache load exception.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public DiskCacheLoadException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates a new instance of the disk cache load exception.
        /// </summary>
        /// <param name="result">Result code</param>
        public DiskCacheLoadException(DiskCacheLoadResult result) : base(result.GetMessage())
        {
            Result = result;
        }
    }
}
