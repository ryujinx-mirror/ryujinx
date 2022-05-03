using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    class Syscall64
    {
        private readonly Syscall _syscall;

        public Syscall64(Syscall syscall)
        {
            _syscall = syscall;
        }

        // IPC

        public KernelResult ConnectToNamedPort64([R(1)] ulong namePtr, [R(1)] out int handle)
        {
            return _syscall.ConnectToNamedPort(out handle, namePtr);
        }

        public KernelResult SendSyncRequest64([R(0)] int handle)
        {
            return _syscall.SendSyncRequest(handle);
        }

        public KernelResult SendSyncRequestWithUserBuffer64([R(0)] ulong messagePtr, [R(1)] ulong messageSize, [R(2)] int handle)
        {
            return _syscall.SendSyncRequestWithUserBuffer(messagePtr, messageSize, handle);
        }

        public KernelResult SendAsyncRequestWithUserBuffer64(
            [R(1)] ulong messagePtr,
            [R(2)] ulong messageSize,
            [R(3)] int handle,
            [R(1)] out int doneEventHandle)
        {
            return _syscall.SendAsyncRequestWithUserBuffer(out doneEventHandle, messagePtr, messageSize, handle);
        }

        public KernelResult CreateSession64(
            [R(2)] bool isLight,
            [R(3)] ulong namePtr,
            [R(1)] out int serverSessionHandle,
            [R(2)] out int clientSessionHandle)
        {
            return _syscall.CreateSession(out serverSessionHandle, out clientSessionHandle, isLight, namePtr);
        }

        public KernelResult AcceptSession64([R(1)] int portHandle, [R(1)] out int sessionHandle)
        {
            return _syscall.AcceptSession(out sessionHandle, portHandle);
        }

        public KernelResult ReplyAndReceive64(
            [R(1)] ulong handlesPtr,
            [R(2)] int handlesCount,
            [R(3)] int replyTargetHandle,
            [R(4)] long timeout,
            [R(1)] out int handleIndex)
        {
            return _syscall.ReplyAndReceive(out handleIndex, handlesPtr, handlesCount, replyTargetHandle, timeout);
        }

        public KernelResult ReplyAndReceiveWithUserBuffer64(
            [R(1)] ulong messagePtr,
            [R(2)] ulong messageSize,
            [R(3)] ulong handlesPtr,
            [R(4)] int handlesCount,
            [R(5)] int replyTargetHandle,
            [R(6)] long timeout,
            [R(1)] out int handleIndex)
        {
            return _syscall.ReplyAndReceiveWithUserBuffer(
                out handleIndex,
                handlesPtr,
                messagePtr,
                messageSize,
                handlesCount,
                replyTargetHandle,
                timeout);
        }

        public KernelResult CreatePort64(
            [R(2)] int maxSessions,
            [R(3)] bool isLight,
            [R(4)] ulong namePtr,
            [R(1)] out int serverPortHandle,
            [R(2)] out int clientPortHandle)
        {
            return _syscall.CreatePort(out serverPortHandle, out clientPortHandle, maxSessions, isLight, namePtr);
        }

        public KernelResult ManageNamedPort64([R(1)] ulong namePtr, [R(2)] int maxSessions, [R(1)] out int handle)
        {
            return _syscall.ManageNamedPort(out handle, namePtr, maxSessions);
        }

        public KernelResult ConnectToPort64([R(1)] int clientPortHandle, [R(1)] out int clientSessionHandle)
        {
            return _syscall.ConnectToPort(out clientSessionHandle, clientPortHandle);
        }

        // Memory

        public KernelResult SetHeapSize64([R(1)] ulong size, [R(1)] out ulong address)
        {
            return _syscall.SetHeapSize(out address, size);
        }

        public KernelResult SetMemoryPermission64(
            [R(0)] ulong address,
            [R(1)] ulong size,
            [R(2)] KMemoryPermission permission)
        {
            return _syscall.SetMemoryPermission(address, size, permission);
        }

        public KernelResult SetMemoryAttribute64(
            [R(0)] ulong address,
            [R(1)] ulong size,
            [R(2)] MemoryAttribute attributeMask,
            [R(3)] MemoryAttribute attributeValue)
        {
            return _syscall.SetMemoryAttribute(address, size, attributeMask, attributeValue);
        }

        public KernelResult MapMemory64([R(0)] ulong dst, [R(1)] ulong src, [R(2)] ulong size)
        {
            return _syscall.MapMemory(dst, src, size);
        }

        public KernelResult UnmapMemory64([R(0)] ulong dst, [R(1)] ulong src, [R(2)] ulong size)
        {
            return _syscall.UnmapMemory(dst, src, size);
        }

        public KernelResult QueryMemory64([R(0)] ulong infoPtr, [R(2)] ulong address, [R(1)] out ulong pageInfo)
        {
            return _syscall.QueryMemory(infoPtr, out pageInfo, address);
        }

        public KernelResult MapSharedMemory64([R(0)] int handle, [R(1)] ulong address, [R(2)] ulong size, [R(3)] KMemoryPermission permission)
        {
            return _syscall.MapSharedMemory(handle, address, size, permission);
        }

        public KernelResult UnmapSharedMemory64([R(0)] int handle, [R(1)] ulong address, [R(2)] ulong size)
        {
            return _syscall.UnmapSharedMemory(handle, address, size);
        }

        public KernelResult CreateTransferMemory64(
            [R(1)] ulong address,
            [R(2)] ulong size,
            [R(3)] KMemoryPermission permission,
            [R(1)] out int handle)
        {
            return _syscall.CreateTransferMemory(out handle, address, size, permission);
        }

        public KernelResult CreateCodeMemory64([R(1)] ulong address, [R(2)] ulong size, [R(1)] out int handle)
        {
            return _syscall.CreateCodeMemory(address, size, out handle);
        }

        public KernelResult ControlCodeMemory64([R(0)] int handle, [R(1)] CodeMemoryOperation op, [R(2)] ulong address, [R(3)] ulong size, [R(4)] KMemoryPermission permission)
        {
            return _syscall.ControlCodeMemory(handle, op, address, size, permission);
        }

        public KernelResult MapTransferMemory64([R(0)] int handle, [R(1)] ulong address, [R(2)] ulong size, [R(3)] KMemoryPermission permission)
        {
            return _syscall.MapTransferMemory(handle, address, size, permission);
        }

        public KernelResult UnmapTransferMemory64([R(0)] int handle, [R(1)] ulong address, [R(2)] ulong size)
        {
            return _syscall.UnmapTransferMemory(handle, address, size);
        }

        public KernelResult MapPhysicalMemory64([R(0)] ulong address, [R(1)] ulong size)
        {
            return _syscall.MapPhysicalMemory(address, size);
        }

        public KernelResult UnmapPhysicalMemory64([R(0)] ulong address, [R(1)] ulong size)
        {
            return _syscall.UnmapPhysicalMemory(address, size);
        }

        public KernelResult SetProcessMemoryPermission64([R(0)] int handle, [R(1)] ulong src, [R(2)] ulong size, [R(3)] KMemoryPermission permission)
        {
            return _syscall.SetProcessMemoryPermission(handle, src, size, permission);
        }

        public KernelResult MapProcessMemory64([R(0)] ulong dst, [R(1)] int handle, [R(2)] ulong src, [R(3)] ulong size)
        {
            return _syscall.MapProcessMemory(dst, handle, src, size);
        }

        public KernelResult UnmapProcessMemory64([R(0)] ulong dst, [R(1)] int handle, [R(2)] ulong src, [R(3)] ulong size)
        {
            return _syscall.UnmapProcessMemory(dst, handle, src, size);
        }

        public KernelResult MapProcessCodeMemory64([R(0)] int handle, [R(1)] ulong dst, [R(2)] ulong src, [R(3)] ulong size)
        {
            return _syscall.MapProcessCodeMemory(handle, dst, src, size);
        }

        public KernelResult UnmapProcessCodeMemory64([R(0)] int handle, [R(1)] ulong dst, [R(2)] ulong src, [R(3)] ulong size)
        {
            return _syscall.UnmapProcessCodeMemory(handle, dst, src, size);
        }

        // System

        public void ExitProcess64()
        {
            _syscall.ExitProcess();
        }

        public KernelResult TerminateProcess64([R(0)] int handle)
        {
            return _syscall.TerminateProcess(handle);
        }

        public KernelResult SignalEvent64([R(0)] int handle)
        {
            return _syscall.SignalEvent(handle);
        }

        public KernelResult ClearEvent64([R(0)] int handle)
        {
            return _syscall.ClearEvent(handle);
        }

        public KernelResult CloseHandle64([R(0)] int handle)
        {
            return _syscall.CloseHandle(handle);
        }

        public KernelResult ResetSignal64([R(0)] int handle)
        {
            return _syscall.ResetSignal(handle);
        }

        public ulong GetSystemTick64()
        {
            return _syscall.GetSystemTick();
        }

        public KernelResult GetProcessId64([R(1)] int handle, [R(1)] out ulong pid)
        {
            return _syscall.GetProcessId(out pid, handle);
        }

        public void Break64([R(0)] ulong reason, [R(1)] ulong x1, [R(2)] ulong info)
        {
            _syscall.Break(reason);
        }

        public void OutputDebugString64([R(0)] ulong strPtr, [R(1)] ulong size)
        {
            _syscall.OutputDebugString(strPtr, size);
        }

        public KernelResult GetInfo64([R(1)] InfoType id, [R(2)] int handle, [R(3)] long subId, [R(1)] out ulong value)
        {
            return _syscall.GetInfo(out value, id, handle, subId);
        }

        public KernelResult CreateEvent64([R(1)] out int wEventHandle, [R(2)] out int rEventHandle)
        {
            return _syscall.CreateEvent(out wEventHandle, out rEventHandle);
        }

        public KernelResult GetProcessList64([R(1)] ulong address, [R(2)] int maxCount, [R(1)] out int count)
        {
            return _syscall.GetProcessList(out count, address, maxCount);
        }

        public KernelResult GetSystemInfo64([R(1)] uint id, [R(2)] int handle, [R(3)] long subId, [R(1)] out long value)
        {
            return _syscall.GetSystemInfo(out value, id, handle, subId);
        }

        public KernelResult GetResourceLimitLimitValue64([R(1)] int handle, [R(2)] LimitableResource resource, [R(1)] out long limitValue)
        {
            return _syscall.GetResourceLimitLimitValue(out limitValue, handle, resource);
        }

        public KernelResult GetResourceLimitCurrentValue64([R(1)] int handle, [R(2)] LimitableResource resource, [R(1)] out long limitValue)
        {
            return _syscall.GetResourceLimitCurrentValue(out limitValue, handle, resource);
        }

        public KernelResult GetResourceLimitPeakValue64([R(1)] int handle, [R(2)] LimitableResource resource, [R(1)] out long peak)
        {
            return _syscall.GetResourceLimitPeakValue(out peak, handle, resource);
        }

        public KernelResult CreateResourceLimit64([R(1)] out int handle)
        {
            return _syscall.CreateResourceLimit(out handle);
        }

        public KernelResult SetResourceLimitLimitValue64([R(0)] int handle, [R(1)] LimitableResource resource, [R(2)] long limitValue)
        {
            return _syscall.SetResourceLimitLimitValue(handle, resource, limitValue);
        }

        // Thread

        public KernelResult CreateThread64(
            [R(1)] ulong entrypoint,
            [R(2)] ulong argsPtr,
            [R(3)] ulong stackTop,
            [R(4)] int priority,
            [R(5)] int cpuCore,
            [R(1)] out int handle)
        {
            return _syscall.CreateThread(out handle, entrypoint, argsPtr, stackTop, priority, cpuCore);
        }

        public KernelResult StartThread64([R(0)] int handle)
        {
            return _syscall.StartThread(handle);
        }

        public void ExitThread64()
        {
            _syscall.ExitThread();
        }

        public void SleepThread64([R(0)] long timeout)
        {
            _syscall.SleepThread(timeout);
        }

        public KernelResult GetThreadPriority64([R(1)] int handle, [R(1)] out int priority)
        {
            return _syscall.GetThreadPriority(out priority, handle);
        }

        public KernelResult SetThreadPriority64([R(0)] int handle, [R(1)] int priority)
        {
            return _syscall.SetThreadPriority(handle, priority);
        }

        public KernelResult GetThreadCoreMask64([R(2)] int handle, [R(1)] out int preferredCore, [R(2)] out ulong affinityMask)
        {
            return _syscall.GetThreadCoreMask(out preferredCore, out affinityMask, handle);
        }

        public KernelResult SetThreadCoreMask64([R(0)] int handle, [R(1)] int preferredCore, [R(2)] ulong affinityMask)
        {
            return _syscall.SetThreadCoreMask(handle, preferredCore, affinityMask);
        }

        public int GetCurrentProcessorNumber64()
        {
            return _syscall.GetCurrentProcessorNumber();
        }

        public KernelResult GetThreadId64([R(1)] int handle, [R(1)] out ulong threadUid)
        {
            return _syscall.GetThreadId(out threadUid, handle);
        }

        public KernelResult SetThreadActivity64([R(0)] int handle, [R(1)] bool pause)
        {
            return _syscall.SetThreadActivity(handle, pause);
        }

        public KernelResult GetThreadContext364([R(0)] ulong address, [R(1)] int handle)
        {
            return _syscall.GetThreadContext3(address, handle);
        }

        // Thread synchronization

        public KernelResult WaitSynchronization64([R(1)] ulong handlesPtr, [R(2)] int handlesCount, [R(3)] long timeout, [R(1)] out int handleIndex)
        {
            return _syscall.WaitSynchronization(out handleIndex, handlesPtr, handlesCount, timeout);
        }

        public KernelResult CancelSynchronization64([R(0)] int handle)
        {
            return _syscall.CancelSynchronization(handle);
        }

        public KernelResult ArbitrateLock64([R(0)] int ownerHandle, [R(1)] ulong mutexAddress, [R(2)] int requesterHandle)
        {
            return _syscall.ArbitrateLock(ownerHandle, mutexAddress, requesterHandle);
        }

        public KernelResult ArbitrateUnlock64([R(0)] ulong mutexAddress)
        {
            return _syscall.ArbitrateUnlock(mutexAddress);
        }

        public KernelResult WaitProcessWideKeyAtomic64(
            [R(0)] ulong mutexAddress,
            [R(1)] ulong condVarAddress,
            [R(2)] int handle,
            [R(3)] long timeout)
        {
            return _syscall.WaitProcessWideKeyAtomic(mutexAddress, condVarAddress, handle, timeout);
        }

        public KernelResult SignalProcessWideKey64([R(0)] ulong address, [R(1)] int count)
        {
            return _syscall.SignalProcessWideKey(address, count);
        }

        public KernelResult WaitForAddress64([R(0)] ulong address, [R(1)] ArbitrationType type, [R(2)] int value, [R(3)] long timeout)
        {
            return _syscall.WaitForAddress(address, type, value, timeout);
        }

        public KernelResult SignalToAddress64([R(0)] ulong address, [R(1)] SignalType type, [R(2)] int value, [R(3)] int count)
        {
            return _syscall.SignalToAddress(address, type, value, count);
        }

        public KernelResult SynchronizePreemptionState64()
        {
            return _syscall.SynchronizePreemptionState();
        }
    }
}
