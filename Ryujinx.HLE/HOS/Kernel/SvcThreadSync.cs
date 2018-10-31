using ChocolArm64.State;
using Ryujinx.Common.Logging;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private void SvcWaitSynchronization(CpuThreadState ThreadState)
        {
            long HandlesPtr   = (long)ThreadState.X1;
            int  HandlesCount =  (int)ThreadState.X2;
            long Timeout      = (long)ThreadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "HandlesPtr = 0x"   + HandlesPtr  .ToString("x16") + ", " +
                "HandlesCount = 0x" + HandlesCount.ToString("x8")  + ", " +
                "Timeout = 0x"      + Timeout     .ToString("x16"));

            if ((uint)HandlesCount > 0x40)
            {
                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.CountOutOfRange);

                return;
            }

            List<KSynchronizationObject> SyncObjs = new List<KSynchronizationObject>();

            for (int Index = 0; Index < HandlesCount; Index++)
            {
                int Handle = Memory.ReadInt32(HandlesPtr + Index * 4);

                KSynchronizationObject SyncObj = Process.HandleTable.GetObject<KSynchronizationObject>(Handle);

                if (SyncObj == null)
                {
                    break;
                }

                SyncObjs.Add(SyncObj);
            }

            int HndIndex = (int)ThreadState.X1;

            ulong High = ThreadState.X1 & (0xffffffffUL << 32);

            long Result = System.Synchronization.WaitFor(SyncObjs.ToArray(), Timeout, ref HndIndex);

            if (Result != 0)
            {
                if (Result == MakeError(ErrorModule.Kernel, KernelErr.Timeout) ||
                    Result == MakeError(ErrorModule.Kernel, KernelErr.Cancelled))
                {
                    Logger.PrintDebug(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
                }
                else
                {
                    Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
                }
            }

            ThreadState.X0 = (ulong)Result;
            ThreadState.X1 = (uint)HndIndex | High;
        }

        private void SvcCancelSynchronization(CpuThreadState ThreadState)
        {
            int ThreadHandle = (int)ThreadState.X0;

            Logger.PrintDebug(LogClass.KernelSvc, "ThreadHandle = 0x" + ThreadHandle.ToString("x8"));

            KThread Thread = Process.HandleTable.GetKThread(ThreadHandle);

            if (Thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{ThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            Thread.CancelSynchronization();

            ThreadState.X0 = 0;
        }

        private void SvcArbitrateLock(CpuThreadState ThreadState)
        {
            int  OwnerHandle     =  (int)ThreadState.X0;
            long MutexAddress    = (long)ThreadState.X1;
            int  RequesterHandle =  (int)ThreadState.X2;

            Logger.PrintDebug(LogClass.KernelSvc,
                "OwnerHandle = 0x"     + OwnerHandle    .ToString("x8")  + ", " +
                "MutexAddress = 0x"    + MutexAddress   .ToString("x16") + ", " +
                "RequesterHandle = 0x" + RequesterHandle.ToString("x8"));

            if (IsPointingInsideKernel(MutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(MutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            long Result = System.AddressArbiter.ArbitrateLock(
                Process,
                Memory,
                OwnerHandle,
                MutexAddress,
                RequesterHandle);

            if (Result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcArbitrateUnlock(CpuThreadState ThreadState)
        {
            long MutexAddress = (long)ThreadState.X0;

            Logger.PrintDebug(LogClass.KernelSvc, "MutexAddress = 0x" + MutexAddress.ToString("x16"));

            if (IsPointingInsideKernel(MutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(MutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            long Result = System.AddressArbiter.ArbitrateUnlock(Memory, MutexAddress);

            if (Result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcWaitProcessWideKeyAtomic(CpuThreadState ThreadState)
        {
            long  MutexAddress   = (long)ThreadState.X0;
            long  CondVarAddress = (long)ThreadState.X1;
            int   ThreadHandle   =  (int)ThreadState.X2;
            long  Timeout        = (long)ThreadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "MutexAddress = 0x"   + MutexAddress  .ToString("x16") + ", " +
                "CondVarAddress = 0x" + CondVarAddress.ToString("x16") + ", " +
                "ThreadHandle = 0x"   + ThreadHandle  .ToString("x8")  + ", " +
                "Timeout = 0x"        + Timeout       .ToString("x16"));

            if (IsPointingInsideKernel(MutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(MutexAddress))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            long Result = System.AddressArbiter.WaitProcessWideKeyAtomic(
                Memory,
                MutexAddress,
                CondVarAddress,
                ThreadHandle,
                Timeout);

            if (Result != 0)
            {
                if (Result == MakeError(ErrorModule.Kernel, KernelErr.Timeout))
                {
                    Logger.PrintDebug(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
                }
                else
                {
                    Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
                }
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcSignalProcessWideKey(CpuThreadState ThreadState)
        {
            long Address = (long)ThreadState.X0;
            int  Count   =  (int)ThreadState.X1;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Address = 0x" + Address.ToString("x16") + ", " +
                "Count = 0x"   + Count  .ToString("x8"));

            System.AddressArbiter.SignalProcessWideKey(Process, Memory, Address, Count);

            ThreadState.X0 = 0;
        }

        private void SvcWaitForAddress(CpuThreadState ThreadState)
        {
            long            Address =            (long)ThreadState.X0;
            ArbitrationType Type    = (ArbitrationType)ThreadState.X1;
            int             Value   =             (int)ThreadState.X2;
            long            Timeout =            (long)ThreadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Address = 0x" + Address.ToString("x16") + ", " +
                "Type = "      + Type   .ToString()      + ", " +
                "Value = 0x"   + Value  .ToString("x8")  + ", " +
                "Timeout = 0x" + Timeout.ToString("x16"));

            if (IsPointingInsideKernel(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address 0x{Address:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned address 0x{Address:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            long Result;

            switch (Type)
            {
                case ArbitrationType.WaitIfLessThan:
                    Result = System.AddressArbiter.WaitForAddressIfLessThan(Memory, Address, Value, false, Timeout);
                    break;

                case ArbitrationType.DecrementAndWaitIfLessThan:
                    Result = System.AddressArbiter.WaitForAddressIfLessThan(Memory, Address, Value, true, Timeout);
                    break;

                case ArbitrationType.WaitIfEqual:
                    Result = System.AddressArbiter.WaitForAddressIfEqual(Memory, Address, Value, Timeout);
                    break;

                default:
                    Result = MakeError(ErrorModule.Kernel, KernelErr.InvalidEnumValue);
                    break;
            }

            if (Result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcSignalToAddress(CpuThreadState ThreadState)
        {
            long       Address =       (long)ThreadState.X0;
            SignalType Type    = (SignalType)ThreadState.X1;
            int        Value   =        (int)ThreadState.X2;
            int        Count   =        (int)ThreadState.X3;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Address = 0x" + Address.ToString("x16") + ", " +
                "Type = "      + Type   .ToString()      + ", " +
                "Value = 0x"   + Value  .ToString("x8")  + ", " +
                "Count = 0x"   + Count  .ToString("x8"));

            if (IsPointingInsideKernel(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address 0x{Address:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsAddressNotWordAligned(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Unaligned address 0x{Address:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            long Result;

            switch (Type)
            {
                case SignalType.Signal:
                    Result = System.AddressArbiter.Signal(Address, Count);
                    break;

                case SignalType.SignalAndIncrementIfEqual:
                    Result = System.AddressArbiter.SignalAndIncrementIfEqual(Memory, Address, Value, Count);
                    break;

                case SignalType.SignalAndModifyIfEqual:
                    Result = System.AddressArbiter.SignalAndModifyIfEqual(Memory, Address, Value, Count);
                    break;

                default:
                    Result = MakeError(ErrorModule.Kernel, KernelErr.InvalidEnumValue);
                    break;
            }

            if (Result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private bool IsPointingInsideKernel(long Address)
        {
            return ((ulong)Address + 0x1000000000) < 0xffffff000;
        }

        private bool IsAddressNotWordAligned(long Address)
        {
            return (Address & 3) != 0;
        }
    }
}