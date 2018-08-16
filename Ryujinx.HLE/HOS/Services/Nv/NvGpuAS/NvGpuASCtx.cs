using Ryujinx.HLE.Gpu.Memory;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nv.NvGpuAS
{
    class NvGpuASCtx
    {
        public NvGpuVmm Vmm { get; private set; }

        private class Range
        {
            public ulong Start  { get; private set; }
            public ulong End    { get; private set; }

            public Range(long Position, long Size)
            {
                Start = (ulong)Position;
                End   = (ulong)Size + Start;
            }
        }

        private class MappedMemory : Range
        {
            public long PhysicalAddress { get; private set; }
            public bool VaAllocated  { get; private set; }

            public MappedMemory(
                long Position,
                long Size,
                long PhysicalAddress,
                bool VaAllocated) : base(Position, Size)
            {
                this.PhysicalAddress = PhysicalAddress;
                this.VaAllocated     = VaAllocated;
            }
        }

        private SortedList<long, Range> Maps;
        private SortedList<long, Range> Reservations;

        public NvGpuASCtx(ServiceCtx Context)
        {
            Vmm = new NvGpuVmm(Context.Memory);

            Maps         = new SortedList<long, Range>();
            Reservations = new SortedList<long, Range>();
        }

        public bool ValidateFixedBuffer(long Position, long Size)
        {
            long MapEnd = Position + Size;

            //Check if size is valid (0 is also not allowed).
            if ((ulong)MapEnd <= (ulong)Position)
            {
                return false;
            }

            //Check if address is page aligned.
            if ((Position & NvGpuVmm.PageMask) != 0)
            {
                return false;
            }

            //Check if region is reserved.
            if (BinarySearch(Reservations, Position) == null)
            {
                return false;
            }

            //Check for overlap with already mapped buffers.
            Range Map = BinarySearchLt(Maps, MapEnd);

            if (Map != null && Map.End > (ulong)Position)
            {
                return false;
            }

            return true;
        }

        public void AddMap(
            long Position,
            long Size,
            long PhysicalAddress,
            bool VaAllocated)
        {
            Maps.Add(Position, new MappedMemory(Position, Size, PhysicalAddress, VaAllocated));
        }

        public bool RemoveMap(long Position, out long Size)
        {
            Size = 0;

            if (Maps.Remove(Position, out Range Value))
            {
                MappedMemory Map = (MappedMemory)Value;

                if (Map.VaAllocated)
                {
                    Size = (long)(Map.End - Map.Start);
                }

                return true;
            }

            return false;
        }

        public bool TryGetMapPhysicalAddress(long Position, out long PhysicalAddress)
        {
            Range Map = BinarySearch(Maps, Position);

            if (Map != null)
            {
                PhysicalAddress = ((MappedMemory)Map).PhysicalAddress;

                return true;
            }

            PhysicalAddress = 0;

            return false;
        }

        public void AddReservation(long Position, long Size)
        {
            Reservations.Add(Position, new Range(Position, Size));
        }

        public bool RemoveReservation(long Position)
        {
            return Reservations.Remove(Position);
        }

        private Range BinarySearch(SortedList<long, Range> Lst, long Position)
        {
            int Left  = 0;
            int Right = Lst.Count - 1;

            while (Left <= Right)
            {
                int Size = Right - Left;

                int Middle = Left + (Size >> 1);

                Range Rg = Lst.Values[Middle];

                if ((ulong)Position >= Rg.Start && (ulong)Position < Rg.End)
                {
                    return Rg;
                }

                if ((ulong)Position < Rg.Start)
                {
                    Right = Middle - 1;
                }
                else
                {
                    Left = Middle + 1;
                }
            }

            return null;
        }

        private Range BinarySearchLt(SortedList<long, Range> Lst, long Position)
        {
            Range LtRg = null;

            int Left  = 0;
            int Right = Lst.Count - 1;

            while (Left <= Right)
            {
                int Size = Right - Left;

                int Middle = Left + (Size >> 1);

                Range Rg = Lst.Values[Middle];

                if ((ulong)Position < Rg.Start)
                {
                    Right = Middle - 1;
                }
                else
                {
                    Left = Middle + 1;

                    LtRg = Rg;
                }
            }

            return LtRg;
        }
    }
}