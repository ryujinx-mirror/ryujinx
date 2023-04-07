using System;

namespace Ryujinx.Graphics.Gpu.Shader.HashTable
{
    /// <summary>
    /// Data accessor, used by <see cref="PartitionedHashTable{T}"/> to access data of unknown length.
    /// </summary>
    /// <remarks>
    /// This will be used to access chuncks of data and try finding a match on the table.
    /// This is necessary because the data size is assumed to be unknown, and so the
    /// hash table must try to "guess" the size of the data based on the entries on the table.
    /// </remarks>
    public interface IDataAccessor
    {
        /// <summary>
        /// Gets a span of shader code at the specified offset, with at most the specified size.
        /// </summary>
        /// <remarks>
        /// This might return a span smaller than the requested <paramref name="length"/> if there's
        /// no more code available.
        /// </remarks>
        /// <param name="offset">Offset in shader code</param>
        /// <param name="length">Size in bytes</param>
        /// <returns>Shader code span</returns>
        ReadOnlySpan<byte> GetSpan(int offset, int length);
    }
}
