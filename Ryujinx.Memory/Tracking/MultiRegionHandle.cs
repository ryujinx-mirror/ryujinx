using System;
using System.Collections.Generic;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// A region handle that tracks a large region using many smaller handles, to provide
    /// granular tracking that can be used to track partial updates.
    /// </summary>
    public class MultiRegionHandle : IMultiRegionHandle
    {
        /// <summary>
        /// A list of region handles for each granularity sized chunk of the whole region.
        /// </summary>
        private readonly RegionHandle[] _handles;
        private readonly ulong Address;
        private readonly ulong Granularity;
        private readonly ulong Size;

        public bool Dirty { get; private set; } = true;

        internal MultiRegionHandle(MemoryTracking tracking, ulong address, ulong size, IEnumerable<IRegionHandle> handles, ulong granularity)
        {
            _handles = new RegionHandle[size / granularity];
            Granularity = granularity;

            int i = 0;

            if (handles != null)
            {
                // Inherit from the handles we were given. Any gaps must be filled with new handles,
                // and old handles larger than our granularity must copy their state onto new granular handles and dispose.
                // It is assumed that the provided handles do not overlap, in order, are on page boundaries,
                // and don't extend past the requested range.

                foreach (RegionHandle handle in handles)
                {
                    int startIndex = (int)((handle.Address - address) / granularity);

                    // Fill any gap left before this handle.
                    while (i < startIndex)
                    {
                        RegionHandle fillHandle = tracking.BeginTracking(address + (ulong)i * granularity, granularity);
                        fillHandle.Parent = this;
                        _handles[i++] = fillHandle;
                    }

                    if (handle.Size == granularity)
                    {
                        handle.Parent = this;
                        _handles[i++] = handle;
                    }
                    else
                    {
                        int endIndex = (int)((handle.EndAddress - address) / granularity);

                        while (i < endIndex)
                        {
                            RegionHandle splitHandle = tracking.BeginTracking(address + (ulong)i * granularity, granularity);
                            splitHandle.Parent = this;

                            splitHandle.Reprotect(handle.Dirty);

                            RegionSignal signal = handle.PreAction;
                            if (signal != null)
                            {
                                splitHandle.RegisterAction(signal);
                            }

                            _handles[i++] = splitHandle;
                        }

                        handle.Dispose();
                    }
                }
            }

            // Fill any remaining space with new handles.
            while (i < _handles.Length)
            {
                RegionHandle handle = tracking.BeginTracking(address + (ulong)i * granularity, granularity);
                handle.Parent = this;
                _handles[i++] = handle;
            }

            Address = address;
            Size = size;
        }

        public void ForceDirty(ulong address, ulong size)
        {
            Dirty = true;

            int startHandle = (int)((address - Address) / Granularity);
            int lastHandle = (int)((address + (size - 1) - Address) / Granularity);

            for (int i = startHandle; i <= lastHandle; i++)
            {
                _handles[i].SequenceNumber--;
                _handles[i].ForceDirty();
            }
        }

        public IEnumerable<RegionHandle> GetHandles()
        {
            return _handles;
        }

        public void SignalWrite()
        {
            Dirty = true;
        }

        public void QueryModified(Action<ulong, ulong> modifiedAction)
        {
            if (!Dirty)
            {
                return;
            }

            Dirty = false;

            QueryModified(Address, Size, modifiedAction);
        }

        public void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction)
        {
            int startHandle = (int)((address - Address) / Granularity);
            int lastHandle = (int)((address + (size - 1) - Address) / Granularity);

            ulong rgStart = _handles[startHandle].Address;
            ulong rgSize = 0;

            for (int i = startHandle; i <= lastHandle; i++)
            {
                RegionHandle handle = _handles[i];

                if (handle.Dirty)
                {
                    rgSize += handle.Size;
                    handle.Reprotect();
                }
                else
                {
                    // Submit the region scanned so far as dirty
                    if (rgSize != 0)
                    {
                        modifiedAction(rgStart, rgSize);
                        rgSize = 0;
                    }
                    rgStart = handle.EndAddress;
                }
            }

            if (rgSize != 0)
            {
                modifiedAction(rgStart, rgSize);
            }
        }

        public void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction, int sequenceNumber)
        {
            int startHandle = (int)((address - Address) / Granularity);
            int lastHandle = (int)((address + (size - 1) - Address) / Granularity);

            ulong rgStart = _handles[startHandle].Address;
            ulong rgSize = 0;

            for (int i = startHandle; i <= lastHandle; i++)
            {
                RegionHandle handle = _handles[i];

                if (sequenceNumber != handle.SequenceNumber && handle.DirtyOrVolatile())
                {
                    rgSize += handle.Size;
                    handle.Reprotect();
                }
                else
                {
                    // Submit the region scanned so far as dirty
                    if (rgSize != 0)
                    {
                        modifiedAction(rgStart, rgSize);
                        rgSize = 0;
                    }
                    rgStart = handle.EndAddress;
                }

                handle.SequenceNumber = sequenceNumber;
            }

            if (rgSize != 0)
            {
                modifiedAction(rgStart, rgSize);
            }
        }

        public void RegisterAction(ulong address, ulong size, RegionSignal action)
        {
            int startHandle = (int)((address - Address) / Granularity);
            int lastHandle = (int)((address + (size - 1) - Address) / Granularity);

            for (int i = startHandle; i <= lastHandle; i++)
            {
                _handles[i].RegisterAction(action);
            }
        }

        public void Dispose()
        {
            foreach (var handle in _handles)
            {
                handle.Dispose();
            }
        }
    }
}
