using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// A record of when buffer data was copied from one buffer to another, along with the SyncNumber when the migration will be complete.
    /// Keeps the source buffer alive for data flushes until the migration is complete.
    /// </summary>
    internal class BufferMigration : IDisposable
    {
        /// <summary>
        /// The offset for the migrated region.
        /// </summary>
        private readonly ulong _offset;

        /// <summary>
        /// The size for the migrated region.
        /// </summary>
        private readonly ulong _size;

        /// <summary>
        /// The buffer that was migrated from.
        /// </summary>
        private readonly Buffer _buffer;

        /// <summary>
        /// The source range action, to be called on overlap with an unreached sync number.
        /// </summary>
        private readonly Action<ulong, ulong> _sourceRangeAction;

        /// <summary>
        /// The source range list.
        /// </summary>
        private readonly BufferModifiedRangeList _source;

        /// <summary>
        /// The destination range list. This range list must be updated when flushing the source.
        /// </summary>
        public readonly BufferModifiedRangeList Destination;

        /// <summary>
        /// The sync number that needs to be reached for this migration to be removed. This is set to the pending sync number on creation.
        /// </summary>
        public readonly ulong SyncNumber;

        /// <summary>
        /// Creates a record for a buffer migration.
        /// </summary>
        /// <param name="buffer">The source buffer for this migration</param>
        /// <param name="sourceRangeAction">The flush action for the source buffer</param>
        /// <param name="source">The modified range list for the source buffer</param>
        /// <param name="dest">The modified range list for the destination buffer</param>
        /// <param name="syncNumber">The sync number for when the migration is complete</param>
        public BufferMigration(
            Buffer buffer,
            Action<ulong, ulong> sourceRangeAction,
            BufferModifiedRangeList source,
            BufferModifiedRangeList dest,
            ulong syncNumber)
        {
            _offset = buffer.Address;
            _size = buffer.Size;
            _buffer = buffer;
            _sourceRangeAction = sourceRangeAction;
            _source = source;
            Destination = dest;
            SyncNumber = syncNumber;
        }

        /// <summary>
        /// Determine if the given range overlaps this migration, and has not been completed yet.
        /// </summary>
        /// <param name="offset">Start offset</param>
        /// <param name="size">Range size</param>
        /// <param name="syncNumber">The sync number that was waited on</param>
        /// <returns>True if overlapping and in progress, false otherwise</returns>
        public bool Overlaps(ulong offset, ulong size, ulong syncNumber)
        {
            ulong end = offset + size;
            ulong destEnd = _offset + _size;
            long syncDiff = (long)(syncNumber - SyncNumber); // syncNumber is less if the copy has not completed.

            return !(end <= _offset || offset >= destEnd) && syncDiff < 0;
        }

        /// <summary>
        /// Determine if the given range matches this migration.
        /// </summary>
        /// <param name="offset">Start offset</param>
        /// <param name="size">Range size</param>
        /// <returns>True if the range exactly matches, false otherwise</returns>
        public bool FullyMatches(ulong offset, ulong size)
        {
            return _offset == offset && _size == size;
        }

        /// <summary>
        /// Perform the migration source's range action on the range provided, clamped to the bounds of the source buffer.
        /// </summary>
        /// <param name="offset">Start offset</param>
        /// <param name="size">Range size</param>
        /// <param name="syncNumber">Current sync number</param>
        /// <param name="parent">The modified range list that originally owned this range</param>
        public void RangeActionWithMigration(ulong offset, ulong size, ulong syncNumber, BufferModifiedRangeList parent)
        {
            ulong end = offset + size;
            end = Math.Min(_offset + _size, end);
            offset = Math.Max(_offset, offset);

            size = end - offset;

            _source.RangeActionWithMigration(offset, size, syncNumber, parent, _sourceRangeAction);
        }

        /// <summary>
        /// Removes this reference to the range list, potentially allowing for the source buffer to be disposed.
        /// </summary>
        public void Dispose()
        {
            Destination.RemoveMigration(this);

            _buffer.DecrementReferenceCount();
        }
    }
}
