using Microsoft.IO;
using System;

namespace Ryujinx.Common.Memory
{
    public static class MemoryStreamManager
    {
        private static readonly RecyclableMemoryStreamManager _shared = new();

        /// <summary>
        /// We don't expose the <c>RecyclableMemoryStreamManager</c> directly because version 2.x
        /// returns them as <c>MemoryStream</c>. This Shared class is here to a) offer only the GetStream() versions we use
        /// and b) return them as <c>RecyclableMemoryStream</c> so we don't have to cast.
        /// </summary>
        public static class Shared
        {
            /// <summary>
            /// Retrieve a new <c>MemoryStream</c> object with no tag and a default initial capacity.
            /// </summary>
            /// <returns>A <c>RecyclableMemoryStream</c></returns>
            public static RecyclableMemoryStream GetStream()
                => new(_shared);

            /// <summary>
            /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
            /// buffer. The provided buffer is not wrapped or used after construction.
            /// </summary>
            /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
            /// <param name="buffer">The byte buffer to copy data from</param>
            /// <returns>A <c>RecyclableMemoryStream</c></returns>
            public static RecyclableMemoryStream GetStream(byte[] buffer)
                => GetStream(Guid.NewGuid(), null, buffer, 0, buffer.Length);

            /// <summary>
            /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
            /// buffer. The provided buffer is not wrapped or used after construction.
            /// </summary>
            /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
            /// <param name="buffer">The byte buffer to copy data from</param>
            /// <returns>A <c>RecyclableMemoryStream</c></returns>
            public static RecyclableMemoryStream GetStream(ReadOnlySpan<byte> buffer)
                => GetStream(Guid.NewGuid(), null, buffer);

            /// <summary>
            /// Retrieve a new <c>RecyclableMemoryStream</c> object with the given tag and with contents copied from the provided
            /// buffer. The provided buffer is not wrapped or used after construction.
            /// </summary>
            /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
            /// <param name="id">A unique identifier which can be used to trace usages of the stream</param>
            /// <param name="tag">A tag which can be used to track the source of the stream</param>
            /// <param name="buffer">The byte buffer to copy data from</param>
            /// <returns>A <c>RecyclableMemoryStream</c></returns>
            public static RecyclableMemoryStream GetStream(Guid id, string tag, ReadOnlySpan<byte> buffer)
            {
                RecyclableMemoryStream stream = null;
                try
                {
                    stream = new RecyclableMemoryStream(_shared, id, tag, buffer.Length);
                    stream.Write(buffer);
                    stream.Position = 0;
                    return stream;
                }
                catch
                {
                    stream?.Dispose();
                    throw;
                }
            }

            /// <summary>
            /// Retrieve a new <c>RecyclableMemoryStream</c> object with the given tag and with contents copied from the provided
            /// buffer. The provided buffer is not wrapped or used after construction.
            /// </summary>
            /// <remarks>The new stream's position is set to the beginning of the stream when returned</remarks>
            /// <param name="id">A unique identifier which can be used to trace usages of the stream</param>
            /// <param name="tag">A tag which can be used to track the source of the stream</param>
            /// <param name="buffer">The byte buffer to copy data from</param>
            /// <param name="offset">The offset from the start of the buffer to copy from</param>
            /// <param name="count">The number of bytes to copy from the buffer</param>
            /// <returns>A <c>RecyclableMemoryStream</c></returns>
            public static RecyclableMemoryStream GetStream(Guid id, string tag, byte[] buffer, int offset, int count)
            {
                RecyclableMemoryStream stream = null;
                try
                {
                    stream = new RecyclableMemoryStream(_shared, id, tag, count);
                    stream.Write(buffer, offset, count);
                    stream.Position = 0;
                    return stream;
                }
                catch
                {
                    stream?.Dispose();
                    throw;
                }
            }
        }
    }
}
