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

            public Range(ulong position, ulong size)
            {
                Start = position;
                End   = size + Start;
            }
        }

        private class MappedMemory : Range
        {
            public ulong PhysicalAddress { get; private set; }
            public bool  VaAllocated     { get; private set; }

            public MappedMemory(
                ulong position,
                ulong size,
                ulong physicalAddress,
                bool vaAllocated) : base(position, size)
            {
                PhysicalAddress = physicalAddress;
                VaAllocated     = vaAllocated;
            }
        }

        private SortedList<ulong, Range> _maps;
        private SortedList<ulong, Range> _reservations;

        public MemoryManager Gmm { get; }

        public AddressSpaceContext(ServiceCtx context)
        {
            Gmm = context.Device.Gpu.MemoryManager;

            _maps         = new SortedList<ulong, Range>();
            _reservations = new SortedList<ulong, Range>();
        }

        public bool ValidateFixedBuffer(ulong position, ulong size, ulong alignment)
        {
            ulong mapEnd = position + size;

            // Check if size is valid (0 is also not allowed).
            if (mapEnd <= position)
            {
                return false;
            }

            // Check if address is aligned.
            if ((position & (alignment - 1)) != 0)
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

            if (map != null && map.End > position)
            {
                return false;
            }

            return true;
        }

        public void AddMap(
            ulong position,
            ulong size,
            ulong physicalAddress,
            bool vaAllocated)
        {
            _maps.Add(position, new MappedMemory(position, size, physicalAddress, vaAllocated));
        }

        public bool RemoveMap(ulong position, out ulong size)
        {
            size = 0;

            if (_maps.Remove(position, out Range value))
            {
                MappedMemory map = (MappedMemory)value;

                if (map.VaAllocated)
                {
                    size = (map.End - map.Start);
                }

                return true;
            }

            return false;
        }

        public bool TryGetMapPhysicalAddress(ulong position, out ulong physicalAddress)
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

        public void AddReservation(ulong position, ulong size)
        {
            _reservations.Add(position, new Range(position, size));
        }

        public bool RemoveReservation(ulong position)
        {
            return _reservations.Remove(position);
        }

        private Range BinarySearch(SortedList<ulong, Range> lst, ulong position)
        {
            int left  = 0;
            int right = lst.Count - 1;

            while (left <= right)
            {
                int size = right - left;

                int middle = left + (size >> 1);

                Range rg = lst.Values[middle];

                if (position >= rg.Start && position < rg.End)
                {
                    return rg;
                }

                if (position < rg.Start)
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

        private Range BinarySearchLt(SortedList<ulong, Range> lst, ulong position)
        {
            Range ltRg = null;

            int left  = 0;
            int right = lst.Count - 1;

            while (left <= right)
            {
                int size = right - left;

                int middle = left + (size >> 1);

                Range rg = lst.Values[middle];

                if (position < rg.Start)
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;

                    if (position > rg.Start)
                    {
                        ltRg = rg;
                    }
                }
            }

            return ltRg;
        }
    }
}
