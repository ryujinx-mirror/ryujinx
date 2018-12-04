using Ryujinx.Common;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KResourceLimit
    {
        private const int Time10SecondsMs = 10000;

        private long[] _current;
        private long[] _limit;
        private long[] _available;

        private object _lockObj;

        private LinkedList<KThread> _waitingThreads;

        private int _waitingThreadsCount;

        private Horizon _system;

        public KResourceLimit(Horizon system)
        {
            _current   = new long[(int)LimitableResource.Count];
            _limit     = new long[(int)LimitableResource.Count];
            _available = new long[(int)LimitableResource.Count];

            _lockObj = new object();

            _waitingThreads = new LinkedList<KThread>();

            _system = system;
        }

        public bool Reserve(LimitableResource resource, ulong amount)
        {
            return Reserve(resource, (long)amount);
        }

        public bool Reserve(LimitableResource resource, long amount)
        {
            return Reserve(resource, amount, KTimeManager.ConvertMillisecondsToNanoseconds(Time10SecondsMs));
        }

        public bool Reserve(LimitableResource resource, long amount, long timeout)
        {
            long endTimePoint = KTimeManager.ConvertNanosecondsToMilliseconds(timeout);

            endTimePoint += PerformanceCounter.ElapsedMilliseconds;

            bool success = false;

            int index = GetIndex(resource);

            lock (_lockObj)
            {
                long newCurrent = _current[index] + amount;

                while (newCurrent > _limit[index] && _available[index] + amount <= _limit[index])
                {
                    _waitingThreadsCount++;

                    KConditionVariable.Wait(_system, _waitingThreads, _lockObj, timeout);

                    _waitingThreadsCount--;

                    newCurrent = _current[index] + amount;

                    if (timeout >= 0 && PerformanceCounter.ElapsedMilliseconds > endTimePoint)
                    {
                        break;
                    }
                }

                if (newCurrent <= _limit[index])
                {
                    _current[index] = newCurrent;

                    success = true;
                }
            }

            return success;
        }

        public void Release(LimitableResource resource, ulong amount)
        {
            Release(resource, (long)amount);
        }

        public void Release(LimitableResource resource, long amount)
        {
            Release(resource, amount, amount);
        }

        private void Release(LimitableResource resource, long usedAmount, long availableAmount)
        {
            int index = GetIndex(resource);

            lock (_lockObj)
            {
                _current  [index] -= usedAmount;
                _available[index] -= availableAmount;

                if (_waitingThreadsCount > 0)
                {
                    KConditionVariable.NotifyAll(_system, _waitingThreads);
                }
            }
        }

        public long GetRemainingValue(LimitableResource resource)
        {
            int index = GetIndex(resource);

            lock (_lockObj)
            {
                return _limit[index] - _current[index];
            }
        }

        public KernelResult SetLimitValue(LimitableResource resource, long limit)
        {
            int index = GetIndex(resource);

            lock (_lockObj)
            {
                if (_current[index] <= limit)
                {
                    _limit[index] = limit;

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidState;
                }
            }
        }

        private static int GetIndex(LimitableResource resource)
        {
            return (int)resource;
        }
    }
}