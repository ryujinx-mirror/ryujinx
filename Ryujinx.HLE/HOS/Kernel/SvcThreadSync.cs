using ChocolArm64.State;
using Ryujinx.Common.Logging;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private void SvcWaitSynchronization(CpuThreadState threadState)
        {
            long handlesPtr   = (long)threadState.X1;
            int  handlesCount =  (int)threadState.X2;
            long timeout      = (long)threadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "HandlesPtr = 0x"   + handlesPtr  .ToString("x16") + ", " +
                "HandlesCount = 0x" + handlesCount.ToString("x8")  + ", " +
                "Timeout = 0x"      + timeout     .ToString("x16"));

            if ((uint)handlesCount > 0x40)
            {
                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.CountOutOfRange);

                return;
            }

            List<KSynchronizationObject> syncObjs = new List<KSynchronizationObject>();

            for (int index = 0; index < handlesCount; index++)
            {
                int handle = _memory.ReadInt32(handlesPtr + index * 4);

                Logger.PrintDebug(LogClass.KernelSvc, $"Sync handle 0x{handle:x8}");

                KSynchronizationObject syncObj = _process.HandleTable.GetObject<KSynchronizationObject>(handle);

                if (syncObj == null)
                {
                    break;
                }

                syncObjs.Add(syncObj);
            }

            int hndIndex = (int)threadState.X1;

            ulong high = threadState.X1 & (0xffffffffUL << 32);

            long result = _system.Synchronization.WaitFor(syncObjs.ToArray(), timeout, ref hndIndex);

            if (result != 0)
            {
                if (result == MakeError(ErrorModule.Kernel, KernelErr.Timeout) ||
                    result == MakeError(ErrorModule.Kernel, KernelErr.Cancelled))
                {
                    Logger.PrintDebug(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
                }
                else
                {
                    Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
                }
            }

            threadState.X0 = (ulong)result;
            threadState.X1 = (uint)hndIndex | high;
        }

        private void SvcCancelSynchronization(CpuThreadState threadState)
        {
            int threadHandle = (int)threadState.X0;

            Logger.PrintDebug(LogClass.KernelSvc, "ThreadHandle = 0x" + threadHandle.ToString("x8"));

            KThread thread = _process.HandleTable.GetKThread(threadHandle);

            if (thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{threadHandle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            thread.CancelSynchronization();

            threadState.X0 = 0;
        }

        private void SvcArbitrateLock(CpuThreadState threadState)
        {
            int  ownerHandle     =  (int)threadState.X0;
            long mutexAddress    = (long)threadState.X1;
            int  requesterHandle =  (int)threadState.X2;

            Logger.PrintDebug(LogClass.KernelSvc,
                "OwnerHandle = 0x"     + ownerHandle    .ToString("x8")  + ", " +
                "MutexAddress = 0x"    + mutexAddress   .ToString("x16") + ", " +
                "RequesterHandle = 0x" + requesterHandle.ToString("x8"));

            if (IsPointingInsideKernel(mutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{mutexAddress:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{mutexAddress:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            long result = currentProcess.AddressArbiter.ArbitrateLock(ownerHandle, mutexAddress, requesterHandle);

            if (result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcArbitrateUnlock(CpuThreadState threadState)
        {
            long mutexAddress = (long)threadState.X0;

            Logger.PrintDebug(LogClass.KernelSvc, "MutexAddress = 0x" + mutexAddress.ToString("x16"));

            if (IsPointingInsideKernel(mutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{mutexAddress:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{mutexAddress:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            long result = currentProcess.AddressArbiter.ArbitrateUnlock(mutexAddress);

            if (result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcWaitProcessWideKeyAtomic(CpuThreadState threadState)
        {
            long  mutexAddress   = (long)threadState.X0;
            long  condVarAddress = (long)threadState.X1;
            int   threadHandle   =  (int)threadState.X2;
            long  timeout        = (long)threadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "MutexAddress = 0x"   + mutexAddress  .ToString("x16") + ", " +
                "CondVarAddress = 0x" + condVarAddress.ToString("x16") + ", " +
                "ThreadHandle = 0x"   + threadHandle  .ToString("x8")  + ", " +
                "Timeout = 0x"        + timeout       .ToString("x16"));

            if (IsPointingInsideKernel(mutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{mutexAddress:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{mutexAddress:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            long result = currentProcess.AddressArbiter.WaitProcessWideKeyAtomic(
                mutexAddress,
                condVarAddress,
                threadHandle,
                timeout);

            if (result != 0)
            {
                if (result == MakeError(ErrorModule.Kernel, KernelErr.Timeout))
                {
                    Logger.PrintDebug(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
                }
                else
                {
                    Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
                }
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcSignalProcessWideKey(CpuThreadState threadState)
        {
            long address = (long)threadState.X0;
            int  count   =  (int)threadState.X1;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Address = 0x" + address.ToString("x16") + ", " +
                "Count = 0x"   + count  .ToString("x8"));

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            currentProcess.AddressArbiter.SignalProcessWideKey(address, count);

            threadState.X0 = 0;
        }

        private void SvcWaitForAddress(CpuThreadState threadState)
        {
            long            address =            (long)threadState.X0;
            ArbitrationType type    = (ArbitrationType)threadState.X1;
            int             value   =             (int)threadState.X2;
            long            timeout =            (long)threadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Address = 0x" + address.ToString("x16") + ", " +
                "Type = "      + type   .ToString()      + ", " +
                "Value = 0x"   + value  .ToString("x8")  + ", " +
                "Timeout = 0x" + timeout.ToString("x16"));

            if (IsPointingInsideKernel(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address 0x{address:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned address 0x{address:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            long result;

            switch (type)
            {
                case ArbitrationType.WaitIfLessThan:
                    result = currentProcess.AddressArbiter.WaitForAddressIfLessThan(address, value, false, timeout);
                    break;

                case ArbitrationType.DecrementAndWaitIfLessThan:
                    result = currentProcess.AddressArbiter.WaitForAddressIfLessThan(address, value, true, timeout);
                    break;

                case ArbitrationType.WaitIfEqual:
                    result = currentProcess.AddressArbiter.WaitForAddressIfEqual(address, value, timeout);
                    break;

                default:
                    result = MakeError(ErrorModule.Kernel, KernelErr.InvalidEnumValue);
                    break;
            }

            if (result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcSignalToAddress(CpuThreadState threadState)
        {
            long       address =       (long)threadState.X0;
            SignalType type    = (SignalType)threadState.X1;
            int        value   =        (int)threadState.X2;
            int        count   =        (int)threadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Address = 0x" + address.ToString("x16") + ", " +
                "Type = "      + type   .ToString()      + ", " +
                "Value = 0x"   + value  .ToString("x8")  + ", " +
                "Count = 0x"   + count  .ToString("x8"));

            if (IsPointingInsideKernel(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address 0x{address:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned address 0x{address:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            long result;

            switch (type)
            {
                case SignalType.Signal:
                    result = currentProcess.AddressArbiter.Signal(address, count);
                    break;

                case SignalType.SignalAndIncrementIfEqual:
                    result = currentProcess.AddressArbiter.SignalAndIncrementIfEqual(address, value, count);
                    break;

                case SignalType.SignalAndModifyIfEqual:
                    result = currentProcess.AddressArbiter.SignalAndModifyIfEqual(address, value, count);
                    break;

                default:
                    result = MakeError(ErrorModule.Kernel, KernelErr.InvalidEnumValue);
                    break;
            }

            if (result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private bool IsPointingInsideKernel(long address)
        {
            return ((ulong)address + 0x1000000000) < 0xffffff000;
        }

        private bool IsAddressNotWordAligned(long address)
        {
            return (address & 3) != 0;
        }
    }
}