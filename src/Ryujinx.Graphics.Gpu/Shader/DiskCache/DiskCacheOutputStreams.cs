using System;
using System.IO;

namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Output streams for the disk shader cache.
    /// </summary>
    class DiskCacheOutputStreams : IDisposable
    {
        /// <summary>
        /// Shared table of contents (TOC) file stream.
        /// </summary>
        public readonly FileStream TocFileStream;

        /// <summary>
        /// Shared data file stream.
        /// </summary>
        public readonly FileStream DataFileStream;

        /// <summary>
        /// Host table of contents (TOC) file stream.
        /// </summary>
        public readonly FileStream HostTocFileStream;

        /// <summary>
        /// Host data file stream.
        /// </summary>
        public readonly FileStream HostDataFileStream;

        /// <summary>
        /// Creates a new instance of a disk cache output stream container.
        /// </summary>
        /// <param name="tocFileStream">Stream for the shared table of contents file</param>
        /// <param name="dataFileStream">Stream for the shared data file</param>
        /// <param name="hostTocFileStream">Stream for the host table of contents file</param>
        /// <param name="hostDataFileStream">Stream for the host data file</param>
        public DiskCacheOutputStreams(FileStream tocFileStream, FileStream dataFileStream, FileStream hostTocFileStream, FileStream hostDataFileStream)
        {
            TocFileStream = tocFileStream;
            DataFileStream = dataFileStream;
            HostTocFileStream = hostTocFileStream;
            HostDataFileStream = hostDataFileStream;
        }

        /// <summary>
        /// Disposes the output file streams.
        /// </summary>
        public void Dispose()
        {
            TocFileStream.Dispose();
            DataFileStream.Dispose();
            HostTocFileStream.Dispose();
            HostDataFileStream.Dispose();
        }
    }
}
