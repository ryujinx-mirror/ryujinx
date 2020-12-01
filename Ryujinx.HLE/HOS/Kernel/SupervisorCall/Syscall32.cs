using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    class Syscall32
    {
        private readonly Syscall _syscall;

        public Syscall32(Syscall syscall)
        {
            _syscall = syscall;
        }

        // IPC

        public KernelResult ConnectToNamedPort32([R(1)] uint namePtr, [R(1)] out int handle)
        {
            return _syscall.ConnectToNamedPort(namePtr, out handle);
        }

        public KernelResult SendSyncRequest32([R(0)] int handle)
        {
            return _syscall.SendSyncRequest(handle);
        }

        public KernelResult SendSyncRequestWithUserBuffer32([R(0)] uint messagePtr, [R(1)] uint messageSize, [R(2)] int handle)
        {
            return _syscall.SendSyncRequestWithUserBuffer(messagePtr, messageSize, handle);
        }

        public KernelResult CreateSession32(
            [R(2)] bool isLight,
            [R(3)] uint namePtr,
            [R(1)] out int serverSessionHandle,
            [R(2)] out int clientSessionHandle)
        {
            return _syscall.CreateSession(isLight, namePtr, out serverSessionHandle, out clientSessionHandle);
        }

        public KernelResult AcceptSession32([R(1)] int portHandle, [R(1)] out int sessionHandle)
        {
            return _syscall.AcceptSession(portHandle, out sessionHandle);
        }

        public KernelResult ReplyAndReceive32(
            [R(0)] uint timeoutLow,
            [R(1)] ulong handlesPtr,
            [R(2)] int handlesCount,
            [R(3)] int replyTargetHandle,
            [R(4)] uint timeoutHigh,
            [R(1)] out int handleIndex)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return _syscall.ReplyAndReceive(handlesPtr, handlesCount, replyTargetHandle, timeout, out handleIndex);
        }

        public KernelResult CreatePort32(
            [R(0)] uint namePtr,
            [R(2)] int maxSessions,
            [R(3)] bool isLight,
            [R(1)] out int serverPortHandle,
            [R(2)] out int clientPortHandle)
        {
            return _syscall.CreatePort(maxSessions, isLight, namePtr, out serverPortHandle, out clientPortHandle);
        }

        public KernelResult ManageNamedPort32([R(1)] uint namePtr, [R(2)] int maxSessions, [R(1)] out int handle)
        {
            return _syscall.ManageNamedPort(namePtr, maxSessions, out handle);
        }

        public KernelResult ConnectToPort32([R(1)] int clientPortHandle, [R(1)] out int clientSessionHandle)
        {
            return _syscall.ConnectToPort(clientPortHandle, out clientSessionHandle);
        }

        // Memory

        public KernelResult SetHeapSize32([R(1)] uint size, [R(1)] out uint position)
        {
            KernelResult result = _syscall.SetHeapSize(size, out ulong temporaryPosition);

            position = (uint)temporaryPosition;

            return result;
        }

        public KernelResult SetMemoryAttribute32(
            [R(0)] uint position,
            [R(1)] uint size,
            [R(2)] MemoryAttribute attributeMask,
            [R(3)] MemoryAttribute attributeValue)
        {
            return _syscall.SetMemoryAttribute(position, size, attributeMask, attributeValue);
        }

        public KernelResult MapMemory32([R(0)] uint dst, [R(1)] uint src, [R(2)] uint size)
        {
            return _syscall.MapMemory(dst, src, size);
        }

        public KernelResult UnmapMemory32([R(0)] uint dst, [R(1)] uint src, [R(2)] uint size)
        {
            return _syscall.UnmapMemory(dst, src, size);
        }

        public KernelResult QueryMemory32([R(0)] uint infoPtr, [R(1)] uint r1, [R(2)] uint position, [R(1)] out uint pageInfo)
        {
            KernelResult result = _syscall.QueryMemory(infoPtr, position, out ulong pageInfo64);

            pageInfo = (uint)pageInfo64;

            return result;
        }

        public KernelResult MapSharedMemory32([R(0)] int handle, [R(1)] uint address, [R(2)] uint size, [R(3)] KMemoryPermission permission)
        {
            return _syscall.MapSharedMemory(handle, address, size, permission);
        }

        public KernelResult UnmapSharedMemory32([R(0)] int handle, [R(1)] uint address, [R(2)] uint size)
        {
            return _syscall.UnmapSharedMemory(handle, address, size);
        }

        public KernelResult CreateTransferMemory32(
            [R(1)] uint address,
            [R(2)] uint size,
            [R(3)] KMemoryPermission permission,
            [R(1)] out int handle)
        {
            return _syscall.CreateTransferMemory(address, size, permission, out handle);
        }

        public KernelResult MapPhysicalMemory32([R(0)] uint address, [R(1)] uint size)
        {
            return _syscall.MapPhysicalMemory(address, size);
        }

        public KernelResult UnmapPhysicalMemory32([R(0)] uint address, [R(1)] uint size)
        {
            return _syscall.UnmapPhysicalMemory(address, size);
        }

        public KernelResult MapProcessCodeMemory32([R(0)] int handle, [R(1)] uint srcLow, [R(2)] uint dstLow, [R(3)] uint dstHigh, [R(4)] uint srcHigh, [R(5)] uint sizeLow, [R(6)] uint sizeHigh)
        {
            ulong src = srcLow | ((ulong)srcHigh << 32);
            ulong dst = dstLow | ((ulong)dstHigh << 32);
            ulong size = sizeLow | ((ulong)sizeHigh << 32);

            return _syscall.MapProcessCodeMemory(handle, dst, src, size);
        }

        public KernelResult UnmapProcessCodeMemory32([R(0)] int handle, [R(1)] uint srcLow, [R(2)] uint dstLow, [R(3)] uint dstHigh, [R(4)] uint srcHigh, [R(5)] uint sizeLow, [R(6)] uint sizeHigh)
        {
            ulong src = srcLow | ((ulong)srcHigh << 32);
            ulong dst = dstLow | ((ulong)dstHigh << 32);
            ulong size = sizeLow | ((ulong)sizeHigh << 32);

            return _syscall.UnmapProcessCodeMemory(handle, dst, src, size);
        }

        public KernelResult SetProcessMemoryPermission32(
            [R(0)] int handle,
            [R(1)] uint sizeLow,
            [R(2)] uint srcLow,
            [R(3)] uint srcHigh,
            [R(4)] uint sizeHigh,
            [R(5)] KMemoryPermission permission)
        {
            ulong src = srcLow | ((ulong)srcHigh << 32);
            ulong size = sizeLow | ((ulong)sizeHigh << 32);

            return _syscall.SetProcessMemoryPermission(handle, src, size, permission);
        }

        // System

        public void ExitProcess32()
        {
            _syscall.ExitProcess();
        }

        public KernelResult TerminateProcess32([R(0)] int handle)
        {
            return _syscall.TerminateProcess(handle);
        }

        public KernelResult SignalEvent32([R(0)] int handle)
        {
            return _syscall.SignalEvent(handle);
        }

        public KernelResult ClearEvent32([R(0)] int handle)
        {
            return _syscall.ClearEvent(handle);
        }

        public KernelResult CloseHandle32([R(0)] int handle)
        {
            return _syscall.CloseHandle(handle);
        }

        public KernelResult ResetSignal32([R(0)] int handle)
        {
            return _syscall.ResetSignal(handle);
        }

        public void GetSystemTick32([R(0)] out uint resultLow, [R(1)] out uint resultHigh)
        {
            ulong result = _syscall.GetSystemTick();

            resultLow = (uint)(result & uint.MaxValue);
            resultHigh = (uint)(result >> 32);
        }

        public KernelResult GetProcessId32([R(1)] int handle, [R(1)] out int pidLow, [R(2)] out int pidHigh)
        {
            KernelResult result = _syscall.GetProcessId(handle, out long pid);

            pidLow = (int)(pid & uint.MaxValue);
            pidHigh = (int)(pid >> 32);

            return result;
        }

        public void Break32([R(0)] uint reason, [R(1)] uint r1, [R(2)] uint info)
        {
            _syscall.Break(reason);
        }

        public void OutputDebugString32([R(0)] uint strPtr, [R(1)] uint size)
        {
            _syscall.OutputDebugString(strPtr, size);
        }

        public KernelResult GetInfo32(
            [R(0)] uint subIdLow,
            [R(1)] uint id,
            [R(2)] int handle,
            [R(3)] uint subIdHigh,
            [R(1)] out uint valueLow,
            [R(2)] out uint valueHigh)
        {
            long subId = (long)(subIdLow | ((ulong)subIdHigh << 32));

            KernelResult result = _syscall.GetInfo(id, handle, subId, out long value);

            valueHigh = (uint)(value >> 32);
            valueLow = (uint)(value & uint.MaxValue);

            return result;
        }

        public KernelResult CreateEvent32([R(1)] out int wEventHandle, [R(2)] out int rEventHandle)
        {
            return _syscall.CreateEvent(out wEventHandle, out rEventHandle);
        }

        public KernelResult GetProcessList32([R(1)] ulong address, [R(2)] int maxCount, [R(1)] out int count)
        {
            return _syscall.GetProcessList(address, maxCount, out count);
        }

        public KernelResult GetSystemInfo32([R(1)] uint subIdLow, [R(2)] uint id, [R(3)] int handle, [R(3)] uint subIdHigh, [R(1)] out int valueLow, [R(2)] out int valueHigh)
        {
            long subId = (long)(subIdLow | ((ulong)subIdHigh << 32));

            KernelResult result = _syscall.GetSystemInfo(id, handle, subId, out long value);

            valueHigh = (int)(value >> 32);
            valueLow = (int)(value & uint.MaxValue);

            return result;
        }

        public KernelResult FlushProcessDataCache32(
            [R(0)] uint processHandle,
            [R(2)] uint addressLow,
            [R(3)] uint addressHigh,
            [R(1)] uint sizeLow,
            [R(4)] uint sizeHigh)
        {
            // FIXME: This needs to be implemented as ARMv7 doesn't have any way to do cache maintenance operations on EL0.
            // As we don't support (and don't actually need) to flush the cache, this is stubbed.
            return KernelResult.Success;
        }

        // Thread

        public KernelResult CreateThread32(
            [R(1)] uint entrypoint,
            [R(2)] uint argsPtr,
            [R(3)] uint stackTop,
            [R(0)] int priority,
            [R(4)] int cpuCore,
            [R(1)] out int handle)
        {
            return _syscall.CreateThread(entrypoint, argsPtr, stackTop, priority, cpuCore, out handle);
        }

        public KernelResult StartThread32([R(0)] int handle)
        {
            return _syscall.StartThread(handle);
        }

        public void ExitThread32()
        {
            _syscall.ExitThread();
        }

        public void SleepThread32([R(0)] uint timeoutLow, [R(1)] uint timeoutHigh)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            _syscall.SleepThread(timeout);
        }

        public KernelResult GetThreadPriority32([R(1)] int handle, [R(1)] out int priority)
        {
            return _syscall.GetThreadPriority(handle, out priority);
        }

        public KernelResult SetThreadPriority32([R(0)] int handle, [R(1)] int priority)
        {
            return _syscall.SetThreadPriority(handle, priority);
        }

        public KernelResult GetThreadCoreMask32([R(2)] int handle, [R(1)] out int preferredCore, [R(2)] out int affinityMaskLow, [R(3)] out int affinityMaskHigh)
        {
            KernelResult result = _syscall.GetThreadCoreMask(handle, out preferredCore, out long affinityMask);

            affinityMaskLow = (int)(affinityMask >> 32);
            affinityMaskHigh = (int)(affinityMask & uint.MaxValue);

            return result;
        }

        public KernelResult SetThreadCoreMask32([R(0)] int handle, [R(1)] int preferredCore, [R(2)] uint affinityMaskLow, [R(3)] uint affinityMaskHigh)
        {
            long affinityMask = (long)(affinityMaskLow | ((ulong)affinityMaskHigh << 32));

            return _syscall.SetThreadCoreMask(handle, preferredCore, affinityMask);
        }

        public int GetCurrentProcessorNumber32()
        {
            return _syscall.GetCurrentProcessorNumber();
        }

        public KernelResult GetThreadId32([R(1)] int handle, [R(1)] out uint threadUidLow, [R(2)] out uint threadUidHigh)
        {
            long threadUid;

            KernelResult result = _syscall.GetThreadId(handle, out threadUid);

            threadUidLow = (uint)(threadUid >> 32);
            threadUidHigh = (uint)(threadUid & uint.MaxValue);

            return result;
        }

        public KernelResult SetThreadActivity32([R(0)] int handle, [R(1)] bool pause)
        {
            return _syscall.SetThreadActivity(handle, pause);
        }

        public KernelResult GetThreadContext332([R(0)] uint address, [R(1)] int handle)
        {
            return _syscall.GetThreadContext3(address, handle);
        }

        // Thread synchronization

        public KernelResult WaitSynchronization32(
            [R(0)] uint timeoutLow,
            [R(1)] uint handlesPtr,
            [R(2)] int handlesCount,
            [R(3)] uint timeoutHigh,
            [R(1)] out int handleIndex)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return _syscall.WaitSynchronization(handlesPtr, handlesCount, timeout, out handleIndex);
        }

        public KernelResult CancelSynchronization32([R(0)] int handle)
        {
            return _syscall.CancelSynchronization(handle);
        }


        public KernelResult ArbitrateLock32([R(0)] int ownerHandle, [R(1)] uint mutexAddress, [R(2)] int requesterHandle)
        {
            return _syscall.ArbitrateLock(ownerHandle, mutexAddress, requesterHandle);
        }

        public KernelResult ArbitrateUnlock32([R(0)] uint mutexAddress)
        {
            return _syscall.ArbitrateUnlock(mutexAddress);
        }

        public KernelResult WaitProcessWideKeyAtomic32(
            [R(0)] uint mutexAddress,
            [R(1)] uint condVarAddress,
            [R(2)] int handle,
            [R(3)] uint timeoutLow,
            [R(4)] uint timeoutHigh)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return _syscall.WaitProcessWideKeyAtomic(mutexAddress, condVarAddress, handle, timeout);
        }

        public KernelResult SignalProcessWideKey32([R(0)] uint address, [R(1)] int count)
        {
            return _syscall.SignalProcessWideKey(address, count);
        }

        public KernelResult WaitForAddress32([R(0)] uint address, [R(1)] ArbitrationType type, [R(2)] int value, [R(3)] uint timeoutLow, [R(4)] uint timeoutHigh)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return _syscall.WaitForAddress(address, type, value, timeout);
        }

        public KernelResult SignalToAddress32([R(0)] uint address, [R(1)] SignalType type, [R(2)] int value, [R(3)] int count)
        {
            return _syscall.SignalToAddress(address, type, value, count);
        }
    }
}
