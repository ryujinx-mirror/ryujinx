using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    partial class SvcHandler
    {
        public KernelResult WaitSynchronization64([R(1)] ulong handlesPtr, [R(2)] int handlesCount, [R(3)] long timeout, [R(1)] out int handleIndex)
        {
            return WaitSynchronization(handlesPtr, handlesCount, timeout, out handleIndex);
        }

        public KernelResult WaitSynchronization32(
            [R(0)] uint    timeoutLow,
            [R(1)] uint    handlesPtr,
            [R(2)] int     handlesCount,
            [R(3)] uint    timeoutHigh,
            [R(1)] out int handleIndex)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return WaitSynchronization(handlesPtr, handlesCount, timeout, out handleIndex);
        }

        private KernelResult WaitSynchronization(ulong handlesPtr, int handlesCount, long timeout, out int handleIndex)
        {
            handleIndex = 0;

            if ((uint)handlesCount > 0x40)
            {
                return KernelResult.MaximumExceeded;
            }

            List<KSynchronizationObject> syncObjs = new List<KSynchronizationObject>();

            for (int index = 0; index < handlesCount; index++)
            {
                int handle = _process.CpuMemory.ReadInt32((long)handlesPtr + index * 4);

                KSynchronizationObject syncObj = _process.HandleTable.GetObject<KSynchronizationObject>(handle);

                if (syncObj == null)
                {
                    break;
                }

                syncObjs.Add(syncObj);
            }

            return _system.Synchronization.WaitFor(syncObjs.ToArray(), timeout, out handleIndex);
        }

        public KernelResult CancelSynchronization64([R(0)] int handle)
        {
            return CancelSynchronization(handle);
        }

        public KernelResult CancelSynchronization32([R(0)] int handle)
        {
            return CancelSynchronization(handle);
        }

        private KernelResult CancelSynchronization(int handle)
        {
            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            thread.CancelSynchronization();

            return KernelResult.Success;
        }

        public KernelResult ArbitrateLock64([R(0)] int ownerHandle, [R(1)] ulong mutexAddress, [R(2)] int requesterHandle)
        {
            return ArbitrateLock(ownerHandle, mutexAddress, requesterHandle);
        }

        public KernelResult ArbitrateLock32([R(0)] int ownerHandle, [R(1)] uint mutexAddress, [R(2)] int requesterHandle)
        {
            return ArbitrateLock(ownerHandle, mutexAddress, requesterHandle);
        }

        private KernelResult ArbitrateLock(int ownerHandle, ulong mutexAddress, int requesterHandle)
        {
            if (IsPointingInsideKernel(mutexAddress))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            return currentProcess.AddressArbiter.ArbitrateLock(ownerHandle, mutexAddress, requesterHandle);
        }

        public KernelResult ArbitrateUnlock64([R(0)] ulong mutexAddress)
        {
            return ArbitrateUnlock(mutexAddress);
        }

        public KernelResult ArbitrateUnlock32([R(0)] uint mutexAddress)
        {
            return ArbitrateUnlock(mutexAddress);
        }

        private KernelResult ArbitrateUnlock(ulong mutexAddress)
        {
            if (IsPointingInsideKernel(mutexAddress))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            return currentProcess.AddressArbiter.ArbitrateUnlock(mutexAddress);
        }

        public KernelResult WaitProcessWideKeyAtomic64(
            [R(0)] ulong mutexAddress,
            [R(1)] ulong condVarAddress,
            [R(2)] int   handle,
            [R(3)] long  timeout)
        {
            return WaitProcessWideKeyAtomic(mutexAddress, condVarAddress, handle, timeout);
        }

        public KernelResult WaitProcessWideKeyAtomic32(
            [R(0)] uint mutexAddress,
            [R(1)] uint condVarAddress,
            [R(2)] int  handle,
            [R(3)] uint timeoutLow,
            [R(4)] uint timeoutHigh)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return WaitProcessWideKeyAtomic(mutexAddress, condVarAddress, handle, timeout);
        }

        private KernelResult WaitProcessWideKeyAtomic(
            ulong mutexAddress,
            ulong condVarAddress,
            int   handle,
            long  timeout)
        {
            if (IsPointingInsideKernel(mutexAddress))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            return currentProcess.AddressArbiter.WaitProcessWideKeyAtomic(
                mutexAddress,
                condVarAddress,
                handle,
                timeout);
        }

        public KernelResult SignalProcessWideKey64([R(0)] ulong address, [R(1)] int count)
        {
            return SignalProcessWideKey(address, count);
        }

        public KernelResult SignalProcessWideKey32([R(0)] uint address, [R(1)] int count)
        {
            return SignalProcessWideKey(address, count);
        }

        private KernelResult SignalProcessWideKey(ulong address, int count)
        {
            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            currentProcess.AddressArbiter.SignalProcessWideKey(address, count);

            return KernelResult.Success;
        }

        public KernelResult WaitForAddress64([R(0)] ulong address, [R(1)] ArbitrationType type, [R(2)] int value, [R(3)] long timeout)
        {
            return WaitForAddress(address, type, value, timeout);
        }

        public KernelResult WaitForAddress32([R(0)] uint address, [R(1)] ArbitrationType type, [R(2)] int value, [R(3)] uint timeoutLow, [R(4)] uint timeoutHigh)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return WaitForAddress(address, type, value, timeout);
        }

        private KernelResult WaitForAddress(ulong address, ArbitrationType type, int value, long timeout)
        {
            if (IsPointingInsideKernel(address))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KernelResult result;

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
                    result = KernelResult.InvalidEnumValue;
                    break;
            }

            return result;
        }

        public KernelResult SignalToAddress64([R(0)] ulong address, [R(1)] SignalType type, [R(2)] int value, [R(3)] int count)
        {
            return SignalToAddress(address, type, value, count);
        }

        public KernelResult SignalToAddress32([R(0)] uint address, [R(1)] SignalType type, [R(2)] int value, [R(3)] int count)
        {
            return SignalToAddress(address, type, value, count);
        }

        private KernelResult SignalToAddress(ulong address, SignalType type, int value, int count)
        {
            if (IsPointingInsideKernel(address))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KernelResult result;

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
                    result = KernelResult.InvalidEnumValue;
                    break;
            }

            return result;
        }

        private bool IsPointingInsideKernel(ulong address)
        {
            return (address + 0x1000000000) < 0xffffff000;
        }

        private bool IsAddressNotWordAligned(ulong address)
        {
            return (address & 3) != 0;
        }
    }
}