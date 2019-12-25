using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu.Types
{
    class AddressSpaceContext
    {
        private class Range
        {
            public ulong Start { get; private set; }
            public ulong End   { get; private set; }

            public Range(long position, long size)
            {
                Start = (ulong)position;
                End   = (ulong)size + Start;
            }
        }

        private class MappedMemory : Range
        {
            public long PhysicalAddress { get; private set; }
            public bool VaAllocated     { get; private set; }

            public MappedMemory(
                long position,
                long size,
                long physicalAddress,
                bool vaAllocated) : base(position, size)
            {
                PhysicalAddress = physicalAddress;
                VaAllocated     = vaAllocated;
            }
        }

        private SortedList<long, Range> _maps;
        private SortedList<long, Range> _reservations;

        public MemoryManager Gmm { get; }

        public AddressSpaceContext(ServiceCtx context)
        {
            Gmm = context.Device.Gpu.MemoryManager;

            _maps         = new SortedList<long, Range>();
            _reservations = new SortedList<long, Range>();
        }

        public bool ValidateFixedBuffer(long position, long size)
        {
            long mapEnd = position + size;

            // Check if size is valid (0 is also not allowed).
            if ((ulong)mapEnd <= (ulong)position)
            {
                return false;
            }

            // Check if address is page aligned.
            if ((position & (long)MemoryManager.PageMask) != 0)
            {
                return false;
            }

            // Check if region is reserved.
            if (BinarySearch(_reservations, position) == null)
            {
                return false;
            }

            // Check for overlap with already mapped buffers.
            Range map = BinarySearchLt(_maps, mapEnd);

            if (map != null && map.End > (ulong)position)
            {
                return false;
            }

            return true;
        }

        public void AddMap(
            long position,
            long size,
            long physicalAddress,
            bool vaAllocated)
        {
            _maps.Add(position, new MappedMemory(position, size, physicalAddress, vaAllocated));
        }

        public bool RemoveMap(long position, out long size)
        {
            size = 0;

            if (_maps.Remove(position, out Range value))
            {
                MappedMemory map = (MappedMemory)value;

                if (map.VaAllocated)
                {
                    size = (long)(map.End - map.Start);
                }

                return true;
            }

            return false;
        }

        public bool TryGetMapPhysicalAddress(long position, out long physicalAddress)
        {
            Range map = BinarySearch(_maps, position);

            if (map != null)
            {
                physicalAddress = ((MappedMemory)map).PhysicalAddress;

                return true;
            }

            physicalAddress = 0;

            return false;
        }

        public void AddReservation(long position, long size)
        {
            _reservations.Add(position, new Range(position, size));
        }

        public bool RemoveReservation(long position)
        {
            return _reservations.Remove(position);
        }

        private Range BinarySearch(SortedList<long, Range> lst, long position)
        {
            int left  = 0;
            int right = lst.Count - 1;

            while (left <= right)
            {
                int size = right - left;

                int middle = left + (size >> 1);

                Range rg = lst.Values[middle];

                if ((ulong)position >= rg.Start && (ulong)position < rg.End)
                {
                    return rg;
                }

                if ((ulong)position < rg.Start)
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;
                }
            }

            return null;
        }

        private Range BinarySearchLt(SortedList<long, Range> lst, long position)
        {
            Range ltRg = null;

            int left  = 0;
            int right = lst.Count - 1;

            while (left <= right)
            {
                int size = right - left;

                int middle = left + (size >> 1);

                Range rg = lst.Values[middle];

                if ((ulong)position < rg.Start)
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;

                    if ((ulong)position > rg.Start)
                    {
                        ltRg = rg;
                    }
                }
            }

            return ltRg;
        }
    }
}
