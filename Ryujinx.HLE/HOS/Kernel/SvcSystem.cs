using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services;
using Ryujinx.HLE.Logging;
using System;
using System.Threading;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private const int AllowedCpuIdBitmask = 0b1111;

        private const bool EnableProcessDebugging = false;

        private void SvcExitProcess(AThreadState ThreadState)
        {
            Device.System.ExitProcess(Process.ProcessId);
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
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Obj is KSession Session)
            {
                Session.Dispose();
            }
            else if (Obj is KTransferMemory TransferMemory)
            {
                Process.MemoryManager.ResetTransferMemory(
                    TransferMemory.Position,
                    TransferMemory.Size);
            }

            ThreadState.X0 = 0;
        }

        private void SvcResetSignal(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            KEvent Event = Process.HandleTable.GetData<KEvent>(Handle);

            if (Event != null)
            {
                Event.Reset();

                ThreadState.X0 = 0;
            }
            else
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid event handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
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
            KSession Session = new KSession(ServiceFactory.MakeService(System, Name), Name);

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

        private void SendSyncRequest(AThreadState ThreadState, long MessagePtr, long Size, int Handle)
        {
            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            byte[] MessageData = Memory.ReadBytes(MessagePtr, Size);

            KSession Session = Process.HandleTable.GetData<KSession>(Handle);

            if (Session != null)
            {
                //Process.Scheduler.Suspend(CurrThread);

                System.CriticalSectionLock.Lock();

                KThread CurrentThread = System.Scheduler.GetCurrentThread();

                CurrentThread.SignaledObj   = null;
                CurrentThread.ObjSyncResult = 0;

                CurrentThread.Reschedule(ThreadSchedState.Paused);

                IpcMessage Message = new IpcMessage(MessageData, MessagePtr);

                ThreadPool.QueueUserWorkItem(ProcessIpcRequest, new HleIpcMessage(
                    CurrentThread,
                    Session,
                    Message,
                    MessagePtr));

                System.CriticalSectionLock.Unlock();

                ThreadState.X0 = (ulong)CurrentThread.ObjSyncResult;
            }
            else
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid session handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void ProcessIpcRequest(object State)
        {
            HleIpcMessage IpcMessage = (HleIpcMessage)State;

            IpcMessage.Thread.ObjSyncResult = (int)IpcHandler.IpcCall(
                Device,
                Process,
                Memory,
                IpcMessage.Session,
                IpcMessage.Message,
                IpcMessage.MessagePtr);

            IpcMessage.Thread.Reschedule(ThreadSchedState.Running);
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

            Device.Log.PrintWarning(LogClass.KernelSvc, Str);

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
                InfoType == 20 ||
                InfoType == 21)
            {
                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidEnumValue);

                return;
            }

            switch (InfoType)
            {
                case 0:
                    ThreadState.X1 = AllowedCpuIdBitmask;
                    break;

                case 2:
                    ThreadState.X1 = (ulong)Process.MemoryManager.MapRegionStart;
                    break;

                case 3:
                    ThreadState.X1 = (ulong)Process.MemoryManager.MapRegionEnd -
                                     (ulong)Process.MemoryManager.MapRegionStart;
                    break;

                case 4:
                    ThreadState.X1 = (ulong)Process.MemoryManager.HeapRegionStart;
                    break;

                case 5:
                    ThreadState.X1 = (ulong)Process.MemoryManager.HeapRegionEnd -
                                     (ulong)Process.MemoryManager.HeapRegionStart;
                    break;

                case 6:
                    ThreadState.X1 = (ulong)Process.Device.Memory.Allocator.TotalAvailableSize;
                    break;

                case 7:
                    ThreadState.X1 = (ulong)Process.Device.Memory.Allocator.TotalUsedSize;
                    break;

                case 8:
                    ThreadState.X1 = EnableProcessDebugging ? 1 : 0;
                    break;

                case 11:
                    ThreadState.X1 = (ulong)Rng.Next() + ((ulong)Rng.Next() << 32);
                    break;

                case 12:
                    ThreadState.X1 = (ulong)Process.MemoryManager.AddrSpaceStart;
                    break;

                case 13:
                    ThreadState.X1 = (ulong)Process.MemoryManager.AddrSpaceEnd -
                                     (ulong)Process.MemoryManager.AddrSpaceStart;
                    break;

                case 14:
                    ThreadState.X1 = (ulong)Process.MemoryManager.NewMapRegionStart;
                    break;

                case 15:
                    ThreadState.X1 = (ulong)Process.MemoryManager.NewMapRegionEnd -
                                     (ulong)Process.MemoryManager.NewMapRegionStart;
                    break;

                case 16:
                    ThreadState.X1 = (ulong)(Process.MetaData?.SystemResourceSize ?? 0);
                    break;

                case 17:
                    ThreadState.X1 = (ulong)Process.MemoryManager.PersonalMmHeapUsage;
                    break;

                default:
                    Process.PrintStackTrace(ThreadState);

                    throw new NotImplementedException($"SvcGetInfo: {InfoType} 0x{Handle:x8} {InfoId}");
            }

            ThreadState.X0 = 0;
        }
    }
}
