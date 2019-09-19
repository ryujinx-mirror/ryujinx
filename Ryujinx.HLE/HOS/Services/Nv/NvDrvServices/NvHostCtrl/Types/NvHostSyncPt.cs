using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl
{
    class NvHostSyncpt
    {
        public const int SyncptsCount = 192;

        private int[] _counterMin;
        private int[] _counterMax;

        private long _eventMask;

        private ConcurrentDictionary<EventWaitHandle, int> _waiters;

        public NvHostSyncpt()
        {
            _counterMin = new int[SyncptsCount];
            _counterMax = new int[SyncptsCount];

            _waiters = new ConcurrentDictionary<EventWaitHandle, int>();
        }

        public int GetMin(int id)
        {
            return _counterMin[id];
        }

        public int GetMax(int id)
        {
            return _counterMax[id];
        }

        public int Increment(int id)
        {
            if (((_eventMask >> id) & 1) != 0)
            {
                Interlocked.Increment(ref _counterMax[id]);
            }

            return IncrementMin(id);
        }

        public int IncrementMin(int id)
        {
            int value = Interlocked.Increment(ref _counterMin[id]);

            WakeUpWaiters(id, value);

            return value;
        }

        public int IncrementMax(int id)
        {
            return Interlocked.Increment(ref _counterMax[id]);
        }

        public void AddWaiter(int threshold, EventWaitHandle waitEvent)
        {
            if (!_waiters.TryAdd(waitEvent, threshold))
            {
                throw new InvalidOperationException();
            }
        }

        public bool RemoveWaiter(EventWaitHandle waitEvent)
        {
            return _waiters.TryRemove(waitEvent, out _);
        }

        private void WakeUpWaiters(int id, int newValue)
        {
            foreach (KeyValuePair<EventWaitHandle, int> kv in _waiters)
            {
                if (MinCompare(id, newValue, _counterMax[id], kv.Value))
                {
                    kv.Key.Set();

                    _waiters.TryRemove(kv.Key, out _);
                }
            }
        }

        public bool MinCompare(int id, int threshold)
        {
            return MinCompare(id, _counterMin[id], _counterMax[id], threshold);
        }

        private bool MinCompare(int id, int min, int max, int threshold)
        {
            int minDiff = min - threshold;
            int maxDiff = max - threshold;

            if (((_eventMask >> id) & 1) != 0)
            {
                return minDiff >= 0;
            }
            else
            {
                return (uint)maxDiff >= (uint)minDiff;
            }
        }
    }
}