using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.Core.OsHle.Exceptions;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Threading;

namespace Ryujinx.Core.OsHle.Svc
{
    partial class SvcHandler
    {
        private void SvcExitProcess(AThreadState ThreadState) => Ns.Os.ExitProcess(ThreadState.ProcessId);

        private void SvcCloseHandle(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            Ns.Os.CloseHandle(Handle);

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcResetSignal(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            //TODO: Implement events.

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcWaitSynchronization(AThreadState ThreadState)
        {
            long HandlesPtr   = (long)ThreadState.X0;
            int  HandlesCount =  (int)ThreadState.X2;
            long Timeout      = (long)ThreadState.X3;

            //TODO: Implement events.

            HThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            Process.Scheduler.Suspend(CurrThread.ProcessorId);
            Process.Scheduler.Resume(CurrThread);

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcGetSystemTick(AThreadState ThreadState)
        {
            ThreadState.X0 = (ulong)ThreadState.CntpctEl0;
        }

        private void SvcConnectToNamedPort(AThreadState ThreadState)
        {
            long StackPtr = (long)ThreadState.X0;
            long NamePtr  = (long)ThreadState.X1;

            string Name = AMemoryHelper.ReadAsciiString(Memory, NamePtr, 8);

            //TODO: Validate that app has perms to access the service, and that the service
            //actually exists, return error codes otherwise.

            HSession Session = new HSession(Name);

            ThreadState.X1 = (ulong)Ns.Os.Handles.GenerateId(Session);
            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcSendSyncRequest(AThreadState ThreadState)
        {
            SendSyncRequest(ThreadState, false);
        }

        private void SvcSendSyncRequestWithUserBuffer(AThreadState ThreadState)
        {
            SendSyncRequest(ThreadState, true);
        }

        private void SendSyncRequest(AThreadState ThreadState, bool UserBuffer)
        {
            long CmdPtr = ThreadState.Tpidr;
            long Size   = 0x100;
            int  Handle = 0;

            if (UserBuffer)
            {
                CmdPtr = (long)ThreadState.X0;
                Size   = (long)ThreadState.X1;
                Handle =  (int)ThreadState.X2;
            }
            else
            {
                Handle = (int)ThreadState.X0;
            }

            HThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            Process.Scheduler.Suspend(CurrThread.ProcessorId);

            byte[] CmdData = AMemoryHelper.ReadBytes(Memory, CmdPtr, (int)Size);

            HSession Session = Ns.Os.Handles.GetData<HSession>(Handle);

            IpcMessage Cmd = new IpcMessage(CmdData, CmdPtr, Session is HDomain);

            if (Session != null)
            {
                IpcHandler.IpcCall(Ns, Memory, Session, Cmd, ThreadState.ThreadId, CmdPtr, Handle);

                byte[] Response = AMemoryHelper.ReadBytes(Memory, CmdPtr, (int)Size);

                ThreadState.X0 = (int)SvcResult.Success;
            }
            else
            {
                ThreadState.X0 = (int)SvcResult.ErrBadIpcReq;
            }

            Thread.Yield();

            Process.Scheduler.Resume(CurrThread);
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

            string Str = AMemoryHelper.ReadAsciiString(Memory, Position, (int)Size);

            Logging.Info($"SvcOutputDebugString: {Str}");

            ThreadState.X0 = (int)SvcResult.Success;
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
                ThreadState.X0 = (int)SvcResult.ErrBadInfo;

                return;
            }

            switch (InfoType)
            {
                case 0:  ThreadState.X1 = AllowedCpuIdBitmask();           break;
                case 2:  ThreadState.X1 = GetMapRegionBaseAddr();          break;
                case 3:  ThreadState.X1 = GetMapRegionSize();              break;
                case 4:  ThreadState.X1 = GetHeapRegionBaseAddr();         break;
                case 5:  ThreadState.X1 = GetHeapRegionSize();             break;
                case 6:  ThreadState.X1 = GetTotalMem();                   break;
                case 7:  ThreadState.X1 = GetUsedMem();                    break;
                case 8:  ThreadState.X1 = IsCurrentProcessBeingDebugged(); break;
                case 11: ThreadState.X1 = GetRnd64();                      break;
                case 12: ThreadState.X1 = GetAddrSpaceBaseAddr();          break;
                case 13: ThreadState.X1 = GetAddrSpaceSize();              break;
                case 14: ThreadState.X1 = GetMapRegionBaseAddr();          break;
                case 15: ThreadState.X1 = GetMapRegionSize();              break;

                default: throw new NotImplementedException($"SvcGetInfo: {InfoType} {Handle} {InfoId}");
            }

            ThreadState.X0 = (int)SvcResult.Success;
        }
        
        private ulong AllowedCpuIdBitmask()
        {
            return 0xF; //Mephisto value.
        }

        private ulong GetMapRegionBaseAddr()
        {
            return MemoryRegions.MapRegionAddress;
        }

        private ulong GetMapRegionSize()
        {
            return MemoryRegions.MapRegionSize;
        }

        private ulong GetHeapRegionBaseAddr()
        {
            return MemoryRegions.HeapRegionAddress;
        }

        private ulong GetHeapRegionSize()
        {
            return MemoryRegions.HeapRegionSize;
        }

        private ulong GetTotalMem()
        {
            return (ulong)Memory.Manager.GetTotalMemorySize();
        }

        private ulong GetUsedMem()
        {
            return (ulong)Memory.Manager.GetUsedMemorySize();
        }

        private ulong IsCurrentProcessBeingDebugged()
        {
            return (ulong)0;
        }

        private ulong GetRnd64()
        {
            return (ulong)Rng.Next() + ((ulong)Rng.Next() << 32);
        }

        private ulong GetAddrSpaceBaseAddr()
        {
            return 0x08000000;
        }

        private ulong GetAddrSpaceSize()
        {
            return AMemoryMgr.AddrSize - GetAddrSpaceBaseAddr();
        }
    }
}
