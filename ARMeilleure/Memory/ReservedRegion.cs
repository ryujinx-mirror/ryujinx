using System;
using System.Collections.Generic;
using System.Text;

namespace ARMeilleure.Memory
{
    class ReservedRegion
    {
        private const int DefaultGranularity = 65536; // Mapping granularity in Windows.

        public IntPtr Pointer { get; }

        private ulong _maxSize;
        private ulong _sizeGranularity;
        private ulong _currentSize;

        public ReservedRegion(ulong maxSize, ulong granularity = 0)
        {
            if (granularity == 0)
            {
                granularity = DefaultGranularity;
            }

            Pointer = MemoryManagement.Reserve(maxSize);
            _maxSize = maxSize;
            _sizeGranularity = granularity;
            _currentSize = 0;
        }

        public void ExpandIfNeeded(ulong desiredSize)
        {
            if (desiredSize > _maxSize)
            {
                throw new OutOfMemoryException();
            }

            if (desiredSize > _currentSize)
            {
                // Lock, and then check again. We only want to commit once.
                lock (this)
                {
                    if (desiredSize >= _currentSize)
                    {
                        ulong overflowBytes = desiredSize - _currentSize;
                        ulong moreToCommit = (((_sizeGranularity - 1) + overflowBytes) / _sizeGranularity) * _sizeGranularity; // Round up.
                        MemoryManagement.Commit(new IntPtr((long)Pointer + (long)_currentSize), moreToCommit);
                        _currentSize += moreToCommit;
                    }
                }
            }
        }
    }
}
