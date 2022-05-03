using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    class Syscall
    {
        private readonly KernelContext _context;

        public Syscall(KernelContext context)
        {
            _context = context;
        }

        // Process

        public KernelResult GetProcessId(out ulong pid, int handle)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KProcess process = currentProcess.HandleTable.GetKProcess(handle);

            if (process == null)
            {
                KThread thread = currentProcess.HandleTable.GetKThread(handle);

                if (thread != null)
                {
                    process = thread.Owner;
                }

                // TODO: KDebugEvent.
            }

            pid = process?.Pid ?? 0;

            return process != null
                ? KernelResult.Success
                : KernelResult.InvalidHandle;
        }

        public KernelResult CreateProcess(
            out int handle,
            ProcessCreationInfo info,
            ReadOnlySpan<int> capabilities,
            IProcessContextFactory contextFactory,
            ThreadStart customThreadStart = null)
        {
            handle = 0;

            if ((info.Flags & ~ProcessCreationFlags.All) != 0)
            {
                return KernelResult.InvalidEnumValue;
            }

            // TODO: Address space check.

            if ((info.Flags & ProcessCreationFlags.PoolPartitionMask) > ProcessCreationFlags.PoolPartitionSystemNonSecure)
            {
                return KernelResult.InvalidEnumValue;
            }

            if ((info.CodeAddress & 0x1fffff) != 0)
            {
                return KernelResult.InvalidAddress;
            }

            if (info.CodePagesCount < 0 || info.SystemResourcePagesCount < 0)
            {
                return KernelResult.InvalidSize;
            }

            if (info.Flags.HasFlag(ProcessCreationFlags.OptimizeMemoryAllocation) &&
                !info.Flags.HasFlag(ProcessCreationFlags.IsApplication))
            {
                return KernelResult.InvalidThread;
            }

            KHandleTable handleTable = KernelStatic.GetCurrentProcess().HandleTable;

            KProcess process = new KProcess(_context);

            using var _ = new OnScopeExit(process.DecrementReferenceCount);

            KResourceLimit resourceLimit;

            if (info.ResourceLimitHandle != 0)
            {
                resourceLimit = handleTable.GetObject<KResourceLimit>(info.ResourceLimitHandle);

                if (resourceLimit == null)
                {
                    return KernelResult.InvalidHandle;
                }
            }
            else
            {
                resourceLimit = _context.ResourceLimit;
            }

            MemoryRegion memRegion = (info.Flags & ProcessCreationFlags.PoolPartitionMask) switch
            {
                ProcessCreationFlags.PoolPartitionApplication => MemoryRegion.Application,
                ProcessCreationFlags.PoolPartitionApplet => MemoryRegion.Applet,
                ProcessCreationFlags.PoolPartitionSystem => MemoryRegion.Service,
                ProcessCreationFlags.PoolPartitionSystemNonSecure => MemoryRegion.NvServices,
                _ => MemoryRegion.NvServices
            };

            KernelResult result = process.Initialize(
                info,
                capabilities,
                resourceLimit,
                memRegion,
                contextFactory,
                customThreadStart);

            if (result != KernelResult.Success)
            {
                return result;
            }

            _context.Processes.TryAdd(process.Pid, process);

            return handleTable.GenerateHandle(process, out handle);
        }

        public KernelResult StartProcess(int handle, int priority, int cpuCore, ulong mainThreadStackSize)
        {
            KProcess process = KernelStatic.GetCurrentProcess().HandleTable.GetObject<KProcess>(handle);

            if (process == null)
            {
                return KernelResult.InvalidHandle;
            }

            if ((uint)cpuCore >= KScheduler.CpuCoresCount || !process.IsCpuCoreAllowed(cpuCore))
            {
                return KernelResult.InvalidCpuCore;
            }

            if ((uint)priority >= KScheduler.PrioritiesCount || !process.IsPriorityAllowed(priority))
            {
                return KernelResult.InvalidPriority;
            }

            process.DefaultCpuCore = cpuCore;

            KernelResult result = process.Start(priority, mainThreadStackSize);

            if (result != KernelResult.Success)
            {
                return result;
            }

            process.IncrementReferenceCount();

            return KernelResult.Success;
        }

        // IPC

        public KernelResult ConnectToNamedPort(out int handle, ulong namePtr)
        {
            handle = 0;

            if (!KernelTransfer.UserToKernelString(out string name, namePtr, 12))
            {
                return KernelResult.UserCopyFailed;
            }

            return ConnectToNamedPort(out handle, name);
        }

        public KernelResult ConnectToNamedPort(out int handle, string name)
        {
            handle = 0;

            if (name.Length > 11)
            {
                return KernelResult.MaximumExceeded;
            }

            KAutoObject autoObj = KAutoObject.FindNamedObject(_context, name);

            if (autoObj is not KClientPort clientPort)
            {
                return KernelResult.NotFound;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KernelResult result = currentProcess.HandleTable.ReserveHandle(out handle);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = clientPort.Connect(out KClientSession clientSession);

            if (result != KernelResult.Success)
            {
                currentProcess.HandleTable.CancelHandleReservation(handle);

                return result;
            }

            currentProcess.HandleTable.SetReservedHandleObj(handle, clientSession);

            clientSession.DecrementReferenceCount();

            return result;
        }

        public KernelResult SendSyncRequest(int handle)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KClientSession session = currentProcess.HandleTable.GetObject<KClientSession>(handle);

            if (session == null)
            {
                return KernelResult.InvalidHandle;
            }

            return session.SendSyncRequest();
        }

        public KernelResult SendSyncRequestWithUserBuffer(ulong messagePtr, ulong messageSize, int handle)
        {
            if (!PageAligned(messagePtr))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(messageSize) || messageSize == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (messagePtr + messageSize <= messagePtr)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KernelResult result = currentProcess.MemoryManager.BorrowIpcBuffer(messagePtr, messageSize);

            if (result != KernelResult.Success)
            {
                return result;
            }

            KClientSession session = currentProcess.HandleTable.GetObject<KClientSession>(handle);

            if (session == null)
            {
                result = KernelResult.InvalidHandle;
            }
            else
            {
                result = session.SendSyncRequest(messagePtr, messageSize);
            }

            KernelResult result2 = currentProcess.MemoryManager.UnborrowIpcBuffer(messagePtr, messageSize);

            if (result == KernelResult.Success)
            {
                result = result2;
            }

            return result;
        }

        public KernelResult SendAsyncRequestWithUserBuffer(out int doneEventHandle, ulong messagePtr, ulong messageSize, int handle)
        {
            doneEventHandle = 0;

            if (!PageAligned(messagePtr))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(messageSize) || messageSize == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (messagePtr + messageSize <= messagePtr)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KernelResult result = currentProcess.MemoryManager.BorrowIpcBuffer(messagePtr, messageSize);

            if (result != KernelResult.Success)
            {
                return result;
            }

            KResourceLimit resourceLimit = currentProcess.ResourceLimit;

            if (resourceLimit != null && !resourceLimit.Reserve(LimitableResource.Event, 1))
            {
                currentProcess.MemoryManager.UnborrowIpcBuffer(messagePtr, messageSize);

                return KernelResult.ResLimitExceeded;
            }

            KClientSession session = currentProcess.HandleTable.GetObject<KClientSession>(handle);

            if (session == null)
            {
                result = KernelResult.InvalidHandle;
            }
            else
            {
                KEvent doneEvent = new KEvent(_context);

                result = currentProcess.HandleTable.GenerateHandle(doneEvent.ReadableEvent, out doneEventHandle);

                if (result == KernelResult.Success)
                {
                    result = session.SendAsyncRequest(doneEvent.WritableEvent, messagePtr, messageSize);

                    if (result != KernelResult.Success)
                    {
                        currentProcess.HandleTable.CloseHandle(doneEventHandle);
                    }
                }
            }

            if (result != KernelResult.Success)
            {
                resourceLimit?.Release(LimitableResource.Event, 1);

                currentProcess.MemoryManager.UnborrowIpcBuffer(messagePtr, messageSize);
            }

            return result;
        }

        public KernelResult CreateSession(
            out int serverSessionHandle,
            out int clientSessionHandle,
            bool isLight,
            ulong namePtr)
        {
            serverSessionHandle = 0;
            clientSessionHandle = 0;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KResourceLimit resourceLimit = currentProcess.ResourceLimit;

            if (resourceLimit != null && !resourceLimit.Reserve(LimitableResource.Session, 1))
            {
                return KernelResult.ResLimitExceeded;
            }

            KernelResult result;

            if (isLight)
            {
                KLightSession session = new KLightSession(_context);

                result = currentProcess.HandleTable.GenerateHandle(session.ServerSession, out serverSessionHandle);

                if (result == KernelResult.Success)
                {
                    result = currentProcess.HandleTable.GenerateHandle(session.ClientSession, out clientSessionHandle);

                    if (result != KernelResult.Success)
                    {
                        currentProcess.HandleTable.CloseHandle(serverSessionHandle);

                        serverSessionHandle = 0;
                    }
                }

                session.ServerSession.DecrementReferenceCount();
                session.ClientSession.DecrementReferenceCount();
            }
            else
            {
                KSession session = new KSession(_context);

                result = currentProcess.HandleTable.GenerateHandle(session.ServerSession, out serverSessionHandle);

                if (result == KernelResult.Success)
                {
                    result = currentProcess.HandleTable.GenerateHandle(session.ClientSession, out clientSessionHandle);

                    if (result != KernelResult.Success)
                    {
                        currentProcess.HandleTable.CloseHandle(serverSessionHandle);

                        serverSessionHandle = 0;
                    }
                }

                session.ServerSession.DecrementReferenceCount();
                session.ClientSession.DecrementReferenceCount();
            }

            return result;
        }

        public KernelResult AcceptSession(out int sessionHandle, int portHandle)
        {
            sessionHandle = 0;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KServerPort serverPort = currentProcess.HandleTable.GetObject<KServerPort>(portHandle);

            if (serverPort == null)
            {
                return KernelResult.InvalidHandle;
            }

            KernelResult result = currentProcess.HandleTable.ReserveHandle(out int handle);

            if (result != KernelResult.Success)
            {
                return result;
            }

            KAutoObject session;

            if (serverPort.IsLight)
            {
                session = serverPort.AcceptIncomingLightConnection();
            }
            else
            {
                session = serverPort.AcceptIncomingConnection();
            }

            if (session != null)
            {
                currentProcess.HandleTable.SetReservedHandleObj(handle, session);

                session.DecrementReferenceCount();

                sessionHandle = handle;

                result = KernelResult.Success;
            }
            else
            {
                currentProcess.HandleTable.CancelHandleReservation(handle);

                result = KernelResult.NotFound;
            }

            return result;
        }

        public KernelResult ReplyAndReceive(
            out int handleIndex,
            ulong handlesPtr,
            int handlesCount,
            int replyTargetHandle,
            long timeout)
        {
            handleIndex = 0;

            if ((uint)handlesCount > 0x40)
            {
                return KernelResult.MaximumExceeded;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            ulong copySize = (ulong)((long)handlesCount * 4);

            if (!currentProcess.MemoryManager.InsideAddrSpace(handlesPtr, copySize))
            {
                return KernelResult.UserCopyFailed;
            }

            if (handlesPtr + copySize < handlesPtr)
            {
                return KernelResult.UserCopyFailed;
            }

            int[] handles = new int[handlesCount];

            if (!KernelTransfer.UserToKernelArray<int>(handlesPtr, handles))
            {
                return KernelResult.UserCopyFailed;
            }

            if (timeout > 0)
            {
                timeout += KTimeManager.DefaultTimeIncrementNanoseconds;
            }

            return ReplyAndReceive(out handleIndex, handles, replyTargetHandle, timeout);
        }

        public KernelResult ReplyAndReceive(out int handleIndex, ReadOnlySpan<int> handles, int replyTargetHandle, long timeout)
        {
            handleIndex = 0;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KSynchronizationObject[] syncObjs = new KSynchronizationObject[handles.Length];

            for (int index = 0; index < handles.Length; index++)
            {
                KSynchronizationObject obj = currentProcess.HandleTable.GetObject<KSynchronizationObject>(handles[index]);

                if (obj == null)
                {
                    return KernelResult.InvalidHandle;
                }

                syncObjs[index] = obj;
            }

            KernelResult result = KernelResult.Success;

            if (replyTargetHandle != 0)
            {
                KServerSession replyTarget = currentProcess.HandleTable.GetObject<KServerSession>(replyTargetHandle);

                if (replyTarget == null)
                {
                    result = KernelResult.InvalidHandle;
                }
                else
                {
                    result = replyTarget.Reply();
                }
            }

            if (result == KernelResult.Success)
            {
                if (timeout > 0)
                {
                    timeout += KTimeManager.DefaultTimeIncrementNanoseconds;
                }

                while ((result = _context.Synchronization.WaitFor(syncObjs, timeout, out handleIndex)) == KernelResult.Success)
                {
                    KServerSession session = currentProcess.HandleTable.GetObject<KServerSession>(handles[handleIndex]);

                    if (session == null)
                    {
                        break;
                    }

                    if ((result = session.Receive()) != KernelResult.NotFound)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public KernelResult ReplyAndReceiveWithUserBuffer(
            out int handleIndex,
            ulong handlesPtr,
            ulong messagePtr,
            ulong messageSize,
            int handlesCount,
            int replyTargetHandle,
            long timeout)
        {
            handleIndex = 0;

            if ((uint)handlesCount > 0x40)
            {
                return KernelResult.MaximumExceeded;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            ulong copySize = (ulong)((long)handlesCount * 4);

            if (!currentProcess.MemoryManager.InsideAddrSpace(handlesPtr, copySize))
            {
                return KernelResult.UserCopyFailed;
            }

            if (handlesPtr + copySize < handlesPtr)
            {
                return KernelResult.UserCopyFailed;
            }

            KernelResult result = currentProcess.MemoryManager.BorrowIpcBuffer(messagePtr, messageSize);

            if (result != KernelResult.Success)
            {
                return result;
            }

            int[] handles = new int[handlesCount];

            if (!KernelTransfer.UserToKernelArray<int>(handlesPtr, handles))
            {
                currentProcess.MemoryManager.UnborrowIpcBuffer(messagePtr, messageSize);

                return KernelResult.UserCopyFailed;
            }

            KSynchronizationObject[] syncObjs = new KSynchronizationObject[handlesCount];

            for (int index = 0; index < handlesCount; index++)
            {
                KSynchronizationObject obj = currentProcess.HandleTable.GetObject<KSynchronizationObject>(handles[index]);

                if (obj == null)
                {
                    currentProcess.MemoryManager.UnborrowIpcBuffer(messagePtr, messageSize);

                    return KernelResult.InvalidHandle;
                }

                syncObjs[index] = obj;
            }

            if (replyTargetHandle != 0)
            {
                KServerSession replyTarget = currentProcess.HandleTable.GetObject<KServerSession>(replyTargetHandle);

                if (replyTarget == null)
                {
                    result = KernelResult.InvalidHandle;
                }
                else
                {
                    result = replyTarget.Reply(messagePtr, messageSize);
                }
            }

            if (result == KernelResult.Success)
            {
                if (timeout > 0)
                {
                    timeout += KTimeManager.DefaultTimeIncrementNanoseconds;
                }

                while ((result = _context.Synchronization.WaitFor(syncObjs, timeout, out handleIndex)) == KernelResult.Success)
                {
                    KServerSession session = currentProcess.HandleTable.GetObject<KServerSession>(handles[handleIndex]);

                    if (session == null)
                    {
                        break;
                    }

                    if ((result = session.Receive(messagePtr, messageSize)) != KernelResult.NotFound)
                    {
                        break;
                    }
                }
            }

            currentProcess.MemoryManager.UnborrowIpcBuffer(messagePtr, messageSize);

            return result;
        }

        public KernelResult CreatePort(
            out int serverPortHandle,
            out int clientPortHandle,
            int maxSessions,
            bool isLight,
            ulong namePtr)
        {
            serverPortHandle = clientPortHandle = 0;

            if (maxSessions < 1)
            {
                return KernelResult.MaximumExceeded;
            }

            KPort port = new KPort(_context, maxSessions, isLight, (long)namePtr);

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

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

        public KernelResult ManageNamedPort(out int handle, ulong namePtr, int maxSessions)
        {
            handle = 0;

            if (!KernelTransfer.UserToKernelString(out string name, namePtr, 12))
            {
                return KernelResult.UserCopyFailed;
            }

            if (name.Length > 11)
            {
                return KernelResult.MaximumExceeded;
            }

            return ManageNamedPort(out handle, name, maxSessions);
        }

        public KernelResult ManageNamedPort(out int handle, string name, int maxSessions)
        {
            handle = 0;

            if (maxSessions < 0)
            {
                return KernelResult.MaximumExceeded;
            }

            if (maxSessions == 0)
            {
                return KAutoObject.RemoveName(_context, name);
            }

            KPort port = new KPort(_context, maxSessions, false, 0);

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KernelResult result = currentProcess.HandleTable.GenerateHandle(port.ServerPort, out handle);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = port.ClientPort.SetName(name);

            if (result != KernelResult.Success)
            {
                currentProcess.HandleTable.CloseHandle(handle);
            }

            return result;
        }

        public KernelResult ConnectToPort(out int clientSessionHandle, int clientPortHandle)
        {
            clientSessionHandle = 0;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KClientPort clientPort = currentProcess.HandleTable.GetObject<KClientPort>(clientPortHandle);

            if (clientPort == null)
            {
                return KernelResult.InvalidHandle;
            }

            KernelResult result = currentProcess.HandleTable.ReserveHandle(out int handle);

            if (result != KernelResult.Success)
            {
                return result;
            }

            KAutoObject session;

            if (clientPort.IsLight)
            {
                result = clientPort.ConnectLight(out KLightClientSession clientSession);

                session = clientSession;
            }
            else
            {
                result = clientPort.Connect(out KClientSession clientSession);

                session = clientSession;
            }

            if (result != KernelResult.Success)
            {
                currentProcess.HandleTable.CancelHandleReservation(handle);

                return result;
            }

            currentProcess.HandleTable.SetReservedHandleObj(handle, session);

            session.DecrementReferenceCount();

            clientSessionHandle = handle;

            return result;
        }

        // Memory

        public KernelResult SetHeapSize(out ulong address, ulong size)
        {
            if ((size & 0xfffffffe001fffff) != 0)
            {
                address = 0;

                return KernelResult.InvalidSize;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            return process.MemoryManager.SetHeapSize(size, out address);
        }

        public KernelResult SetMemoryPermission(ulong address, ulong size, KMemoryPermission permission)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            if (permission != KMemoryPermission.None && (permission | KMemoryPermission.Write) != KMemoryPermission.ReadAndWrite)
            {
                return KernelResult.InvalidPermission;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            return currentProcess.MemoryManager.SetMemoryPermission(address, size, permission);
        }

        public KernelResult SetMemoryAttribute(
            ulong address,
            ulong size,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeValue)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            MemoryAttribute attributes = attributeMask | attributeValue;

            if (attributes != attributeMask ||
               (attributes | MemoryAttribute.Uncached) != MemoryAttribute.Uncached)
            {
                return KernelResult.InvalidCombination;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            if (!process.MemoryManager.InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            KernelResult result = process.MemoryManager.SetMemoryAttribute(
                address,
                size,
                attributeMask,
                attributeValue);

            return result;
        }

        public KernelResult MapMemory(ulong dst, ulong src, ulong size)
        {
            if (!PageAligned(src | dst))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (src + size <= src || dst + size <= dst)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(src, size))
            {
                return KernelResult.InvalidMemState;
            }

            if (currentProcess.MemoryManager.OutsideStackRegion(dst, size) ||
                currentProcess.MemoryManager.InsideHeapRegion(dst, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(dst, size))
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            return process.MemoryManager.Map(dst, src, size);
        }

        public KernelResult UnmapMemory(ulong dst, ulong src, ulong size)
        {
            if (!PageAligned(src | dst))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (src + size <= src || dst + size <= dst)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(src, size))
            {
                return KernelResult.InvalidMemState;
            }

            if (currentProcess.MemoryManager.OutsideStackRegion(dst, size) ||
                currentProcess.MemoryManager.InsideHeapRegion(dst, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(dst, size))
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            return process.MemoryManager.Unmap(dst, src, size);
        }

        public KernelResult QueryMemory(ulong infoPtr, out ulong pageInfo, ulong address)
        {
            KernelResult result = QueryMemory(out MemoryInfo info, out pageInfo, address);

            if (result == KernelResult.Success)
            {
                return KernelTransfer.KernelToUser(infoPtr, info)
                    ? KernelResult.Success
                    : KernelResult.InvalidMemState;
            }

            return result;
        }

        public KernelResult QueryMemory(out MemoryInfo info, out ulong pageInfo, ulong address)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KMemoryInfo blockInfo = process.MemoryManager.QueryMemory(address);

            info = new MemoryInfo(
                blockInfo.Address,
                blockInfo.Size,
                blockInfo.State & MemoryState.UserMask,
                blockInfo.Attribute,
                blockInfo.Permission & KMemoryPermission.UserMask,
                blockInfo.IpcRefCount,
                blockInfo.DeviceRefCount);

            pageInfo = 0;

            return KernelResult.Success;
        }

        public KernelResult MapSharedMemory(int handle, ulong address, ulong size, KMemoryPermission permission)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            if ((permission | KMemoryPermission.Write) != KMemoryPermission.ReadAndWrite)
            {
                return KernelResult.InvalidPermission;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KSharedMemory sharedMemory = currentProcess.HandleTable.GetObject<KSharedMemory>(handle);

            if (sharedMemory == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion(address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion(address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return sharedMemory.MapIntoProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess,
                permission);
        }

        public KernelResult UnmapSharedMemory(int handle, ulong address, ulong size)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KSharedMemory sharedMemory = currentProcess.HandleTable.GetObject<KSharedMemory>(handle);

            if (sharedMemory == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion(address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion(address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return sharedMemory.UnmapFromProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess);
        }

        public KernelResult CreateTransferMemory(out int handle, ulong address, ulong size, KMemoryPermission permission)
        {
            handle = 0;

            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            if (permission > KMemoryPermission.ReadAndWrite || permission == KMemoryPermission.Write)
            {
                return KernelResult.InvalidPermission;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            KResourceLimit resourceLimit = process.ResourceLimit;

            if (resourceLimit != null && !resourceLimit.Reserve(LimitableResource.TransferMemory, 1))
            {
                return KernelResult.ResLimitExceeded;
            }

            void CleanUpForError()
            {
                resourceLimit?.Release(LimitableResource.TransferMemory, 1);
            }

            if (!process.MemoryManager.InsideAddrSpace(address, size))
            {
                CleanUpForError();

                return KernelResult.InvalidMemState;
            }

            KTransferMemory transferMemory = new KTransferMemory(_context);

            KernelResult result = transferMemory.Initialize(address, size, permission);

            if (result != KernelResult.Success)
            {
                CleanUpForError();

                return result;
            }

            result = process.HandleTable.GenerateHandle(transferMemory, out handle);

            transferMemory.DecrementReferenceCount();

            return result;
        }

        public KernelResult MapTransferMemory(int handle, ulong address, ulong size, KMemoryPermission permission)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            if (permission > KMemoryPermission.ReadAndWrite || permission == KMemoryPermission.Write)
            {
                return KernelResult.InvalidPermission;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KTransferMemory transferMemory = currentProcess.HandleTable.GetObject<KTransferMemory>(handle);

            if (transferMemory == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion(address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion(address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return transferMemory.MapIntoProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess,
                permission);
        }

        public KernelResult UnmapTransferMemory(int handle, ulong address, ulong size)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KTransferMemory transferMemory = currentProcess.HandleTable.GetObject<KTransferMemory>(handle);

            if (transferMemory == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion(address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion(address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return transferMemory.UnmapFromProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess);
        }

        public KernelResult MapPhysicalMemory(ulong address, ulong size)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if ((currentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                return KernelResult.InvalidState;
            }

            if (!currentProcess.MemoryManager.InsideAddrSpace(address, size) ||
                 currentProcess.MemoryManager.OutsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            return process.MemoryManager.MapPhysicalMemory(address, size);
        }

        public KernelResult UnmapPhysicalMemory(ulong address, ulong size)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if ((currentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                return KernelResult.InvalidState;
            }

            if (!currentProcess.MemoryManager.InsideAddrSpace(address, size) ||
                 currentProcess.MemoryManager.OutsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            return process.MemoryManager.UnmapPhysicalMemory(address, size);
        }

        public KernelResult CreateCodeMemory(ulong address, ulong size, out int handle)
        {
            handle = 0;

            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (size + address <= address)
            {
                return KernelResult.InvalidMemState;
            }

            KCodeMemory codeMemory = new KCodeMemory(_context);

            using var _ = new OnScopeExit(codeMemory.DecrementReferenceCount);

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            KernelResult result = codeMemory.Initialize(address, size);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return currentProcess.HandleTable.GenerateHandle(codeMemory, out handle);
        }

        public KernelResult ControlCodeMemory(int handle, CodeMemoryOperation op, ulong address, ulong size, KMemoryPermission permission)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KCodeMemory codeMemory = currentProcess.HandleTable.GetObject<KCodeMemory>(handle);

            // Newer versions of the kernel also returns an error here if the owner and process
            // where the operation will happen are the same. We do not return an error here
            // for homebrew because some of them requires this to be patched out to work (for JIT).
            if (codeMemory == null || (!currentProcess.AllowCodeMemoryForJit && codeMemory.Owner == currentProcess))
            {
                return KernelResult.InvalidHandle;
            }

            switch (op)
            {
                case CodeMemoryOperation.Map:
                    if (!currentProcess.MemoryManager.CanContain(address, size, MemoryState.CodeWritable))
                    {
                        return KernelResult.InvalidMemRange;
                    }

                    if (permission != KMemoryPermission.ReadAndWrite)
                    {
                        return KernelResult.InvalidPermission;
                    }

                    return codeMemory.Map(address, size, permission);

                case CodeMemoryOperation.MapToOwner:
                    if (!currentProcess.MemoryManager.CanContain(address, size, MemoryState.CodeReadOnly))
                    {
                        return KernelResult.InvalidMemRange;
                    }

                    if (permission != KMemoryPermission.Read && permission != KMemoryPermission.ReadAndExecute)
                    {
                        return KernelResult.InvalidPermission;
                    }

                    return codeMemory.MapToOwner(address, size, permission);

                case CodeMemoryOperation.Unmap:
                    if (!currentProcess.MemoryManager.CanContain(address, size, MemoryState.CodeWritable))
                    {
                        return KernelResult.InvalidMemRange;
                    }

                    if (permission != KMemoryPermission.None)
                    {
                        return KernelResult.InvalidPermission;
                    }

                    return codeMemory.Unmap(address, size);

                case CodeMemoryOperation.UnmapFromOwner:
                    if (!currentProcess.MemoryManager.CanContain(address, size, MemoryState.CodeReadOnly))
                    {
                        return KernelResult.InvalidMemRange;
                    }

                    if (permission != KMemoryPermission.None)
                    {
                        return KernelResult.InvalidPermission;
                    }

                    return codeMemory.UnmapFromOwner(address, size);

                default: return KernelResult.InvalidEnumValue;
            }
        }

        public KernelResult SetProcessMemoryPermission(int handle, ulong src, ulong size, KMemoryPermission permission)
        {
            if (!PageAligned(src))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (permission != KMemoryPermission.None &&
                permission != KMemoryPermission.Read &&
                permission != KMemoryPermission.ReadAndWrite &&
                permission != KMemoryPermission.ReadAndExecute)
            {
                return KernelResult.InvalidPermission;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KProcess targetProcess = currentProcess.HandleTable.GetObject<KProcess>(handle);

            if (targetProcess == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (targetProcess.MemoryManager.OutsideAddrSpace(src, size))
            {
                return KernelResult.InvalidMemState;
            }

            return targetProcess.MemoryManager.SetProcessMemoryPermission(src, size, permission);
        }

        public KernelResult MapProcessMemory(ulong dst, int handle, ulong src, ulong size)
        {
            if (!PageAligned(src) || !PageAligned(dst))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (dst + size <= dst || src + size <= src)
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess dstProcess = KernelStatic.GetCurrentProcess();
            KProcess srcProcess = dstProcess.HandleTable.GetObject<KProcess>(handle);

            if (srcProcess == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (!srcProcess.MemoryManager.InsideAddrSpace(src, size) ||
                !dstProcess.MemoryManager.CanContain(dst, size, MemoryState.ProcessMemory))
            {
                return KernelResult.InvalidMemRange;
            }

            KPageList pageList = new KPageList();

            KernelResult result = srcProcess.MemoryManager.GetPagesIfStateEquals(
                src,
                size,
                MemoryState.MapProcessAllowed,
                MemoryState.MapProcessAllowed,
                KMemoryPermission.None,
                KMemoryPermission.None,
                MemoryAttribute.Mask,
                MemoryAttribute.None,
                pageList);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return dstProcess.MemoryManager.MapPages(dst, pageList, MemoryState.ProcessMemory, KMemoryPermission.ReadAndWrite);
        }

        public KernelResult UnmapProcessMemory(ulong dst, int handle, ulong src, ulong size)
        {
            if (!PageAligned(src) || !PageAligned(dst))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (dst + size <= dst || src + size <= src)
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess dstProcess = KernelStatic.GetCurrentProcess();
            KProcess srcProcess = dstProcess.HandleTable.GetObject<KProcess>(handle);

            if (srcProcess == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (!srcProcess.MemoryManager.InsideAddrSpace(src, size) ||
                !dstProcess.MemoryManager.CanContain(dst, size, MemoryState.ProcessMemory))
            {
                return KernelResult.InvalidMemRange;
            }

            KernelResult result = dstProcess.MemoryManager.UnmapProcessMemory(dst, size, srcProcess.MemoryManager, src);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return KernelResult.Success;
        }

        public KernelResult MapProcessCodeMemory(int handle, ulong dst, ulong src, ulong size)
        {
            if (!PageAligned(dst) || !PageAligned(src))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KProcess targetProcess = currentProcess.HandleTable.GetObject<KProcess>(handle);

            if (targetProcess == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (targetProcess.MemoryManager.OutsideAddrSpace(dst, size) ||
                targetProcess.MemoryManager.OutsideAddrSpace(src, size) ||
                targetProcess.MemoryManager.InsideAliasRegion(dst, size) ||
                targetProcess.MemoryManager.InsideHeapRegion(dst, size))
            {
                return KernelResult.InvalidMemRange;
            }

            if (size + dst <= dst || size + src <= src)
            {
                return KernelResult.InvalidMemState;
            }

            return targetProcess.MemoryManager.MapProcessCodeMemory(dst, src, size);
        }

        public KernelResult UnmapProcessCodeMemory(int handle, ulong dst, ulong src, ulong size)
        {
            if (!PageAligned(dst) || !PageAligned(src))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            KProcess targetProcess = currentProcess.HandleTable.GetObject<KProcess>(handle);

            if (targetProcess == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (targetProcess.MemoryManager.OutsideAddrSpace(dst, size) ||
                targetProcess.MemoryManager.OutsideAddrSpace(src, size) ||
                targetProcess.MemoryManager.InsideAliasRegion(dst, size) ||
                targetProcess.MemoryManager.InsideHeapRegion(dst, size))
            {
                return KernelResult.InvalidMemRange;
            }

            if (size + dst <= dst || size + src <= src)
            {
                return KernelResult.InvalidMemState;
            }

            return targetProcess.MemoryManager.UnmapProcessCodeMemory(dst, src, size);
        }

        private static bool PageAligned(ulong address)
        {
            return (address & (KPageTableBase.PageSize - 1)) == 0;
        }

        // System

        public KernelResult TerminateProcess(int handle)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            process = process.HandleTable.GetObject<KProcess>(handle);

            KernelResult result;

            if (process != null)
            {
                if (process == KernelStatic.GetCurrentProcess())
                {
                    result = KernelResult.Success;
                    process.DecrementToZeroWhileTerminatingCurrent();
                }
                else
                {
                    result = process.Terminate();
                    process.DecrementReferenceCount();
                }
            }
            else
            {
                result = KernelResult.InvalidHandle;
            }

            return result;
        }

        public void ExitProcess()
        {
            KernelStatic.GetCurrentProcess().TerminateCurrentProcess();
        }

        public KernelResult SignalEvent(int handle)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KWritableEvent writableEvent = process.HandleTable.GetObject<KWritableEvent>(handle);

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

            return result;
        }

        public KernelResult ClearEvent(int handle)
        {
            KernelResult result;

            KProcess process = KernelStatic.GetCurrentProcess();

            KWritableEvent writableEvent = process.HandleTable.GetObject<KWritableEvent>(handle);

            if (writableEvent == null)
            {
                KReadableEvent readableEvent = process.HandleTable.GetObject<KReadableEvent>(handle);

                result = readableEvent?.Clear() ?? KernelResult.InvalidHandle;
            }
            else
            {
                result = writableEvent.Clear();
            }

            return result;
        }

        public KernelResult CloseHandle(int handle)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            return currentProcess.HandleTable.CloseHandle(handle) ? KernelResult.Success : KernelResult.InvalidHandle;
        }

        public KernelResult ResetSignal(int handle)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

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

            return result;
        }

        public ulong GetSystemTick()
        {
            return KernelStatic.GetCurrentThread().Context.CntpctEl0;
        }

        public void Break(ulong reason)
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            if ((reason & (1UL << 31)) == 0)
            {
                currentThread.PrintGuestStackTrace();
                currentThread.PrintGuestRegisterPrintout();

                // As the process is exiting, this is probably caused by emulation termination.
                if (currentThread.Owner.State == ProcessState.Exiting)
                {
                    return;
                }

                // TODO: Debug events.
                currentThread.Owner.TerminateCurrentProcess();

                throw new GuestBrokeExecutionException();
            }
            else
            {
                Logger.Debug?.Print(LogClass.KernelSvc, "Debugger triggered.");
            }
        }

        public void OutputDebugString(ulong strPtr, ulong size)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            string str = MemoryHelper.ReadAsciiString(process.CpuMemory, strPtr, (long)size);

            Logger.Warning?.Print(LogClass.KernelSvc, str);
        }

        public KernelResult GetInfo(out ulong value, InfoType id, int handle, long subId)
        {
            value = 0;

            switch (id)
            {
                case InfoType.CoreMask:
                case InfoType.PriorityMask:
                case InfoType.AliasRegionAddress:
                case InfoType.AliasRegionSize:
                case InfoType.HeapRegionAddress:
                case InfoType.HeapRegionSize:
                case InfoType.TotalMemorySize:
                case InfoType.UsedMemorySize:
                case InfoType.AslrRegionAddress:
                case InfoType.AslrRegionSize:
                case InfoType.StackRegionAddress:
                case InfoType.StackRegionSize:
                case InfoType.SystemResourceSizeTotal:
                case InfoType.SystemResourceSizeUsed:
                case InfoType.ProgramId:
                case InfoType.UserExceptionContextAddress:
                case InfoType.TotalNonSystemMemorySize:
                case InfoType.UsedNonSystemMemorySize:
                case InfoType.IsApplication:
                case InfoType.FreeThreadCount:
                    {
                        if (subId != 0)
                        {
                            return KernelResult.InvalidCombination;
                        }

                        KProcess currentProcess = KernelStatic.GetCurrentProcess();

                        KProcess process = currentProcess.HandleTable.GetKProcess(handle);

                        if (process == null)
                        {
                            return KernelResult.InvalidHandle;
                        }

                        switch (id)
                        {
                            case InfoType.CoreMask: value = process.Capabilities.AllowedCpuCoresMask; break;
                            case InfoType.PriorityMask: value = process.Capabilities.AllowedThreadPriosMask; break;

                            case InfoType.AliasRegionAddress: value = process.MemoryManager.AliasRegionStart; break;
                            case InfoType.AliasRegionSize:
                                value = (process.MemoryManager.AliasRegionEnd -
                                         process.MemoryManager.AliasRegionStart); break;

                            case InfoType.HeapRegionAddress: value = process.MemoryManager.HeapRegionStart; break;
                            case InfoType.HeapRegionSize:
                                value = (process.MemoryManager.HeapRegionEnd -
                                         process.MemoryManager.HeapRegionStart); break;

                            case InfoType.TotalMemorySize: value = process.GetMemoryCapacity(); break;

                            case InfoType.UsedMemorySize: value = process.GetMemoryUsage(); break;

                            case InfoType.AslrRegionAddress: value = process.MemoryManager.GetAddrSpaceBaseAddr(); break;

                            case InfoType.AslrRegionSize: value = process.MemoryManager.GetAddrSpaceSize(); break;

                            case InfoType.StackRegionAddress: value = process.MemoryManager.StackRegionStart; break;
                            case InfoType.StackRegionSize:
                                value = (process.MemoryManager.StackRegionEnd -
                                         process.MemoryManager.StackRegionStart); break;

                            case InfoType.SystemResourceSizeTotal: value = process.PersonalMmHeapPagesCount * KPageTableBase.PageSize; break;

                            case InfoType.SystemResourceSizeUsed:
                                if (process.PersonalMmHeapPagesCount != 0)
                                {
                                    value = process.MemoryManager.GetMmUsedPages() * KPageTableBase.PageSize;
                                }

                                break;

                            case InfoType.ProgramId: value = process.TitleId; break;

                            case InfoType.UserExceptionContextAddress: value = process.UserExceptionContextAddress; break;

                            case InfoType.TotalNonSystemMemorySize: value = process.GetMemoryCapacityWithoutPersonalMmHeap(); break;

                            case InfoType.UsedNonSystemMemorySize: value = process.GetMemoryUsageWithoutPersonalMmHeap(); break;

                            case InfoType.IsApplication: value = process.IsApplication ? 1UL : 0UL; break;

                            case InfoType.FreeThreadCount:
                                if (process.ResourceLimit != null)
                                {
                                    value = (ulong)(process.ResourceLimit.GetLimitValue(LimitableResource.Thread) -
                                                    process.ResourceLimit.GetCurrentValue(LimitableResource.Thread));
                                }
                                else
                                {
                                    value = 0;
                                }

                                break;
                        }

                        break;
                    }

                case InfoType.DebuggerAttached:
                    {
                        if (handle != 0)
                        {
                            return KernelResult.InvalidHandle;
                        }

                        if (subId != 0)
                        {
                            return KernelResult.InvalidCombination;
                        }

                        value = KernelStatic.GetCurrentProcess().Debug ? 1UL : 0UL;

                        break;
                    }

                case InfoType.ResourceLimit:
                    {
                        if (handle != 0)
                        {
                            return KernelResult.InvalidHandle;
                        }

                        if (subId != 0)
                        {
                            return KernelResult.InvalidCombination;
                        }

                        KProcess currentProcess = KernelStatic.GetCurrentProcess();

                        if (currentProcess.ResourceLimit != null)
                        {
                            KHandleTable handleTable = currentProcess.HandleTable;
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

                case InfoType.IdleTickCount:
                    {
                        if (handle != 0)
                        {
                            return KernelResult.InvalidHandle;
                        }

                        int currentCore = KernelStatic.GetCurrentThread().CurrentCore;

                        if (subId != -1 && subId != currentCore)
                        {
                            return KernelResult.InvalidCombination;
                        }

                        value = (ulong)KTimeManager.ConvertHostTicksToTicks(_context.Schedulers[currentCore].TotalIdleTimeTicks);

                        break;
                    }

                case InfoType.RandomEntropy:
                    {
                        if (handle != 0)
                        {
                            return KernelResult.InvalidHandle;
                        }

                        if ((ulong)subId > 3)
                        {
                            return KernelResult.InvalidCombination;
                        }

                        KProcess currentProcess = KernelStatic.GetCurrentProcess();

                        value = currentProcess.RandomEntropy[subId];

                        break;
                    }

                case InfoType.ThreadTickCount:
                    {
                        if (subId < -1 || subId > 3)
                        {
                            return KernelResult.InvalidCombination;
                        }

                        KThread thread = KernelStatic.GetCurrentProcess().HandleTable.GetKThread(handle);

                        if (thread == null)
                        {
                            return KernelResult.InvalidHandle;
                        }

                        KThread currentThread = KernelStatic.GetCurrentThread();

                        int currentCore = currentThread.CurrentCore;

                        if (subId != -1 && subId != currentCore)
                        {
                            return KernelResult.Success;
                        }

                        KScheduler scheduler = _context.Schedulers[currentCore];

                        long timeDelta = PerformanceCounter.ElapsedTicks - scheduler.LastContextSwitchTime;

                        if (subId != -1)
                        {
                            value = (ulong)KTimeManager.ConvertHostTicksToTicks(timeDelta);
                        }
                        else
                        {
                            long totalTimeRunning = thread.TotalTimeRunning;

                            if (thread == currentThread)
                            {
                                totalTimeRunning += timeDelta;
                            }

                            value = (ulong)KTimeManager.ConvertHostTicksToTicks(totalTimeRunning);
                        }

                        break;
                    }

                default: return KernelResult.InvalidEnumValue;
            }

            return KernelResult.Success;
        }

        public KernelResult CreateEvent(out int wEventHandle, out int rEventHandle)
        {
            KEvent Event = new KEvent(_context);

            KProcess process = KernelStatic.GetCurrentProcess();

            KernelResult result = process.HandleTable.GenerateHandle(Event.WritableEvent, out wEventHandle);

            if (result == KernelResult.Success)
            {
                result = process.HandleTable.GenerateHandle(Event.ReadableEvent, out rEventHandle);

                if (result != KernelResult.Success)
                {
                    process.HandleTable.CloseHandle(wEventHandle);
                }
            }
            else
            {
                rEventHandle = 0;
            }

            return result;
        }

        public KernelResult GetProcessList(out int count, ulong address, int maxCount)
        {
            count = 0;

            if ((maxCount >> 28) != 0)
            {
                return KernelResult.MaximumExceeded;
            }

            if (maxCount != 0)
            {
                KProcess currentProcess = KernelStatic.GetCurrentProcess();

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

            lock (_context.Processes)
            {
                foreach (KProcess process in _context.Processes.Values)
                {
                    if (copyCount < maxCount)
                    {
                        if (!KernelTransfer.KernelToUser(address + (ulong)copyCount * 8, process.Pid))
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

        public KernelResult GetSystemInfo(out long value, uint id, int handle, long subId)
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

                KMemoryRegionManager region = _context.MemoryManager.MemoryRegions[subId];

                switch (id)
                {
                    // Memory region capacity.
                    case 0: value = (long)region.Size; break;

                    // Memory region free space.
                    case 1:
                        {
                            ulong freePagesCount = region.GetFreePages();

                            value = (long)(freePagesCount * KPageTableBase.PageSize);

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
                    case 0: value = _context.PrivilegedProcessLowestId; break;
                    case 1: value = _context.PrivilegedProcessHighestId; break;
                }
            }

            return KernelResult.Success;
        }

        public KernelResult GetResourceLimitLimitValue(out long limitValue, int handle, LimitableResource resource)
        {
            limitValue = 0;

            if (resource >= LimitableResource.Count)
            {
                return KernelResult.InvalidEnumValue;
            }

            KResourceLimit resourceLimit = KernelStatic.GetCurrentProcess().HandleTable.GetObject<KResourceLimit>(handle);

            if (resourceLimit == null)
            {
                return KernelResult.InvalidHandle;
            }

            limitValue = resourceLimit.GetLimitValue(resource);

            return KernelResult.Success;
        }

        public KernelResult GetResourceLimitCurrentValue(out long limitValue, int handle, LimitableResource resource)
        {
            limitValue = 0;

            if (resource >= LimitableResource.Count)
            {
                return KernelResult.InvalidEnumValue;
            }

            KResourceLimit resourceLimit = KernelStatic.GetCurrentProcess().HandleTable.GetObject<KResourceLimit>(handle);

            if (resourceLimit == null)
            {
                return KernelResult.InvalidHandle;
            }

            limitValue = resourceLimit.GetCurrentValue(resource);

            return KernelResult.Success;
        }

        public KernelResult GetResourceLimitPeakValue(out long peak, int handle, LimitableResource resource)
        {
            peak = 0;

            if (resource >= LimitableResource.Count)
            {
                return KernelResult.InvalidEnumValue;
            }

            KResourceLimit resourceLimit = KernelStatic.GetCurrentProcess().HandleTable.GetObject<KResourceLimit>(handle);

            if (resourceLimit == null)
            {
                return KernelResult.InvalidHandle;
            }

            peak = resourceLimit.GetPeakValue(resource);

            return KernelResult.Success;
        }

        public KernelResult CreateResourceLimit(out int handle)
        {
            KResourceLimit limit = new KResourceLimit(_context);

            KProcess process = KernelStatic.GetCurrentProcess();

            return process.HandleTable.GenerateHandle(limit, out handle);
        }

        public KernelResult SetResourceLimitLimitValue(int handle, LimitableResource resource, long limitValue)
        {
            if (resource >= LimitableResource.Count)
            {
                return KernelResult.InvalidEnumValue;
            }

            KResourceLimit resourceLimit = KernelStatic.GetCurrentProcess().HandleTable.GetObject<KResourceLimit>(handle);

            if (resourceLimit == null)
            {
                return KernelResult.InvalidHandle;
            }

            return resourceLimit.SetLimitValue(resource, limitValue);
        }

        // Thread

        public KernelResult CreateThread(
            out int handle,
            ulong entrypoint,
            ulong argsPtr,
            ulong stackTop,
            int priority,
            int cpuCore)
        {
            handle = 0;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (cpuCore == -2)
            {
                cpuCore = currentProcess.DefaultCpuCore;
            }

            if ((uint)cpuCore >= KScheduler.CpuCoresCount || !currentProcess.IsCpuCoreAllowed(cpuCore))
            {
                return KernelResult.InvalidCpuCore;
            }

            if ((uint)priority >= KScheduler.PrioritiesCount || !currentProcess.IsPriorityAllowed(priority))
            {
                return KernelResult.InvalidPriority;
            }

            long timeout = KTimeManager.ConvertMillisecondsToNanoseconds(100);

            if (currentProcess.ResourceLimit != null &&
               !currentProcess.ResourceLimit.Reserve(LimitableResource.Thread, 1, timeout))
            {
                return KernelResult.ResLimitExceeded;
            }

            KThread thread = new KThread(_context);

            KernelResult result = currentProcess.InitializeThread(
                thread,
                entrypoint,
                argsPtr,
                stackTop,
                priority,
                cpuCore);

            if (result == KernelResult.Success)
            {
                KProcess process = KernelStatic.GetCurrentProcess();

                result = process.HandleTable.GenerateHandle(thread, out handle);
            }
            else
            {
                currentProcess.ResourceLimit?.Release(LimitableResource.Thread, 1);
            }

            thread.DecrementReferenceCount();

            return result;
        }

        public KernelResult StartThread(int handle)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                thread.IncrementReferenceCount();

                KernelResult result = thread.Start();

                if (result == KernelResult.Success)
                {
                    thread.IncrementReferenceCount();
                }

                thread.DecrementReferenceCount();

                return result;
            }
            else
            {
                return KernelResult.InvalidHandle;
            }
        }

        public void ExitThread()
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            currentThread.Exit();
        }

        public void SleepThread(long timeout)
        {
            if (timeout < 1)
            {
                switch (timeout)
                {
                    case 0: KScheduler.Yield(_context); break;
                    case -1: KScheduler.YieldWithLoadBalancing(_context); break;
                    case -2: KScheduler.YieldToAnyThread(_context); break;
                }
            }
            else
            {
                KernelStatic.GetCurrentThread().Sleep(timeout + KTimeManager.DefaultTimeIncrementNanoseconds);
            }
        }

        public KernelResult GetThreadPriority(out int priority, int handle)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                priority = thread.DynamicPriority;

                return KernelResult.Success;
            }
            else
            {
                priority = 0;

                return KernelResult.InvalidHandle;
            }
        }

        public KernelResult SetThreadPriority(int handle, int priority)
        {
            // TODO: NPDM check.

            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            thread.SetPriority(priority);

            return KernelResult.Success;
        }

        public KernelResult GetThreadCoreMask(out int preferredCore, out ulong affinityMask, int handle)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                preferredCore = thread.PreferredCore;
                affinityMask = thread.AffinityMask;

                return KernelResult.Success;
            }
            else
            {
                preferredCore = 0;
                affinityMask = 0;

                return KernelResult.InvalidHandle;
            }
        }

        public KernelResult SetThreadCoreMask(int handle, int preferredCore, ulong affinityMask)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (preferredCore == -2)
            {
                preferredCore = currentProcess.DefaultCpuCore;

                affinityMask = 1UL << preferredCore;
            }
            else
            {
                if ((currentProcess.Capabilities.AllowedCpuCoresMask | affinityMask) !=
                     currentProcess.Capabilities.AllowedCpuCoresMask)
                {
                    return KernelResult.InvalidCpuCore;
                }

                if (affinityMask == 0)
                {
                    return KernelResult.InvalidCombination;
                }

                if ((uint)preferredCore > 3)
                {
                    if ((preferredCore | 2) != -1)
                    {
                        return KernelResult.InvalidCpuCore;
                    }
                }
                else if ((affinityMask & (1UL << preferredCore)) == 0)
                {
                    return KernelResult.InvalidCombination;
                }
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            return thread.SetCoreAndAffinityMask(preferredCore, affinityMask);
        }

        public int GetCurrentProcessorNumber()
        {
            return KernelStatic.GetCurrentThread().CurrentCore;
        }

        public KernelResult GetThreadId(out ulong threadUid, int handle)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                threadUid = thread.ThreadUid;

                return KernelResult.Success;
            }
            else
            {
                threadUid = 0;

                return KernelResult.InvalidHandle;
            }
        }

        public KernelResult SetThreadActivity(int handle, bool pause)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetObject<KThread>(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (thread.Owner != process)
            {
                return KernelResult.InvalidHandle;
            }

            if (thread == KernelStatic.GetCurrentThread())
            {
                return KernelResult.InvalidThread;
            }

            return thread.SetActivity(pause);
        }

        public KernelResult GetThreadContext3(ulong address, int handle)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();
            KThread currentThread = KernelStatic.GetCurrentThread();

            KThread thread = currentProcess.HandleTable.GetObject<KThread>(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (thread.Owner != currentProcess)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentThread == thread)
            {
                return KernelResult.InvalidThread;
            }

            KernelResult result = thread.GetThreadContext3(out ThreadContext context);

            if (result == KernelResult.Success)
            {
                return KernelTransfer.KernelToUser(address, context)
                    ? KernelResult.Success
                    : KernelResult.InvalidMemState;
            }

            return result;
        }

        // Thread synchronization

        public KernelResult WaitSynchronization(out int handleIndex, ulong handlesPtr, int handlesCount, long timeout)
        {
            handleIndex = 0;

            if ((uint)handlesCount > KThread.MaxWaitSyncObjects)
            {
                return KernelResult.MaximumExceeded;
            }

            KThread currentThread = KernelStatic.GetCurrentThread();

            var syncObjs = new Span<KSynchronizationObject>(currentThread.WaitSyncObjects).Slice(0, handlesCount);

            if (handlesCount != 0)
            {
                KProcess currentProcess = KernelStatic.GetCurrentProcess();

                if (currentProcess.MemoryManager.AddrSpaceStart > handlesPtr)
                {
                    return KernelResult.UserCopyFailed;
                }

                long handlesSize = handlesCount * 4;

                if (handlesPtr + (ulong)handlesSize <= handlesPtr)
                {
                    return KernelResult.UserCopyFailed;
                }

                if (handlesPtr + (ulong)handlesSize - 1 > currentProcess.MemoryManager.AddrSpaceEnd - 1)
                {
                    return KernelResult.UserCopyFailed;
                }

                Span<int> handles = new Span<int>(currentThread.WaitSyncHandles).Slice(0, handlesCount);

                if (!KernelTransfer.UserToKernelArray(handlesPtr, handles))
                {
                    return KernelResult.UserCopyFailed;
                }

                int processedHandles = 0;

                for (; processedHandles < handlesCount; processedHandles++)
                {
                    KSynchronizationObject syncObj = currentProcess.HandleTable.GetObject<KSynchronizationObject>(handles[processedHandles]);

                    if (syncObj == null)
                    {
                        break;
                    }

                    syncObjs[processedHandles] = syncObj;

                    syncObj.IncrementReferenceCount();
                }

                if (processedHandles != handlesCount)
                {
                    // One or more handles are invalid.
                    for (int index = 0; index < processedHandles; index++)
                    {
                        currentThread.WaitSyncObjects[index].DecrementReferenceCount();
                    }

                    return KernelResult.InvalidHandle;
                }
            }

            if (timeout > 0)
            {
                timeout += KTimeManager.DefaultTimeIncrementNanoseconds;
            }

            KernelResult result = _context.Synchronization.WaitFor(syncObjs, timeout, out handleIndex);

            if (result == KernelResult.PortRemoteClosed)
            {
                result = KernelResult.Success;
            }

            for (int index = 0; index < handlesCount; index++)
            {
                currentThread.WaitSyncObjects[index].DecrementReferenceCount();
            }

            return result;
        }

        public KernelResult CancelSynchronization(int handle)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KThread thread = process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            thread.CancelSynchronization();

            return KernelResult.Success;
        }

        public KernelResult ArbitrateLock(int ownerHandle, ulong mutexAddress, int requesterHandle)
        {
            if (IsPointingInsideKernel(mutexAddress))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            return currentProcess.AddressArbiter.ArbitrateLock(ownerHandle, mutexAddress, requesterHandle);
        }

        public KernelResult ArbitrateUnlock(ulong mutexAddress)
        {
            if (IsPointingInsideKernel(mutexAddress))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            return currentProcess.AddressArbiter.ArbitrateUnlock(mutexAddress);
        }

        public KernelResult WaitProcessWideKeyAtomic(
            ulong mutexAddress,
            ulong condVarAddress,
            int handle,
            long timeout)
        {
            if (IsPointingInsideKernel(mutexAddress))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(mutexAddress))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (timeout > 0)
            {
                timeout += KTimeManager.DefaultTimeIncrementNanoseconds;
            }

            return currentProcess.AddressArbiter.WaitProcessWideKeyAtomic(
                mutexAddress,
                condVarAddress,
                handle,
                timeout);
        }

        public KernelResult SignalProcessWideKey(ulong address, int count)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            currentProcess.AddressArbiter.SignalProcessWideKey(address, count);

            return KernelResult.Success;
        }

        public KernelResult WaitForAddress(ulong address, ArbitrationType type, int value, long timeout)
        {
            if (IsPointingInsideKernel(address))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (timeout > 0)
            {
                timeout += KTimeManager.DefaultTimeIncrementNanoseconds;
            }

            return type switch
            {
                ArbitrationType.WaitIfLessThan
                    => currentProcess.AddressArbiter.WaitForAddressIfLessThan(address, value, false, timeout),
                ArbitrationType.DecrementAndWaitIfLessThan
                    => currentProcess.AddressArbiter.WaitForAddressIfLessThan(address, value, true, timeout),
                ArbitrationType.WaitIfEqual
                    => currentProcess.AddressArbiter.WaitForAddressIfEqual(address, value, timeout),
                _ => KernelResult.InvalidEnumValue,
            };
        }

        public KernelResult SignalToAddress(ulong address, SignalType type, int value, int count)
        {
            if (IsPointingInsideKernel(address))
            {
                return KernelResult.InvalidMemState;
            }

            if (IsAddressNotWordAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            return type switch
            {
                SignalType.Signal
                    => currentProcess.AddressArbiter.Signal(address, count),
                SignalType.SignalAndIncrementIfEqual
                    => currentProcess.AddressArbiter.SignalAndIncrementIfEqual(address, value, count),
                SignalType.SignalAndModifyIfEqual
                    => currentProcess.AddressArbiter.SignalAndModifyIfEqual(address, value, count),
                _ => KernelResult.InvalidEnumValue
            };
        }

        public KernelResult SynchronizePreemptionState()
        {
            KernelStatic.GetCurrentThread().SynchronizePreemptionState();

            return KernelResult.Success;
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
