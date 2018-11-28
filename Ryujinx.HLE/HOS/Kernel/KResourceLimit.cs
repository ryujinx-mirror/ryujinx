using Ryujinx.Common;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KResourceLimit
    {
        private const int Time10SecondsMs = 10000;

        private long[] Current;
        private long[] Limit;
        private long[] Available;

        private object LockObj;

        private LinkedList<KThread> WaitingThreads;

        private int WaitingThreadsCount;

        private Horizon System;

        public KResourceLimit(Horizon System)
        {
            Current   = new long[(int)LimitableResource.Count];
            Limit     = new long[(int)LimitableResource.Count];
            Available = new long[(int)LimitableResource.Count];

            LockObj = new object();

            WaitingThreads = new LinkedList<KThread>();

            this.System = System;
        }

        public bool Reserve(LimitableResource Resource, ulong Amount)
        {
            return Reserve(Resource, (long)Amount);
        }

        public bool Reserve(LimitableResource Resource, long Amount)
        {
            return Reserve(Resource, Amount, KTimeManager.ConvertMillisecondsToNanoseconds(Time10SecondsMs));
        }

        public bool Reserve(LimitableResource Resource, long Amount, long Timeout)
        {
            long EndTimePoint = KTimeManager.ConvertNanosecondsToMilliseconds(Timeout);

            EndTimePoint += PerformanceCounter.ElapsedMilliseconds;

            bool Success = false;

            int Index = GetIndex(Resource);

            lock (LockObj)
            {
                long NewCurrent = Current[Index] + Amount;

                while (NewCurrent > Limit[Index] && Available[Index] + Amount <= Limit[Index])
                {
                    WaitingThreadsCount++;

                    KConditionVariable.Wait(System, WaitingThreads, LockObj, Timeout);

                    WaitingThreadsCount--;

                    NewCurrent = Current[Index] + Amount;

                    if (Timeout >= 0 && PerformanceCounter.ElapsedMilliseconds > EndTimePoint)
                    {
                        break;
                    }
                }

                if (NewCurrent <= Limit[Index])
                {
                    Current[Index] = NewCurrent;

                    Success = true;
                }
            }

            return Success;
        }

        public void Release(LimitableResource Resource, ulong Amount)
        {
            Release(Resource, (long)Amount);
        }

        public void Release(LimitableResource Resource, long Amount)
        {
            Release(Resource, Amount, Amount);
        }

        private void Release(LimitableResource Resource, long UsedAmount, long AvailableAmount)
        {
            int Index = GetIndex(Resource);

            lock (LockObj)
            {
                Current  [Index] -= UsedAmount;
                Available[Index] -= AvailableAmount;

                if (WaitingThreadsCount > 0)
                {
                    KConditionVariable.NotifyAll(System, WaitingThreads);
                }
            }
        }

        public long GetRemainingValue(LimitableResource Resource)
        {
            int Index = GetIndex(Resource);

            lock (LockObj)
            {
                return Limit[Index] - Current[Index];
            }
        }

        public KernelResult SetLimitValue(LimitableResource Resource, long Limit)
        {
            int Index = GetIndex(Resource);

            lock (LockObj)
            {
                if (Current[Index] <= Limit)
                {
                    this.Limit[Index] = Limit;

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidState;
                }
            }
        }

        private static int GetIndex(LimitableResource Resource)
        {
            return (int)Resource;
        }
    }
}