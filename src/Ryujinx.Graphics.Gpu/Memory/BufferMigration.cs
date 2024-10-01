using System;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// A record of when buffer data was copied from multiple buffers to one migration target,
    /// along with the SyncNumber when the migration will be complete.
    /// Keeps the source buffers alive for data flushes until the migration is complete.
    /// All spans cover the full range of the "destination" buffer.
    /// </summary>
    internal class BufferMigration : IDisposable
    {
        /// <summary>
        /// Ranges from source buffers that were copied as part of this migration.
        /// Ordered by increasing base address.
        /// </summary>
        public BufferMigrationSpan[] Spans { get; private set; }

        /// <summary>
        /// The destination range list. This range list must be updated when flushing the source.
        /// </summary>
        public readonly BufferModifiedRangeList Destination;

        /// <summary>
        /// The sync number that needs to be reached for this migration to be removed. This is set to the pending sync number on creation.
        /// </summary>
        public readonly ulong SyncNumber;

        /// <summary>
        /// Number of active users there are traversing this migration's spans.
        /// </summary>
        private int _refCount;

        /// <summary>
        /// Create a new buffer migration.
        /// </summary>
        /// <param name="spans">Source spans for the migration</param>
        /// <param name="destination">Destination buffer range list</param>
        /// <param name="syncNumber">Sync number where this migration will be complete</param>
        public BufferMigration(BufferMigrationSpan[] spans, BufferModifiedRangeList destination, ulong syncNumber)
        {
            Spans = spans;
            Destination = destination;
            SyncNumber = syncNumber;
        }

        /// <summary>
        /// Add a span to the migration. Allocates a new array with the target size, and replaces it.
        /// </summary>
        /// <remarks>
        /// The base address for the span is assumed to be higher than all other spans in the migration,
        /// to keep the span array ordered.
        /// </remarks>
        public void AddSpanToEnd(BufferMigrationSpan span)
        {
            BufferMigrationSpan[] oldSpans = Spans;

            BufferMigrationSpan[] newSpans = new BufferMigrationSpan[oldSpans.Length + 1];

            oldSpans.CopyTo(newSpans, 0);

            newSpans[oldSpans.Length] = span;

            Spans = newSpans;
        }

        /// <summary>
        /// Performs the given range action, or one from a migration that overlaps and has not synced yet.
        /// </summary>
        /// <param name="offset">The offset to pass to the action</param>
        /// <param name="size">The size to pass to the action</param>
        /// <param name="syncNumber">The sync number that has been reached</param>
        /// <param name="rangeAction">The action to perform</param>
        public void RangeActionWithMigration(ulong offset, ulong size, ulong syncNumber, BufferFlushAction rangeAction)
        {
            long syncDiff = (long)(syncNumber - SyncNumber);

            if (syncDiff >= 0)
            {
                // The migration has completed. Run the parent action.
                rangeAction(offset, size, syncNumber);
            }
            else
            {
                Interlocked.Increment(ref _refCount);

                ulong prevAddress = offset;
                ulong endAddress = offset + size;

                foreach (BufferMigrationSpan span in Spans)
                {
                    if (!span.Overlaps(offset, size))
                    {
                        continue;
                    }

                    if (span.Address > prevAddress)
                    {
                        // There's a gap between this span and the last (or the start address). Flush the range using the parent action.

                        rangeAction(prevAddress, span.Address - prevAddress, syncNumber);
                    }

                    span.RangeActionWithMigration(offset, size, syncNumber);

                    prevAddress = span.Address + span.Size;
                }

                if (endAddress > prevAddress)
                {
                    // There's a gap at the end of the range with no migration. Flush the range using the parent action.
                    rangeAction(prevAddress, endAddress - prevAddress, syncNumber);
                }

                Interlocked.Decrement(ref _refCount);
            }
        }

        /// <summary>
        /// Dispose the buffer migration. This removes the reference from the destination range list,
        /// and runs all the dispose buffers for the migration spans. (typically disposes the source buffer)
        /// </summary>
        public void Dispose()
        {
            while (Volatile.Read(ref _refCount) > 0)
            {
                // Coming into this method, the sync for the migration will be met, so nothing can increment the ref count.
                // However, an existing traversal of the spans for data flush could still be in progress.
                // Spin if this is ever the case, so they don't get disposed before the operation is complete.
            }

            Destination.RemoveMigration(this);

            foreach (BufferMigrationSpan span in Spans)
            {
                span.Dispose();
            }
        }
    }

    /// <summary>
    /// A record of when buffer data was copied from one buffer to another, for a specific range in a source buffer.
    /// Keeps the source buffer alive for data flushes until the migration is complete.
    /// </summary>
    internal readonly struct BufferMigrationSpan : IDisposable
    {
        /// <summary>
        /// The offset for the migrated region.
        /// </summary>
        public readonly ulong Address;

        /// <summary>
        /// The size for the migrated region.
        /// </summary>
        public readonly ulong Size;

        /// <summary>
        /// The action to perform when the migration isn't needed anymore.
        /// </summary>
        private readonly Action _disposeAction;

        /// <summary>
        /// The source range action, to be called on overlap with an unreached sync number.
        /// </summary>
        private readonly BufferFlushAction _sourceRangeAction;

        /// <summary>
        /// Optional migration for the source data. Can chain together if many migrations happen in a short time.
        /// If this is null, then _sourceRangeAction will always provide up to date data.
        /// </summary>
        private readonly BufferMigration _source;

        /// <summary>
        /// Creates a record for a buffer migration.
        /// </summary>
        /// <param name="buffer">The source buffer for this migration</param>
        /// <param name="disposeAction">The action to perform when the migration isn't needed anymore</param>
        /// <param name="sourceRangeAction">The flush action for the source buffer</param>
        /// <param name="source">Pending migration for the source buffer</param>
        public BufferMigrationSpan(
            Buffer buffer,
            Action disposeAction,
            BufferFlushAction sourceRangeAction,
            BufferMigration source)
        {
            Address = buffer.Address;
            Size = buffer.Size;
            _disposeAction = disposeAction;
            _sourceRangeAction = sourceRangeAction;
            _source = source;
        }

        /// <summary>
        /// Creates a record for a buffer migration, using the default buffer dispose action.
        /// </summary>
        /// <param name="buffer">The source buffer for this migration</param>
        /// <param name="sourceRangeAction">The flush action for the source buffer</param>
        /// <param name="source">Pending migration for the source buffer</param>
        public BufferMigrationSpan(
            Buffer buffer,
            BufferFlushAction sourceRangeAction,
            BufferMigration source) : this(buffer, buffer.DecrementReferenceCount, sourceRangeAction, source) { }

        /// <summary>
        /// Determine if the given range overlaps this migration, and has not been completed yet.
        /// </summary>
        /// <param name="offset">Start offset</param>
        /// <param name="size">Range size</param>
        /// <returns>True if overlapping and in progress, false otherwise</returns>
        public bool Overlaps(ulong offset, ulong size)
        {
            ulong end = offset + size;
            ulong destEnd = Address + Size;

            return !(end <= Address || offset >= destEnd);
        }

        /// <summary>
        /// Perform the migration source's range action on the range provided, clamped to the bounds of the source buffer.
        /// </summary>
        /// <param name="offset">Start offset</param>
        /// <param name="size">Range size</param>
        /// <param name="syncNumber">Current sync number</param>
        public void RangeActionWithMigration(ulong offset, ulong size, ulong syncNumber)
        {
            ulong end = offset + size;
            end = Math.Min(Address + Size, end);
            offset = Math.Max(Address, offset);

            size = end - offset;

            if (_source != null)
            {
                _source.RangeActionWithMigration(offset, size, syncNumber, _sourceRangeAction);
            }
            else
            {
                _sourceRangeAction(offset, size, syncNumber);
            }
        }

        /// <summary>
        /// Removes this migration span, potentially allowing for the source buffer to be disposed.
        /// </summary>
        public void Dispose()
        {
            _disposeAction();
        }
    }
}
