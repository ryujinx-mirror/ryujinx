using System.Runtime.InteropServices;
using System.Threading;
using static Ryujinx.Common.Memory.PartialUnmaps.PartialUnmapHelpers;

namespace Ryujinx.Common.Memory.PartialUnmaps
{
    /// <summary>
    /// A simple implementation of a ReaderWriterLock which can be used from native code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NativeReaderWriterLock
    {
        public int WriteLock;
        public int ReaderCount;

        public static readonly int WriteLockOffset;
        public static readonly int ReaderCountOffset;

        /// <summary>
        /// Populates the field offsets for use when emitting native code.
        /// </summary>
        static NativeReaderWriterLock()
        {
            NativeReaderWriterLock instance = new();

            WriteLockOffset = OffsetOf(ref instance, ref instance.WriteLock);
            ReaderCountOffset = OffsetOf(ref instance, ref instance.ReaderCount);
        }

        /// <summary>
        /// Acquires the reader lock.
        /// </summary>
        public void AcquireReaderLock()
        {
            // Must take write lock for a very short time to become a reader.

            while (Interlocked.CompareExchange(ref WriteLock, 1, 0) != 0)
            {
            }

            Interlocked.Increment(ref ReaderCount);

            Interlocked.Exchange(ref WriteLock, 0);
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
            Interlocked.Decrement(ref ReaderCount);
        }

        /// <summary>
        /// Upgrades to a writer lock. The reader lock is temporarily released while obtaining the writer lock.
        /// </summary>
        public void UpgradeToWriterLock()
        {
            // Prevent any more threads from entering reader.
            // If the write lock is already taken, wait for it to not be taken.

            Interlocked.Decrement(ref ReaderCount);

            while (Interlocked.CompareExchange(ref WriteLock, 1, 0) != 0)
            {
            }

            // Wait for reader count to drop to 0, then take the lock again as the only reader.

            while (Interlocked.CompareExchange(ref ReaderCount, 1, 0) != 0)
            {
            }
        }

        /// <summary>
        /// Downgrades from a writer lock, back to a reader one.
        /// </summary>
        public void DowngradeFromWriterLock()
        {
            // Release the WriteLock.

            Interlocked.Exchange(ref WriteLock, 0);
        }
    }
}
