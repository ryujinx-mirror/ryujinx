using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Core.OsHle.Services.Nv.NvHostCtrl
{
    class NvHostSyncpt
    {
        public const int SyncptsCount = 192;

        private int[] CounterMin;
        private int[] CounterMax;

        private long EventMask;

        private ConcurrentDictionary<EventWaitHandle, int> Waiters;

        public NvHostSyncpt()
        {
            CounterMin = new int[SyncptsCount];
            CounterMax = new int[SyncptsCount];

            Waiters = new ConcurrentDictionary<EventWaitHandle, int>();
        }

        public int GetMin(int Id)
        {
            return CounterMin[Id];
        }

        public int GetMax(int Id)
        {
            return CounterMax[Id];
        }

        public int Increment(int Id)
        {
            if (((EventMask >> Id) & 1) != 0)
            {
                Interlocked.Increment(ref CounterMax[Id]);
            }

            return IncrementMin(Id);
        }

        public int IncrementMin(int Id)
        {
            int Value = Interlocked.Increment(ref CounterMin[Id]);

            WakeUpWaiters(Id, Value);

            return Value;
        }

        public int IncrementMax(int Id)
        {
            return Interlocked.Increment(ref CounterMax[Id]);
        }

        public void AddWaiter(int Threshold, EventWaitHandle WaitEvent)
        {
            if (!Waiters.TryAdd(WaitEvent, Threshold))
            {
                throw new InvalidOperationException();
            }
        }

        public bool RemoveWaiter(EventWaitHandle WaitEvent)
        {
            return Waiters.TryRemove(WaitEvent, out _);
        }

        private void WakeUpWaiters(int Id, int NewValue)
        {
            foreach (KeyValuePair<EventWaitHandle, int> KV in Waiters)
            {
                if (MinCompare(Id, NewValue, CounterMax[Id], KV.Value))
                {
                    KV.Key.Set();

                    Waiters.TryRemove(KV.Key, out _);
                }
            }
        }

        public bool MinCompare(int Id, int Threshold)
        {
            return MinCompare(Id, CounterMin[Id], CounterMax[Id], Threshold);
        }

        private bool MinCompare(int Id, int Min, int Max, int Threshold)
        {
            int MinDiff = Min - Threshold;
            int MaxDiff = Max - Threshold;

            if (((EventMask >> Id) & 1) != 0)
            {
                return MinDiff >= 0;
            }
            else
            {
                return (uint)MaxDiff >= (uint)MinDiff;
            }
        }
    }
}