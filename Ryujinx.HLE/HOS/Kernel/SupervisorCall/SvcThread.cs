using ARMeilleure.Memory;
using ARMeilleure.State;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    partial class SvcHandler
    {
        public KernelResult CreateThread64(
            ulong   entrypoint,
            ulong   argsPtr,
            ulong   stackTop,
            int     priority,
            int     cpuCore,
            out int handle)
        {
            return CreateThread(entrypoint, argsPtr, stackTop, priority, cpuCore, out handle);
        }

        private KernelResult CreateThread(
            ulong   entrypoint,
            ulong   argsPtr,
            ulong   stackTop,
            int     priority,
            int     cpuCore,
            out int handle)
        {
            handle = 0;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (cpuCore == -2)
            {
                cpuCore = currentProcess.DefaultCpuCore;
            }

            if ((uint)cpuCore >= KScheduler.CpuCoresCount || !currentProcess.IsCpuCoreAllowed(cpuCore))
            {
                return KernelResult.InvalidCpuCore;
            }

            if ((uint)priority >= KScheduler.PrioritiesCount || !currentProcess.IsPriorityAllowed(priority))
            {
                return KernelResult.InvalidPriority;
            }

            long timeout = KTimeManager.ConvertMillisecondsToNanoseconds(100);

            if (currentProcess.ResourceLimit != null &&
               !currentProcess.ResourceLimit.Reserve(LimitableResource.Thread, 1, timeout))
            {
                return KernelResult.ResLimitExceeded;
            }

            KThread thread = new KThread(_system);

            KernelResult result = currentProcess.InitializeThread(
                thread,
                entrypoint,
                argsPtr,
                stackTop,
                priority,
                cpuCore);

            if (result == KernelResult.Success)
            {
                result = _process.HandleTable.GenerateHandle(thread, out handle);
            }
            else
            {
                currentProcess.ResourceLimit?.Release(LimitableResource.Thread, 1);
            }

            thread.DecrementReferenceCount();

            return result;
        }

        public KernelResult StartThread64(int handle)
        {
            return StartThread(handle);
        }

        private KernelResult StartThread(int handle)
        {
            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                thread.IncrementReferenceCount();

                KernelResult result = thread.Start();

                if (result == KernelResult.Success)
                {
                    thread.IncrementReferenceCount();
                }

                thread.DecrementReferenceCount();

                return result;
            }
            else
            {
                return KernelResult.InvalidHandle;
            }
        }

        public void ExitThread64()
        {
            ExitThread();
        }

        private void ExitThread()
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.Scheduler.ExitThread(currentThread);

            currentThread.Exit();
        }

        public void SleepThread64(long timeout)
        {
            SleepThread(timeout);
        }

        private void SleepThread(long timeout)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            if (timeout < 1)
            {
                switch (timeout)
                {
                    case  0: currentThread.Yield();                        break;
                    case -1: currentThread.YieldWithLoadBalancing();       break;
                    case -2: currentThread.YieldAndWaitForLoadBalancing(); break;
                }
            }
            else
            {
                currentThread.Sleep(timeout);
            }
        }

        public KernelResult GetThreadPriority64(int handle, out int priority)
        {
            return GetThreadPriority(handle, out priority);
        }

        private KernelResult GetThreadPriority(int handle, out int priority)
        {
            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                priority = thread.DynamicPriority;

                return KernelResult.Success;
            }
            else
            {
                priority = 0;

                return KernelResult.InvalidHandle;
            }
        }

        public KernelResult SetThreadPriority64(int handle, int priority)
        {
            return SetThreadPriority(handle, priority);
        }

        public KernelResult SetThreadPriority(int handle, int priority)
        {
            // TODO: NPDM check.

            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            thread.SetPriority(priority);

            return KernelResult.Success;
        }

        public KernelResult GetThreadCoreMask64(int handle, out int preferredCore, out long affinityMask)
        {
            return GetThreadCoreMask(handle, out preferredCore, out affinityMask);
        }

        private KernelResult GetThreadCoreMask(int handle, out int preferredCore, out long affinityMask)
        {
            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                preferredCore = thread.PreferredCore;
                affinityMask  = thread.AffinityMask;

                return KernelResult.Success;
            }
            else
            {
                preferredCore = 0;
                affinityMask  = 0;

                return KernelResult.InvalidHandle;
            }
        }

        public KernelResult SetThreadCoreMask64(int handle, int preferredCore, long affinityMask)
        {
            return SetThreadCoreMask(handle, preferredCore, affinityMask);
        }

        private KernelResult SetThreadCoreMask(int handle, int preferredCore, long affinityMask)
        {
            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (preferredCore == -2)
            {
                preferredCore = currentProcess.DefaultCpuCore;

                affinityMask = 1 << preferredCore;
            }
            else
            {
                if ((currentProcess.Capabilities.AllowedCpuCoresMask | affinityMask) !=
                     currentProcess.Capabilities.AllowedCpuCoresMask)
                {
                    return KernelResult.InvalidCpuCore;
                }

                if (affinityMask == 0)
                {
                    return KernelResult.InvalidCombination;
                }

                if ((uint)preferredCore > 3)
                {
                    if ((preferredCore | 2) != -1)
                    {
                        return KernelResult.InvalidCpuCore;
                    }
                }
                else if ((affinityMask & (1 << preferredCore)) == 0)
                {
                    return KernelResult.InvalidCombination;
                }
            }

            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            return thread.SetCoreAndAffinityMask(preferredCore, affinityMask);
        }

        public int GetCurrentProcessorNumber64()
        {
            return _system.Scheduler.GetCurrentThread().CurrentCore;
        }

        public KernelResult GetThreadId64(int handle, out long threadUid)
        {
            return GetThreadId(handle, out threadUid);
        }

        private KernelResult GetThreadId(int handle, out long threadUid)
        {
            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                threadUid = thread.ThreadUid;

                return KernelResult.Success;
            }
            else
            {
                threadUid = 0;

                return KernelResult.InvalidHandle;
            }
        }

        public KernelResult SetThreadActivity64(int handle, bool pause)
        {
            return SetThreadActivity(handle, pause);
        }

        private KernelResult SetThreadActivity(int handle, bool pause)
        {
            KThread thread = _process.HandleTable.GetObject<KThread>(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (thread.Owner != _system.Scheduler.GetCurrentProcess())
            {
                return KernelResult.InvalidHandle;
            }

            if (thread == _system.Scheduler.GetCurrentThread())
            {
                return KernelResult.InvalidThread;
            }

            return thread.SetActivity(pause);
        }

        public KernelResult GetThreadContext364(ulong address, int handle)
        {
            return GetThreadContext3(address, handle);
        }

        private KernelResult GetThreadContext3(ulong address, int handle)
        {
            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();
            KThread  currentThread  = _system.Scheduler.GetCurrentThread();

            KThread thread = _process.HandleTable.GetObject<KThread>(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (thread.Owner != currentProcess)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentThread == thread)
            {
                return KernelResult.InvalidThread;
            }

            IMemoryManager memory = currentProcess.CpuMemory;

            memory.WriteUInt64((long)address + 0x0,  thread.Context.GetX(0));
            memory.WriteUInt64((long)address + 0x8,  thread.Context.GetX(1));
            memory.WriteUInt64((long)address + 0x10, thread.Context.GetX(2));
            memory.WriteUInt64((long)address + 0x18, thread.Context.GetX(3));
            memory.WriteUInt64((long)address + 0x20, thread.Context.GetX(4));
            memory.WriteUInt64((long)address + 0x28, thread.Context.GetX(5));
            memory.WriteUInt64((long)address + 0x30, thread.Context.GetX(6));
            memory.WriteUInt64((long)address + 0x38, thread.Context.GetX(7));
            memory.WriteUInt64((long)address + 0x40, thread.Context.GetX(8));
            memory.WriteUInt64((long)address + 0x48, thread.Context.GetX(9));
            memory.WriteUInt64((long)address + 0x50, thread.Context.GetX(10));
            memory.WriteUInt64((long)address + 0x58, thread.Context.GetX(11));
            memory.WriteUInt64((long)address + 0x60, thread.Context.GetX(12));
            memory.WriteUInt64((long)address + 0x68, thread.Context.GetX(13));
            memory.WriteUInt64((long)address + 0x70, thread.Context.GetX(14));
            memory.WriteUInt64((long)address + 0x78, thread.Context.GetX(15));
            memory.WriteUInt64((long)address + 0x80, thread.Context.GetX(16));
            memory.WriteUInt64((long)address + 0x88, thread.Context.GetX(17));
            memory.WriteUInt64((long)address + 0x90, thread.Context.GetX(18));
            memory.WriteUInt64((long)address + 0x98, thread.Context.GetX(19));
            memory.WriteUInt64((long)address + 0xa0, thread.Context.GetX(20));
            memory.WriteUInt64((long)address + 0xa8, thread.Context.GetX(21));
            memory.WriteUInt64((long)address + 0xb0, thread.Context.GetX(22));
            memory.WriteUInt64((long)address + 0xb8, thread.Context.GetX(23));
            memory.WriteUInt64((long)address + 0xc0, thread.Context.GetX(24));
            memory.WriteUInt64((long)address + 0xc8, thread.Context.GetX(25));
            memory.WriteUInt64((long)address + 0xd0, thread.Context.GetX(26));
            memory.WriteUInt64((long)address + 0xd8, thread.Context.GetX(27));
            memory.WriteUInt64((long)address + 0xe0, thread.Context.GetX(28));
            memory.WriteUInt64((long)address + 0xe8, thread.Context.GetX(29));
            memory.WriteUInt64((long)address + 0xf0, thread.Context.GetX(30));
            memory.WriteUInt64((long)address + 0xf8, thread.Context.GetX(31));

            memory.WriteInt64((long)address + 0x100, thread.LastPc);

            memory.WriteUInt64((long)address + 0x108, (ulong)GetPsr(thread.Context));

            memory.WriteVector128((long)address + 0x110, thread.Context.GetV(0));
            memory.WriteVector128((long)address + 0x120, thread.Context.GetV(1));
            memory.WriteVector128((long)address + 0x130, thread.Context.GetV(2));
            memory.WriteVector128((long)address + 0x140, thread.Context.GetV(3));
            memory.WriteVector128((long)address + 0x150, thread.Context.GetV(4));
            memory.WriteVector128((long)address + 0x160, thread.Context.GetV(5));
            memory.WriteVector128((long)address + 0x170, thread.Context.GetV(6));
            memory.WriteVector128((long)address + 0x180, thread.Context.GetV(7));
            memory.WriteVector128((long)address + 0x190, thread.Context.GetV(8));
            memory.WriteVector128((long)address + 0x1a0, thread.Context.GetV(9));
            memory.WriteVector128((long)address + 0x1b0, thread.Context.GetV(10));
            memory.WriteVector128((long)address + 0x1c0, thread.Context.GetV(11));
            memory.WriteVector128((long)address + 0x1d0, thread.Context.GetV(12));
            memory.WriteVector128((long)address + 0x1e0, thread.Context.GetV(13));
            memory.WriteVector128((long)address + 0x1f0, thread.Context.GetV(14));
            memory.WriteVector128((long)address + 0x200, thread.Context.GetV(15));
            memory.WriteVector128((long)address + 0x210, thread.Context.GetV(16));
            memory.WriteVector128((long)address + 0x220, thread.Context.GetV(17));
            memory.WriteVector128((long)address + 0x230, thread.Context.GetV(18));
            memory.WriteVector128((long)address + 0x240, thread.Context.GetV(19));
            memory.WriteVector128((long)address + 0x250, thread.Context.GetV(20));
            memory.WriteVector128((long)address + 0x260, thread.Context.GetV(21));
            memory.WriteVector128((long)address + 0x270, thread.Context.GetV(22));
            memory.WriteVector128((long)address + 0x280, thread.Context.GetV(23));
            memory.WriteVector128((long)address + 0x290, thread.Context.GetV(24));
            memory.WriteVector128((long)address + 0x2a0, thread.Context.GetV(25));
            memory.WriteVector128((long)address + 0x2b0, thread.Context.GetV(26));
            memory.WriteVector128((long)address + 0x2c0, thread.Context.GetV(27));
            memory.WriteVector128((long)address + 0x2d0, thread.Context.GetV(28));
            memory.WriteVector128((long)address + 0x2e0, thread.Context.GetV(29));
            memory.WriteVector128((long)address + 0x2f0, thread.Context.GetV(30));
            memory.WriteVector128((long)address + 0x300, thread.Context.GetV(31));

            memory.WriteInt32((long)address + 0x310, (int)thread.Context.Fpcr);
            memory.WriteInt32((long)address + 0x314, (int)thread.Context.Fpsr);
            memory.WriteInt64((long)address + 0x318, thread.Context.Tpidr);

            return KernelResult.Success;
        }

        private static int GetPsr(IExecutionContext context)
        {
            return (context.GetPstateFlag(PState.NFlag) ? (1 << 31) : 0) |
                   (context.GetPstateFlag(PState.ZFlag) ? (1 << 30) : 0) |
                   (context.GetPstateFlag(PState.CFlag) ? (1 << 29) : 0) |
                   (context.GetPstateFlag(PState.VFlag) ? (1 << 28) : 0);
        }
    }
}