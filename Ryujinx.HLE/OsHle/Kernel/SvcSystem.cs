using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Exceptions;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using Ryujinx.HLE.OsHle.Services;
using System;
using System.Threading;

using static Ryujinx.HLE.OsHle.ErrorCode;

namespace Ryujinx.HLE.OsHle.Kernel
{
    partial class SvcHandler
    {
        private const int AllowedCpuIdBitmask = 0b1111;

        private const bool EnableProcessDebugging = false;

        private const bool IsVirtualMemoryEnabled = true; //This is always true(?)

        private void SvcExitProcess(AThreadState ThreadState)
        {
            Ns.Os.ExitProcess(ThreadState.ProcessId);
        }

        private void SvcClearEvent(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            //TODO: Implement events.

            ThreadState.X0 = 0;
        }

        private void SvcCloseHandle(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            object Obj = Process.HandleTable.CloseHandle(Handle);

            if (Obj == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Obj is KSession Session)
            {
                Session.Dispose();
            }
            else if (Obj is HTransferMem TMem)
            {
                TMem.Memory.Manager.Reprotect(
                    TMem.Position,
                    TMem.Size,
                    TMem.Perm);
            }

            ThreadState.X0 = 0;
        }

        private void SvcResetSignal(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            KEvent Event = Process.HandleTable.GetData<KEvent>(Handle);

            if (Event != null)
            {
                Event.WaitEvent.Reset();

                ThreadState.X0 = 0;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid event handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcWaitSynchronization(AThreadState ThreadState)
        {
            long  HandlesPtr   = (long)ThreadState.X1;
            int   HandlesCount =  (int)ThreadState.X2;
            ulong Timeout      =       ThreadState.X3;

            Ns.Log.PrintDebug(LogClass.KernelSvc,
                "HandlesPtr = "   + HandlesPtr  .ToString("x16") + ", " +
                "HandlesCount = " + HandlesCount.ToString("x8")  + ", " +
                "Timeout = "      + Timeout     .ToString("x16"));

            if ((uint)HandlesCount > 0x40)
            {
                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.CountOutOfRange);

                return;
            }

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            WaitHandle[] Handles = new WaitHandle[HandlesCount + 1];

            for (int Index = 0; Index < HandlesCount; Index++)
            {
                int Handle = Memory.ReadInt32(HandlesPtr + Index * 4);

                KSynchronizationObject SyncObj = Process.HandleTable.GetData<KSynchronizationObject>(Handle);

                if (SyncObj == null)
                {
                    Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid handle 0x{Handle:x8}!");

                    ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                    return;
                }

                Handles[Index] = SyncObj.WaitEvent;
            }

            using (AutoResetEvent WaitEvent = new AutoResetEvent(false))
            {
                if (!SyncWaits.TryAdd(CurrThread, WaitEvent))
                {
                    throw new InvalidOperationException();
                }

                Handles[HandlesCount] = WaitEvent;

                Process.Scheduler.Suspend(CurrThread);

                int HandleIndex;

                ulong Result = 0;

                if (Timeout != ulong.MaxValue)
                {
                    HandleIndex = WaitHandle.WaitAny(Handles, NsTimeConverter.GetTimeMs(Timeout));
                }
                else
                {
                    HandleIndex = WaitHandle.WaitAny(Handles);
                }

                if (HandleIndex == WaitHandle.WaitTimeout)
                {
                    Result = MakeError(ErrorModule.Kernel, KernelErr.Timeout);
                }
                else if (HandleIndex == HandlesCount)
                {
                    Result = MakeError(ErrorModule.Kernel, KernelErr.Canceled);
                }

                SyncWaits.TryRemove(CurrThread, out _);

                Process.Scheduler.Resume(CurrThread);

                ThreadState.X0 = Result;

                if (Result == 0)
                {
                    ThreadState.X1 = (ulong)HandleIndex;
                }
            }
        }

        private void SvcCancelSynchronization(AThreadState ThreadState)
        {
            int ThreadHandle = (int)ThreadState.X0;

            KThread Thread = GetThread(ThreadState.Tpidr, ThreadHandle);

            if (Thread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{ThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (SyncWaits.TryRemove(Thread, out AutoResetEvent WaitEvent))
            {
                WaitEvent.Set();
            }

            ThreadState.X0 = 0;
        }

        private void SvcGetSystemTick(AThreadState ThreadState)
        {
            ThreadState.X0 = ThreadState.CntpctEl0;
        }

        private void SvcConnectToNamedPort(AThreadState ThreadState)
        {
            long StackPtr = (long)ThreadState.X0;
            long NamePtr  = (long)ThreadState.X1;

            string Name = AMemoryHelper.ReadAsciiString(Memory, NamePtr, 8);

            //TODO: Validate that app has perms to access the service, and that the service
            //actually exists, return error codes otherwise.
            KSession Session = new KSession(ServiceFactory.MakeService(Name), Name);

            ulong Handle = (ulong)Process.HandleTable.OpenHandle(Session);

            ThreadState.X0 = 0;
            ThreadState.X1 = Handle;
        }

        private void SvcSendSyncRequest(AThreadState ThreadState)
        {
            SendSyncRequest(ThreadState, ThreadState.Tpidr, 0x100, (int)ThreadState.X0);
        }

        private void SvcSendSyncRequestWithUserBuffer(AThreadState ThreadState)
        {
            SendSyncRequest(
                      ThreadState,
                (long)ThreadState.X0,
                (long)ThreadState.X1,
                 (int)ThreadState.X2);
        }

        private void SendSyncRequest(AThreadState ThreadState, long CmdPtr, long Size, int Handle)
        {
            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            byte[] CmdData = Memory.ReadBytes(CmdPtr, Size);

            KSession Session = Process.HandleTable.GetData<KSession>(Handle);

            if (Session != null)
            {
                Process.Scheduler.Suspend(CurrThread);

                IpcMessage Cmd = new IpcMessage(CmdData, CmdPtr);

                long Result = IpcHandler.IpcCall(Ns, Process, Memory, Session, Cmd, CmdPtr);

                Thread.Yield();

                Process.Scheduler.Resume(CurrThread);

                ThreadState.X0 = (ulong)Result;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid session handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcBreak(AThreadState ThreadState)
        {
            long Reason  = (long)ThreadState.X0;
            long Unknown = (long)ThreadState.X1;
            long Info    = (long)ThreadState.X2;

            Process.PrintStackTrace(ThreadState);

            throw new GuestBrokeExecutionException();
        }

        private void SvcOutputDebugString(AThreadState ThreadState)
        {
            long Position = (long)ThreadState.X0;
            long Size     = (long)ThreadState.X1;

            string Str = AMemoryHelper.ReadAsciiString(Memory, Position, Size);

            Ns.Log.PrintWarning(LogClass.KernelSvc, Str);

            ThreadState.X0 = 0;
        }

        private void SvcGetInfo(AThreadState ThreadState)
        {
            long StackPtr = (long)ThreadState.X0;
            int  InfoType =  (int)ThreadState.X1;
            long Handle   = (long)ThreadState.X2;
            int  InfoId   =  (int)ThreadState.X3;

            //Fail for info not available on older Kernel versions.
            if (InfoType == 18 ||
                InfoType == 19 ||
                InfoType == 20)
            {
                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidInfo);

                return;
            }

            switch (InfoType)
            {
                case 0:
                    ThreadState.X1 = AllowedCpuIdBitmask;
                    break;

                case 2:
                    ThreadState.X1 = MemoryRegions.MapRegionAddress;
                    break;

                case 3:
                    ThreadState.X1 = MemoryRegions.MapRegionSize;
                    break;

                case 4:
                    ThreadState.X1 = MemoryRegions.HeapRegionAddress;
                    break;

                case 5:
                    ThreadState.X1 = MemoryRegions.HeapRegionSize;
                    break;

                case 6:
                    ThreadState.X1 = MemoryRegions.TotalMemoryAvailable;
                    break;

                case 7:
                    ThreadState.X1 = MemoryRegions.TotalMemoryUsed + CurrentHeapSize;
                    break;

                case 8:
                    ThreadState.X1 = EnableProcessDebugging ? 1 : 0;
                    break;

                case 11:
                    ThreadState.X1 = (ulong)Rng.Next() + ((ulong)Rng.Next() << 32);
                    break;

                case 12:
                    ThreadState.X1 = MemoryRegions.AddrSpaceStart;
                    break;

                case 13:
                    ThreadState.X1 = MemoryRegions.AddrSpaceSize;
                    break;

                case 14:
                    ThreadState.X1 = MemoryRegions.MapRegionAddress;
                    break;

                case 15:
                    ThreadState.X1 = MemoryRegions.MapRegionSize;
                    break;

                case 16:
                    ThreadState.X1 = IsVirtualMemoryEnabled ? 1 : 0;
                    break;

                default:
                    Process.PrintStackTrace(ThreadState);

                    throw new NotImplementedException($"SvcGetInfo: {InfoType} {Handle:x8} {InfoId}");
            }

            ThreadState.X0 = 0;
        }
    }
}
