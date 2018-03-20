using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.Core.OsHle.Exceptions;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Services;
using System;
using System.Threading;

using static Ryujinx.Core.OsHle.ErrorCode;

namespace Ryujinx.Core.OsHle.Svc
{
    partial class SvcHandler
    {
        private const int AllowedCpuIdBitmask = 0b1111;

        private const bool EnableProcessDebugging = false;

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
                Logging.Warn($"Tried to CloseHandle on invalid handle 0x{Handle:x8}!");

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
                Event.Handle.Reset();

                ThreadState.X0 = 0;
            }
            else
            {
                Logging.Warn($"Tried to ResetSignal on invalid event handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcWaitSynchronization(AThreadState ThreadState)
        {
            long HandlesPtr   = (long)ThreadState.X1;
            int  HandlesCount =  (int)ThreadState.X2;
            long Timeout      = (long)ThreadState.X3;

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            WaitHandle[] Handles = new WaitHandle[HandlesCount];

            for (int Index = 0; Index < HandlesCount; Index++)
            {
                int Handle = Memory.ReadInt32(HandlesPtr + Index * 4);

                KSynchronizationObject SyncObj = Process.HandleTable.GetData<KSynchronizationObject>(Handle);

                if (SyncObj == null)
                {
                    Logging.Warn($"Tried to WaitSynchronization on invalid handle 0x{Handle:x8}!");

                    ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                    return;
                }

                Handles[Index] = SyncObj.Handle;
            }

            Process.Scheduler.Suspend(CurrThread.ProcessorId);

            int HandleIndex;

            ulong Result = 0;

            if (Timeout != -1)
            {
                HandleIndex = WaitHandle.WaitAny(Handles, (int)(Timeout / 1000000));

                if (HandleIndex == WaitHandle.WaitTimeout)
                {
                    Result = MakeError(ErrorModule.Kernel, KernelErr.Timeout);
                }
            }
            else
            {
                HandleIndex = WaitHandle.WaitAny(Handles);
            }

            Process.Scheduler.Resume(CurrThread);

            ThreadState.X0 = Result;

            if (Result == 0)
            {
                ThreadState.X1 = (ulong)HandleIndex;
            }
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
            KSession Session = new KSession(ServiceFactory.MakeService(Name));

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

            byte[] CmdData = AMemoryHelper.ReadBytes(Memory, CmdPtr, Size);

            KSession Session = Process.HandleTable.GetData<KSession>(Handle);

            if (Session != null)
            {
                Process.Scheduler.Suspend(CurrThread.ProcessorId);

                IpcMessage Cmd = new IpcMessage(CmdData, CmdPtr);

                IpcHandler.IpcCall(Ns, Process, Memory, Session, Cmd, CmdPtr);

                Thread.Yield();

                Process.Scheduler.Resume(CurrThread);

                ThreadState.X0 = 0;
            }
            else
            {
                Logging.Warn($"Tried to SendSyncRequest on invalid session handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcBreak(AThreadState ThreadState)
        {
            long Reason  = (long)ThreadState.X0;
            long Unknown = (long)ThreadState.X1;
            long Info    = (long)ThreadState.X2;

            throw new GuestBrokeExecutionException();
        }

        private void SvcOutputDebugString(AThreadState ThreadState)
        {
            long Position = (long)ThreadState.X0;
            long Size     = (long)ThreadState.X1;

            string Str = AMemoryHelper.ReadAsciiString(Memory, Position, Size);

            Logging.Info($"SvcOutputDebugString: {Str}");

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
                InfoType == 19)
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

                default: throw new NotImplementedException($"SvcGetInfo: {InfoType} {Handle} {InfoId}");
            }

            ThreadState.X0 = 0;
        }
    }
}
