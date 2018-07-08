using ChocolArm64.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.Gpu.Memory
{
    class NvGpuVmmCache
    {
        private const int MaxCpCount     = 10000;
        private const int MaxCpTimeDelta = 60000;

        private class CachedPage
        {
            private struct Range
            {
                public long Start;
                public long End;

                public Range(long Start, long End)
                {
                    this.Start = Start;
                    this.End   = End;
                }
            }

            private List<Range>[] Regions;

            public LinkedListNode<long> Node { get; set; }

            public int Timestamp { get; private set; }

            public CachedPage()
            {
                Regions = new List<Range>[(int)NvGpuBufferType.Count];

                for (int Index = 0; Index < Regions.Length; Index++)
                {
                    Regions[Index] = new List<Range>();
                }
            }

            public bool AddRange(long Start, long End, NvGpuBufferType BufferType)
            {
                List<Range> BtRegions = Regions[(int)BufferType];

                for (int Index = 0; Index < BtRegions.Count; Index++)
                {
                    Range Rg = BtRegions[Index];

                    if (Start >= Rg.Start && End <= Rg.End)
                    {
                        return false;
                    }

                    if (Start <= Rg.End && Rg.Start <= End)
                    {
                        long MinStart = Math.Min(Rg.Start, Start);
                        long MaxEnd   = Math.Max(Rg.End,   End);

                        BtRegions[Index] = new Range(MinStart, MaxEnd);

                        Timestamp = Environment.TickCount;

                        return true;
                    }
                }

                BtRegions.Add(new Range(Start, End));

                Timestamp = Environment.TickCount;

                return true;
            }

            public int GetTotalCount()
            {
                int Count = 0;

                for (int Index = 0; Index < Regions.Length; Index++)
                {
                    Count += Regions[Index].Count;
                }

                return Count;
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

        public bool IsRegionModified(AMemory Memory, NvGpuBufferType BufferType, long PA, long Size)
        {
            bool[] Modified = Memory.IsRegionModified(PA, Size);

            if (Modified == null)
            {
                return true;
            }

            ClearCachedPagesIfNeeded();

            long PageSize = Memory.GetHostPageSize();

            long Mask = PageSize - 1;

            long PAEnd = PA + Size;

            bool RegMod = false;

            int Index = 0;

            while (PA < PAEnd)
            {
                long Key = PA & ~Mask;

                long PAPgEnd = Math.Min((PA + PageSize) & ~Mask, PAEnd);

                bool IsCached = Cache.TryGetValue(Key, out CachedPage Cp);

                if (IsCached)
                {
                    CpCount -= Cp.GetTotalCount();

                    SortedCache.Remove(Cp.Node);
                }
                else
                {
                    Cp = new CachedPage();

                    Cache.Add(Key, Cp);
                }

                if (Modified[Index++] && IsCached)
                {
                    Cp = new CachedPage();

                    Cache[Key] = Cp;
                }

                Cp.Node = SortedCache.AddLast(Key);

                RegMod |= Cp.AddRange(PA, PAPgEnd, BufferType);

                CpCount += Cp.GetTotalCount();

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

                CpCount -= Cp.GetTotalCount();

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