using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.OsHle.Exceptions;
using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;
using System;

namespace Ryujinx.OsHle.Svc
{
    partial class SvcHandler
    {
        private void SvcExitProcess(ARegisters Registers) => Ns.Os.ExitProcess(Registers.ProcessId);

        private void SvcCloseHandle(ARegisters Registers)
        {
            int Handle = (int)Registers.X0;

            Ns.Os.CloseHandle(Handle);

            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcResetSignal(ARegisters Registers)
        {
            int Handle = (int)Registers.X0;

            //TODO: Implement events.

            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcWaitSynchronization(ARegisters Registers)
        {
            long HandlesPtr   = (long)Registers.X0;
            int  HandlesCount =  (int)Registers.X2;
            long Timeout      = (long)Registers.X3;

            //TODO: Implement events.

            //Logging.Info($"SvcWaitSynchronization Thread {Registers.ThreadId}");

            if (Process.TryGetThread(Registers.Tpidr, out HThread Thread))
            {
                Process.Scheduler.Yield(Thread);
            }
            else
            {
                Logging.Error($"Thread with TPIDR_EL0 0x{Registers.Tpidr:x16} not found!");
            }

            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcGetSystemTick(ARegisters Registers)
        {
            Registers.X0 = (ulong)Registers.CntpctEl0;
        }

        private void SvcConnectToNamedPort(ARegisters Registers)
        {
            long StackPtr = (long)Registers.X0;
            long NamePtr  = (long)Registers.X1;

            string Name = AMemoryHelper.ReadAsciiString(Memory, NamePtr, 8);

            //TODO: Validate that app has perms to access the service, and that the service
            //actually exists, return error codes otherwise.

            HSession Session = new HSession(Name);

            Registers.X1 = (ulong)Ns.Os.Handles.GenerateId(Session);
            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcSendSyncRequest(ARegisters Registers)
        {
            SendSyncRequest(Registers, false);
        }

        private void SvcSendSyncRequestWithUserBuffer(ARegisters Registers)
        {
            SendSyncRequest(Registers, true);
        }

        private void SendSyncRequest(ARegisters Registers, bool UserBuffer)
        {
            long CmdPtr = Registers.Tpidr;
            long Size   = 0x100;
            int  Handle = 0;

            if (UserBuffer)
            {
                CmdPtr = (long)Registers.X0;
                Size   = (long)Registers.X1;
                Handle =  (int)Registers.X2;
            }
            else
            {
                Handle = (int)Registers.X0;
            }

            byte[] CmdData = AMemoryHelper.ReadBytes(Memory, CmdPtr, (int)Size);

            HSession Session = Ns.Os.Handles.GetData<HSession>(Handle);

            IpcMessage Cmd = new IpcMessage(CmdData, CmdPtr, Session is HDomain);

            if (Session != null)
            {
                IpcHandler.IpcCall(Ns, Memory, Session, Cmd, CmdPtr, Handle);

                byte[] Response = AMemoryHelper.ReadBytes(Memory, CmdPtr, (int)Size);

                Registers.X0 = (int)SvcResult.Success;
            }
            else
            {
                Registers.X0 = (int)SvcResult.ErrBadIpcReq;
            }
        }

        private void SvcBreak(ARegisters Registers)
        {
            long Reason  = (long)Registers.X0;
            long Unknown = (long)Registers.X1;
            long Info    = (long)Registers.X2;

            throw new GuestBrokeExecutionException();
        }

        private void SvcOutputDebugString(ARegisters Registers)
        {
            long Position = (long)Registers.X0;
            long Size     = (long)Registers.X1;

            string Str = AMemoryHelper.ReadAsciiString(Memory, Position, (int)Size);

            Logging.Info($"SvcOutputDebugString: {Str}");

            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcGetInfo(ARegisters Registers)
        {
            long StackPtr = (long)Registers.X0;
            int  InfoType =  (int)Registers.X1;
            long Handle   = (long)Registers.X2;
            int  InfoId   =  (int)Registers.X3;

            //Fail for info not available on older Kernel versions.
            if (InfoType == 18 ||
                InfoType == 19)
            {
                Registers.X0 = (int)SvcResult.ErrBadInfo;

                return;
            }

            switch (InfoType)
            {
                case 2:  Registers.X1 = GetMapRegionBaseAddr();  break;
                case 3:  Registers.X1 = GetMapRegionSize();      break;
                case 4:  Registers.X1 = GetHeapRegionBaseAddr(); break;
                case 5:  Registers.X1 = GetHeapRegionSize();     break;
                case 6:  Registers.X1 = GetTotalMem();           break;
                case 7:  Registers.X1 = GetUsedMem();            break;
                case 11: Registers.X1 = GetRnd64();              break;
                case 12: Registers.X1 = GetAddrSpaceBaseAddr();  break;
                case 13: Registers.X1 = GetAddrSpaceSize();      break;
                case 14: Registers.X1 = GetMapRegionBaseAddr();  break;
                case 15: Registers.X1 = GetMapRegionSize();      break;

                default: throw new NotImplementedException($"SvcGetInfo: {InfoType} {Handle} {InfoId}");
            }

            Registers.X0 = (int)SvcResult.Success;
        }

        private ulong GetTotalMem()
        {
            return (ulong)Memory.Manager.GetTotalMemorySize();
        }

        private ulong GetUsedMem()
        {
            return (ulong)Memory.Manager.GetUsedMemorySize();
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

        private ulong GetMapRegionBaseAddr()
        {
            return 0x80000000;
        }

        private ulong GetMapRegionSize()
        {
            return 0x40000000;
        }

        private ulong GetHeapRegionBaseAddr()
        {
            return GetMapRegionBaseAddr() + GetMapRegionSize();
        }

        private ulong GetHeapRegionSize()
        {
            return 0x40000000;
        }
    }
}