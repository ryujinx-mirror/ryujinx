using ChocolArm64.Memory;
using System;

namespace Ryujinx.Graphics.Memory
{
    class NvGpuVmmCache
    {
        private ValueRangeSet<int> CachedRanges;

        public NvGpuVmmCache()
        {
            CachedRanges = new ValueRangeSet<int>();
        }

        public bool IsRegionModified(AMemory Memory, NvGpuBufferType BufferType, long PA, long Size)
        {
            (bool[] Modified, long ModifiedCount) = Memory.IsRegionModified(PA, Size);

            //Remove all modified ranges.
            int Index = 0;

            long Position = PA & ~NvGpuVmm.PageMask;

            while (ModifiedCount > 0)
            {
                if (Modified[Index++])
                {
                    CachedRanges.Remove(new ValueRange<int>(Position, Position + NvGpuVmm.PageSize));

                    ModifiedCount--;
                }

                Position += NvGpuVmm.PageSize;
            }

            //Mask has the bit set for the current resource type.
            //If the region is not yet present on the list, then a new ValueRange
            //is directly added with the current resource type as the only bit set.
            //Otherwise, it just sets the bit for this new resource type on the current mask.
            int Mask = 1 << (int)BufferType;

            ValueRange<int> NewCached = new ValueRange<int>(PA, PA + Size);

            ValueRange<int>[] Ranges = CachedRanges.GetAllIntersections(NewCached);

            long LastEnd = NewCached.Start;

            long Coverage = 0;

            for (Index = 0; Index < Ranges.Length; Index++)
            {
                ValueRange<int> Current = Ranges[Index];

                long RgStart = Math.Max(Current.Start, NewCached.Start);
                long RgEnd   = Math.Min(Current.End,   NewCached.End);

                if ((Current.Value & Mask) == 0)
                {
                    CachedRanges.Add(new ValueRange<int>(RgStart, RgEnd, Current.Value | Mask));
                }
                else
                {
                    Coverage += RgEnd - RgStart;
                }

                if (RgStart > LastEnd)
                {
                    CachedRanges.Add(new ValueRange<int>(LastEnd, RgStart, Mask));
                }

                LastEnd = RgEnd;
            }

            if (LastEnd < NewCached.End)
            {
                CachedRanges.Add(new ValueRange<int>(LastEnd, NewCached.End, Mask));
            }

            return Coverage != Size;
        }
    }
}