using ChocolArm64.Memory;
using System;

namespace Ryujinx.Graphics.Memory
{
    class NvGpuVmmCache
    {
        private struct CachedResource
        {
            public long Key;
            public int  Mask;

            public CachedResource(long Key, int Mask)
            {
                this.Key  = Key;
                this.Mask = Mask;
            }

            public override int GetHashCode()
            {
                return (int)(Key * 23 + Mask);
            }

            public override bool Equals(object obj)
            {
                return obj is CachedResource Cached && Equals(Cached);
            }

            public bool Equals(CachedResource other)
            {
                return Key == other.Key && Mask == other.Mask;
            }
        }

        private ValueRangeSet<CachedResource> CachedRanges;

        public NvGpuVmmCache()
        {
            CachedRanges = new ValueRangeSet<CachedResource>();
        }

        public bool IsRegionModified(MemoryManager Memory, NvGpuBufferType BufferType, long Start, long Size)
        {
            (bool[] Modified, long ModifiedCount) = Memory.IsRegionModified(Start, Size);

            //Remove all modified ranges.
            int Index = 0;

            long Position = Start & ~NvGpuVmm.PageMask;

            while (ModifiedCount > 0)
            {
                if (Modified[Index++])
                {
                    CachedRanges.Remove(new ValueRange<CachedResource>(Position, Position + NvGpuVmm.PageSize));

                    ModifiedCount--;
                }

                Position += NvGpuVmm.PageSize;
            }

            //Mask has the bit set for the current resource type.
            //If the region is not yet present on the list, then a new ValueRange
            //is directly added with the current resource type as the only bit set.
            //Otherwise, it just sets the bit for this new resource type on the current mask.
            //The physical address of the resource is used as key, those keys are used to keep
            //track of resources that are already on the cache. A resource may be inside another
            //resource, and in this case we should return true if the "sub-resource" was not
            //yet cached.
            int Mask = 1 << (int)BufferType;

            CachedResource NewCachedValue = new CachedResource(Start, Mask);

            ValueRange<CachedResource> NewCached = new ValueRange<CachedResource>(Start, Start + Size);

            ValueRange<CachedResource>[] Ranges = CachedRanges.GetAllIntersections(NewCached);

            bool IsKeyCached = Ranges.Length > 0 && Ranges[0].Value.Key == Start;

            long LastEnd = NewCached.Start;

            long Coverage = 0;

            for (Index = 0; Index < Ranges.Length; Index++)
            {
                ValueRange<CachedResource> Current = Ranges[Index];

                CachedResource Cached = Current.Value;

                long RgStart = Math.Max(Current.Start, NewCached.Start);
                long RgEnd   = Math.Min(Current.End,   NewCached.End);

                if ((Cached.Mask & Mask) != 0)
                {
                    Coverage += RgEnd - RgStart;
                }

                //Highest key value has priority, this prevents larger resources
                //for completely invalidating smaller ones on the cache. For example,
                //consider that a resource in the range [100, 200) was added, and then
                //another one in the range [50, 200). We prevent the new resource from
                //completely replacing the old one by spliting it like this:
                //New resource key is added at [50, 100), old key is still present at [100, 200).
                if (Cached.Key < Start)
                {
                    Cached.Key = Start;
                }

                Cached.Mask |= Mask;

                CachedRanges.Add(new ValueRange<CachedResource>(RgStart, RgEnd, Cached));

                if (RgStart > LastEnd)
                {
                    CachedRanges.Add(new ValueRange<CachedResource>(LastEnd, RgStart, NewCachedValue));
                }

                LastEnd = RgEnd;
            }

            if (LastEnd < NewCached.End)
            {
                CachedRanges.Add(new ValueRange<CachedResource>(LastEnd, NewCached.End, NewCachedValue));
            }

            return !IsKeyCached || Coverage != Size;
        }
    }
}