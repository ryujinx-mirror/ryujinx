using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services;
using System;
using System.Threading;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private void SvcExitProcess(CpuThreadState threadState)
        {
            _system.Scheduler.GetCurrentProcess().Terminate();
        }

        private void SignalEvent64(CpuThreadState threadState)
        {
            threadState.X0 = (ulong)SignalEvent((int)threadState.X0);
        }

        private KernelResult SignalEvent(int handle)
        {
            KWritableEvent writableEvent = _process.HandleTable.GetObject<KWritableEvent>(handle);

            KernelResult result;

            if (writableEvent != null)
            {
                writableEvent.Signal();

                result = KernelResult.Success;
            }
            else
            {
                result = KernelResult.InvalidHandle;
            }

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Operation failed with error: " + result + "!");
            }

            return result;
        }

        private void ClearEvent64(CpuThreadState threadState)
        {
            threadState.X0 = (ulong)ClearEvent((int)threadState.X0);
        }

        private KernelResult ClearEvent(int handle)
        {
            KernelResult result;

            KWritableEvent writableEvent = _process.HandleTable.GetObject<KWritableEvent>(handle);

            if (writableEvent == null)
            {
                KReadableEvent readableEvent = _process.HandleTable.GetObject<KReadableEvent>(handle);

                result = readableEvent?.Clear() ?? KernelResult.InvalidHandle;
            }
            else
            {
                result = writableEvent.Clear();
            }

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Operation failed with error: " + result + "!");
            }

            return result;
        }

        private void SvcCloseHandle(CpuThreadState threadState)
        {
            int handle = (int)threadState.X0;

            object obj = _process.HandleTable.GetObject<object>(handle);

            _process.HandleTable.CloseHandle(handle);

            if (obj == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (obj is KSession session)
            {
                session.Dispose();
            }
            else if (obj is KTransferMemory transferMemory)
            {
                _process.MemoryManager.ResetTransferMemory(
                    transferMemory.Address,
                    transferMemory.Size);
            }

            threadState.X0 = 0;
        }

        private void ResetSignal64(CpuThreadState threadState)
        {
            threadState.X0 = (ulong)ResetSignal((int)threadState.X0);
        }

        private KernelResult ResetSignal(int handle)
        {
            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KReadableEvent readableEvent = currentProcess.HandleTable.GetObject<KReadableEvent>(handle);

            KernelResult result;

            if (readableEvent != null)
            {
                result = readableEvent.ClearIfSignaled();
            }
            else
            {
                KProcess process = currentProcess.HandleTable.GetKProcess(handle);

                if (process != null)
                {
                    result = process.ClearIfNotExited();
                }
                else
                {
                    result = KernelResult.InvalidHandle;
                }
            }

            if (result == KernelResult.InvalidState)
            {
                Logger.PrintDebug(LogClass.KernelSvc, "Operation failed with error: " + result + "!");
            }
            else if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Operation failed with error: " + result + "!");
            }

            return result;
        }

        private void SvcGetSystemTick(CpuThreadState threadState)
        {
            threadState.X0 = threadState.CntpctEl0;
        }

        private void SvcConnectToNamedPort(CpuThreadState threadState)
        {
            long stackPtr = (long)threadState.X0;
            long namePtr  = (long)threadState.X1;

            string name = MemoryHelper.ReadAsciiString(_memory, namePtr, 8);

            //TODO: Validate that app has perms to access the service, and that the service
            //actually exists, return error codes otherwise.
            KSession session = new KSession(ServiceFactory.MakeService(_system, name), name);

            if (_process.HandleTable.GenerateHandle(session, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            threadState.X0 = 0;
            threadState.X1 = (uint)handle;
        }

        private void SvcSendSyncRequest(CpuThreadState threadState)
        {
            SendSyncRequest(threadState, threadState.Tpidr, 0x100, (int)threadState.X0);
        }

        private void SvcSendSyncRequestWithUserBuffer(CpuThreadState threadState)
        {
            SendSyncRequest(
                      threadState,
                (long)threadState.X0,
                (long)threadState.X1,
                 (int)threadState.X2);
        }

        private void SendSyncRequest(CpuThreadState threadState, long messagePtr, long size, int handle)
        {
            byte[] messageData = _memory.ReadBytes(messagePtr, size);

            KSession session = _process.HandleTable.GetObject<KSession>(handle);

            if (session != null)
            {
                _system.CriticalSection.Enter();

                KThread currentThread = _system.Scheduler.GetCurrentThread();

                currentThread.SignaledObj   = null;
                currentThread.ObjSyncResult = 0;

                currentThread.Reschedule(ThreadSchedState.Paused);

                IpcMessage message = new IpcMessage(messageData, messagePtr);

                ThreadPool.QueueUserWorkItem(ProcessIpcRequest, new HleIpcMessage(
                    currentThread,
                    session,
                    message,
                    messagePtr));

                _system.ThreadCounter.AddCount();

                _system.CriticalSection.Leave();

                threadState.X0 = (ulong)currentThread.ObjSyncResult;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid session handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void ProcessIpcRequest(object state)
        {
            HleIpcMessage ipcMessage = (HleIpcMessage)state;

            ipcMessage.Thread.ObjSyncResult = (int)IpcHandler.IpcCall(
                _device,
                _process,
                _memory,
                ipcMessage.Session,
                ipcMessage.Message,
                ipcMessage.MessagePtr);

            _system.ThreadCounter.Signal();

            ipcMessage.Thread.Reschedule(ThreadSchedState.Running);
        }

        private void GetProcessId64(CpuThreadState threadState)
        {
            int handle = (int)threadState.X1;

            KernelResult result = GetProcessId(handle, out long pid);

            threadState.X0 = (ulong)result;
            threadState.X1 = (ulong)pid;
        }

        private KernelResult GetProcessId(int handle, out long pid)
        {
            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KProcess process = currentProcess.HandleTable.GetKProcess(handle);

            if (process == null)
            {
                KThread thread = currentProcess.HandleTable.GetKThread(handle);

                if (thread != null)
                {
                    process = thread.Owner;
                }

                //TODO: KDebugEvent.
            }

            pid = process?.Pid ?? 0;

            return process != null
                ? KernelResult.Success
                : KernelResult.InvalidHandle;
        }

        private void SvcBreak(CpuThreadState threadState)
        {
            long reason  = (long)threadState.X0;
            long unknown = (long)threadState.X1;
            long info    = (long)threadState.X2;

            KThread currentThread = _system.Scheduler.GetCurrentThread();

            if ((reason & (1 << 31)) == 0)
            {
                currentThread.PrintGuestStackTrace();

                throw new GuestBrokeExecutionException();
            }
            else
            {
                Logger.PrintInfo(LogClass.KernelSvc, "Debugger triggered.");

                currentThread.PrintGuestStackTrace();
            }
        }

        private void SvcOutputDebugString(CpuThreadState threadState)
        {
            long position = (long)threadState.X0;
            long size     = (long)threadState.X1;

            string str = MemoryHelper.ReadAsciiString(_memory, position, size);

            Logger.PrintWarning(LogClass.KernelSvc, str);

            threadState.X0 = 0;
        }

        private void GetInfo64(CpuThreadState threadState)
        {
            long stackPtr = (long)threadState.X0;
            uint id       = (uint)threadState.X1;
            int  handle   =  (int)threadState.X2;
            long subId    = (long)threadState.X3;

            KernelResult result = GetInfo(id, handle, subId, out long value);

            threadState.X0 = (ulong)result;
            threadState.X1 = (ulong)value;
        }

        private KernelResult GetInfo(uint id, int handle, long subId, out long value)
        {
            value = 0;

            switch (id)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 20:
                case 21:
                case 22:
                {
                    if (subId != 0)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

                    KProcess process = currentProcess.HandleTable.GetKProcess(handle);

                    if (process == null)
                    {
                        return KernelResult.InvalidHandle;
                    }

                    switch (id)
                    {
                        case 0: value = process.Capabilities.AllowedCpuCoresMask;    break;
                        case 1: value = process.Capabilities.AllowedThreadPriosMask; break;

                        case 2: value = (long)process.MemoryManager.AliasRegionStart; break;
                        case 3: value = (long)(process.MemoryManager.AliasRegionEnd -
                                               process.MemoryManager.AliasRegionStart); break;

                        case 4: value = (long)process.MemoryManager.HeapRegionStart; break;
                        case 5: value = (long)(process.MemoryManager.HeapRegionEnd -
                                               process.MemoryManager.HeapRegionStart); break;

                        case 6: value = (long)process.GetMemoryCapacity(); break;

                        case 7: value = (long)process.GetMemoryUsage(); break;

                        case 12: value = (long)process.MemoryManager.GetAddrSpaceBaseAddr(); break;

                        case 13: value = (long)process.MemoryManager.GetAddrSpaceSize(); break;

                        case 14: value = (long)process.MemoryManager.StackRegionStart; break;
                        case 15: value = (long)(process.MemoryManager.StackRegionEnd -
                                                process.MemoryManager.StackRegionStart); break;

                        case 16: value = (long)process.PersonalMmHeapPagesCount * KMemoryManager.PageSize; break;

                        case 17:
                            if (process.PersonalMmHeapPagesCount != 0)
                            {
                                value = process.MemoryManager.GetMmUsedPages() * KMemoryManager.PageSize;
                            }

                            break;

                        case 18: value = process.TitleId; break;

                        case 20: value = (long)process.UserExceptionContextAddress; break;

                        case 21: value = (long)process.GetMemoryCapacityWithoutPersonalMmHeap(); break;

                        case 22: value = (long)process.GetMemoryUsageWithoutPersonalMmHeap(); break;
                    }

                    break;
                }

                case 8:
                {
                    if (handle != 0)
                    {
                        return KernelResult.InvalidHandle;
                    }

                    if (subId != 0)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    value = _system.Scheduler.GetCurrentProcess().Debug ? 1 : 0;

                    break;
                }

                case 9:
                {
                    if (handle != 0)
                    {
                        return KernelResult.InvalidHandle;
                    }

                    if (subId != 0)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

                    if (currentProcess.ResourceLimit != null)
                    {
                        KHandleTable   handleTable   = currentProcess.HandleTable;
                        KResourceLimit resourceLimit = currentProcess.ResourceLimit;

                        KernelResult result = handleTable.GenerateHandle(resourceLimit, out int resLimHandle);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }

                        value = (uint)resLimHandle;
                    }

                    break;
                }

                case 10:
                {
                    if (handle != 0)
                    {
                        return KernelResult.InvalidHandle;
                    }

                    int currentCore = _system.Scheduler.GetCurrentThread().CurrentCore;

                    if (subId != -1 && subId != currentCore)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    value = _system.Scheduler.CoreContexts[currentCore].TotalIdleTimeTicks;

                    break;
                }

                case 11:
                {
                    if (handle != 0)
                    {
                        return KernelResult.InvalidHandle;
                    }

                    if ((ulong)subId > 3)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    KProcess currentProcess = _system.Scheduler.GetCurrentProcess();


                    value = currentProcess.RandomEntropy[subId];

                    break;
                }

                case 0xf0000002u:
                {
                    if (subId < -1 || subId > 3)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    KThread thread = _system.Scheduler.GetCurrentProcess().HandleTable.GetKThread(handle);

                    if (thread == null)
                    {
                        return KernelResult.InvalidHandle;
                    }

                    KThread currentThread = _system.Scheduler.GetCurrentThread();

                    int currentCore = currentThread.CurrentCore;

                    if (subId != -1 && subId != currentCore)
                    {
                        return KernelResult.Success;
                    }

                    KCoreContext coreContext = _system.Scheduler.CoreContexts[currentCore];

                    long timeDelta = PerformanceCounter.ElapsedMilliseconds - coreContext.LastContextSwitchTime;

                    if (subId != -1)
                    {
                        value = KTimeManager.ConvertMillisecondsToTicks(timeDelta);
                    }
                    else
                    {
                        long totalTimeRunning = thread.TotalTimeRunning;

                        if (thread == currentThread)
                        {
                            totalTimeRunning += timeDelta;
                        }

                        value = KTimeManager.ConvertMillisecondsToTicks(totalTimeRunning);
                    }

                    break;
                }

                default: return KernelResult.InvalidEnumValue;
            }

            return KernelResult.Success;
        }

        private void CreateEvent64(CpuThreadState state)
        {
            KernelResult result = CreateEvent(out int wEventHandle, out int rEventHandle);

            state.X0 = (ulong)result;
            state.X1 = (ulong)wEventHandle;
            state.X2 = (ulong)rEventHandle;
        }

        private KernelResult CreateEvent(out int wEventHandle, out int rEventHandle)
        {
            KEvent Event = new KEvent(_system);

            KernelResult result = _process.HandleTable.GenerateHandle(Event.WritableEvent, out wEventHandle);

            if (result == KernelResult.Success)
            {
                result = _process.HandleTable.GenerateHandle(Event.ReadableEvent, out rEventHandle);

                if (result != KernelResult.Success)
                {
                    _process.HandleTable.CloseHandle(wEventHandle);
                }
            }
            else
            {
                rEventHandle = 0;
            }

            return result;
        }

        private void GetProcessList64(CpuThreadState state)
        {
            ulong address =      state.X1;
            int   maxOut  = (int)state.X2;

            KernelResult result = GetProcessList(address, maxOut, out int count);

            state.X0 = (ulong)result;
            state.X1 = (ulong)count;
        }

        private KernelResult GetProcessList(ulong address, int maxCount, out int count)
        {
            count = 0;

            if ((maxCount >> 28) != 0)
            {
                return KernelResult.MaximumExceeded;
            }

            if (maxCount != 0)
            {
                KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

                ulong copySize = (ulong)maxCount * 8;

                if (address + copySize <= address)
                {
                    return KernelResult.InvalidMemState;
                }

                if (currentProcess.MemoryManager.OutsideAddrSpace(address, copySize))
                {
                    return KernelResult.InvalidMemState;
                }
            }

            int copyCount = 0;

            lock (_system.Processes)
            {
                foreach (KProcess process in _system.Processes.Values)
                {
                    if (copyCount < maxCount)
                    {
                        if (!KernelTransfer.KernelToUserInt64(_system, (long)address + copyCount * 8, process.Pid))
                        {
                            return KernelResult.UserCopyFailed;
                        }
                    }

                    copyCount++;
                }
            }

            count = copyCount;

            return KernelResult.Success;
        }

        private void GetSystemInfo64(CpuThreadState state)
        {
            uint id     = (uint)state.X1;
            int  handle =  (int)state.X2;
            long subId  = (long)state.X3;

            KernelResult result = GetSystemInfo(id, handle, subId, out long value);

            state.X0 = (ulong)result;
            state.X1 = (ulong)value;
        }

        private KernelResult GetSystemInfo(uint id, int handle, long subId, out long value)
        {
            value = 0;

            if (id > 2)
            {
                return KernelResult.InvalidEnumValue;
            }

            if (handle != 0)
            {
                return KernelResult.InvalidHandle;
            }

            if (id < 2)
            {
                if ((ulong)subId > 3)
                {
                    return KernelResult.InvalidCombination;
                }

                KMemoryRegionManager region = _system.MemoryRegions[subId];

                switch (id)
                {
                    //Memory region capacity.
                    case 0: value = (long)region.Size; break;

                    //Memory region free space.
                    case 1:
                    {
                        ulong freePagesCount = region.GetFreePages();

                        value = (long)(freePagesCount * KMemoryManager.PageSize);

                        break;
                    }
                }
            }
            else /* if (Id == 2) */
            {
                if ((ulong)subId > 1)
                {
                    return KernelResult.InvalidCombination;
                }

                switch (subId)
                {
                    case 0: value = _system.PrivilegedProcessLowestId;  break;
                    case 1: value = _system.PrivilegedProcessHighestId; break;
                }
            }

            return KernelResult.Success;
        }

        private void CreatePort64(CpuThreadState state)
        {
            int  maxSessions =  (int)state.X2;
            bool isLight     =      (state.X3 & 1) != 0;
            long nameAddress = (long)state.X4;

            KernelResult result = CreatePort(
                maxSessions,
                isLight,
                nameAddress,
                out int serverPortHandle,
                out int clientPortHandle);

            state.X0 = (ulong)result;
            state.X1 = (ulong)serverPortHandle;
            state.X2 = (ulong)clientPortHandle;
        }

        private KernelResult CreatePort(
            int     maxSessions,
            bool    isLight,
            long    nameAddress,
            out int serverPortHandle,
            out int clientPortHandle)
        {
            serverPortHandle = clientPortHandle = 0;

            if (maxSessions < 1)
            {
                return KernelResult.MaximumExceeded;
            }

            KPort port = new KPort(_system);

            port.Initialize(maxSessions, isLight, nameAddress);

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KernelResult result = currentProcess.HandleTable.GenerateHandle(port.ClientPort, out clientPortHandle);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = currentProcess.HandleTable.GenerateHandle(port.ServerPort, out serverPortHandle);

            if (result != KernelResult.Success)
            {
                currentProcess.HandleTable.CloseHandle(clientPortHandle);
            }

            return result;
        }

        private void ManageNamedPort64(CpuThreadState state)
        {
            long nameAddress = (long)state.X1;
            int  maxSessions =  (int)state.X2;

            KernelResult result = ManageNamedPort(nameAddress, maxSessions, out int handle);

            state.X0 = (ulong)result;
            state.X1 = (ulong)handle;
        }

        private KernelResult ManageNamedPort(long nameAddress, int maxSessions, out int handle)
        {
            handle = 0;

            if (!KernelTransfer.UserToKernelString(_system, nameAddress, 12, out string name))
            {
                return KernelResult.UserCopyFailed;
            }

            if (maxSessions < 0 || name.Length > 11)
            {
                return KernelResult.MaximumExceeded;
            }

            if (maxSessions == 0)
            {
                return KClientPort.RemoveName(_system, name);
            }

            KPort port = new KPort(_system);

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KernelResult result = currentProcess.HandleTable.GenerateHandle(port.ServerPort, out handle);

            if (result != KernelResult.Success)
            {
                return result;
            }

            port.Initialize(maxSessions, false, 0);

            result = port.SetName(name);

            if (result != KernelResult.Success)
            {
                currentProcess.HandleTable.CloseHandle(handle);
            }

            return result;
        }
    }
}
