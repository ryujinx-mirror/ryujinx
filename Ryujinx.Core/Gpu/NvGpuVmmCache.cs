using ChocolArm64.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.Gpu
{
    class NvGpuVmmCache
    {
        private const int MaxCpCount     = 10000;
        private const int MaxCpTimeDelta = 60000;

        private class CachedPage
        {
            private List<(long Start, long End)> Regions;

            public LinkedListNode<long> Node { get; set; }

            public int Count => Regions.Count;

            public int Timestamp { get; private set; }

            public long PABase { get; private set; }

            public NvGpuBufferType BufferType { get; private set; }

            public CachedPage(long PABase, NvGpuBufferType BufferType)
            {
                this.PABase     = PABase;
                this.BufferType = BufferType;

                Regions = new List<(long, long)>();
            }

            public bool AddRange(long Start, long End)
            {
                for (int Index = 0; Index < Regions.Count; Index++)
                {
                    (long RgStart, long RgEnd) = Regions[Index];

                    if (Start >= RgStart && End <= RgEnd)
                    {
                        return false;
                    }

                    if (Start <= RgEnd && RgStart <= End)
                    {
                        long MinStart = Math.Min(RgStart, Start);
                        long MaxEnd   = Math.Max(RgEnd,   End);

                        Regions[Index] = (MinStart, MaxEnd);

                        Timestamp = Environment.TickCount;

                        return true;
                    }
                }

                Regions.Add((Start, End));

                Timestamp = Environment.TickCount;

                return true;
            }
        }

        private Dictionary<long, CachedPage> Cache;

        private LinkedList<long> SortedCache;

        private int CpCount;

        public NvGpuVmmCache()
        {
            Cache = new Dictionary<long, CachedPage>();

            SortedCache = new LinkedList<long>();
        }

        public bool IsRegionModified(
            AMemory         Memory,
            NvGpuBufferType BufferType,
            long            VA,
            long            PA,
            long            Size)
        {
            ClearCachedPagesIfNeeded();

            long PageSize = Memory.GetHostPageSize();

            long Mask = PageSize - 1;

            long VAEnd = VA + Size;
            long PAEnd = PA + Size;

            bool RegMod = false;

            while (VA < VAEnd)
            {
                long Key    = VA & ~Mask;
                long PABase = PA & ~Mask;

                long VAPgEnd = Math.Min((VA + PageSize) & ~Mask, VAEnd);
                long PAPgEnd = Math.Min((PA + PageSize) & ~Mask, PAEnd);

                bool IsCached = Cache.TryGetValue(Key, out CachedPage Cp);

                bool PgReset = false;

                if (!IsCached)
                {
                    Cp = new CachedPage(PABase, BufferType);

                    Cache.Add(Key, Cp);
                }
                else
                {
                    CpCount -= Cp.Count;

                    SortedCache.Remove(Cp.Node);

                    if (Cp.PABase     != PABase ||
                        Cp.BufferType != BufferType)
                    {
                        PgReset = true;
                    }
                }

                PgReset |= Memory.IsRegionModified(PA, PAPgEnd - PA) && IsCached;

                if (PgReset)
                {
                    Cp = new CachedPage(PABase, BufferType);

                    Cache[Key] = Cp;
                }

                Cp.Node = SortedCache.AddLast(Key);

                RegMod |= Cp.AddRange(VA, VAPgEnd);

                CpCount += Cp.Count;

                VA = VAPgEnd;
                PA = PAPgEnd;
            }

            return RegMod;
        }

        private void ClearCachedPagesIfNeeded()
        {
            if (CpCount <= MaxCpCount)
            {
                return;
            }

            int Timestamp = Environment.TickCount;

            int TimeDelta;

            do
            {
                if (!TryPopOldestCachedPageKey(Timestamp, out long Key))
                {
                    break;
                }

                CachedPage Cp = Cache[Key];

                Cache.Remove(Key);

                CpCount -= Cp.Count;

                TimeDelta = RingDelta(Cp.Timestamp, Timestamp);
            }
            while (CpCount > (MaxCpCount >> 1) || (uint)TimeDelta > (uint)MaxCpTimeDelta);
        }

        private bool TryPopOldestCachedPageKey(int Timestamp, out long Key)
        {
            LinkedListNode<long> Node = SortedCache.First;

            if (Node == null)
            {
                Key = 0;

                return false;
            }

            SortedCache.Remove(Node);

            Key = Node.Value;

            return true;
        }

        private int RingDelta(int Old, int New)
        {
            if ((uint)New < (uint)Old)
            {
                return New + (~Old + 1);
            }
            else
            {
                return New - Old;
            }
        }
    }
}