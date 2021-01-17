using System;

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

        internal MultiRegionHandle(MemoryTracking tracking, ulong address, ulong size, ulong granularity)
        {
            _handles = new RegionHandle[size / granularity];
            Granularity = granularity;

            for (int i = 0; i < _handles.Length; i++)
            {
                RegionHandle handle = tracking.BeginTracking(address + (ulong)i * granularity, granularity);
                handle.Parent = this;
                _handles[i] = handle;
            }

            Address = address;
            Size = size;
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

                if (handle.Dirty && sequenceNumber != handle.SequenceNumber)
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
