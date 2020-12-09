using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Memory;
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

        public KernelResult GetProcessId(int handle, out long pid)
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
            ProcessCreationInfo info,
            ReadOnlySpan<int> capabilities,
            out int handle,
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

        public KernelResult ConnectToNamedPort(ulong namePtr, out int handle)
        {
            handle = 0;

            if (!KernelTransfer.UserToKernelString(_context, namePtr, 12, out string name))
            {
                return KernelResult.UserCopyFailed;
            }

            return ConnectToNamedPort(name, out handle);
        }

        public KernelResult ConnectToNamedPort(string name, out int handle)
        {
            handle = 0;

            if (name.Length > 11)
            {
                return KernelResult.MaximumExceeded;
            }

            KAutoObject autoObj = KAutoObject.FindNamedObject(_context, name);

            if (!(autoObj is KClientPort clientPort))
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

        public KernelResult SendAsyncRequestWithUserBuffer(ulong messagePtr, ulong messageSize, int handle, out int doneEventHandle)
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
            bool isLight,
            ulong namePtr,
            out int serverSessionHandle,
            out int clientSessionHandle)
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

        public KernelResult AcceptSession(int portHandle, out int sessionHandle)
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
            ulong handlesPtr,
            int handlesCount,
            int replyTargetHandle,
            long timeout,
            out int handleIndex)
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

            if (!KernelTransfer.UserToKernelInt32Array(_context, handlesPtr, handles))
            {
                return KernelResult.UserCopyFailed;
            }

            return ReplyAndReceive(handles, replyTargetHandle, timeout, out handleIndex);
        }

        public KernelResult ReplyAndReceive(ReadOnlySpan<int> handles, int replyTargetHandle, long timeout, out int handleIndex)
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
            ulong handlesPtr,
            ulong messagePtr,
            ulong messageSize,
            int handlesCount,
            int replyTargetHandle,
            long timeout,
            out int handleIndex)
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

            if (!KernelTransfer.UserToKernelInt32Array(_context, handlesPtr, handles))
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
            int maxSessions,
            bool isLight,
            ulong namePtr,
            out int serverPortHandle,
            out int clientPortHandle)
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

        public KernelResult ManageNamedPort(ulong namePtr, int maxSessions, out int handle)
        {
            handle = 0;

            if (!KernelTransfer.UserToKernelString(_context, namePtr, 12, out string name))
            {
                return KernelResult.UserCopyFailed;
            }

            if (name.Length > 11)
            {
                return KernelResult.MaximumExceeded;
            }

            return ManageNamedPort(name, maxSessions, out handle);
        }

        public KernelResult ManageNamedPort(string name, int maxSessions, out int handle)
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

        public KernelResult ConnectToPort(int clientPortHandle, out int clientSessionHandle)
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

        public KernelResult SetHeapSize(ulong size, out ulong position)
        {
            if ((size & 0xfffffffe001fffff) != 0)
            {
                position = 0;

                return KernelResult.InvalidSize;
            }

            KProcess process = KernelStatic.GetCurrentProcess();

            return process.MemoryManager.SetHeapSize(size, out position);
        }

        public KernelResult SetMemoryAttribute(
            ulong position,
            ulong size,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeValue)
        {
            if (!PageAligned(position))
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

            KernelResult result = process.MemoryManager.SetMemoryAttribute(
                position,
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

        public KernelResult QueryMemory(ulong infoPtr, ulong position, out ulong pageInfo)
        {
            KProcess process = KernelStatic.GetCurrentProcess();

            KMemoryInfo blkInfo = process.MemoryManager.QueryMemory(position);

            process.CpuMemory.Write(infoPtr + 0x00, blkInfo.Address);
            process.CpuMemory.Write(infoPtr + 0x08, blkInfo.Size);
            process.CpuMemory.Write(infoPtr + 0x10, (int)blkInfo.State & 0xff);
            process.CpuMemory.Write(infoPtr + 0x14, (int)blkInfo.Attribute);
            process.CpuMemory.Write(infoPtr + 0x18, (int)blkInfo.Permission);
            process.CpuMemory.Write(infoPtr + 0x1c, blkInfo.IpcRefCount);
            process.CpuMemory.Write(infoPtr + 0x20, blkInfo.DeviceRefCount);
            process.CpuMemory.Write(infoPtr + 0x24, 0);

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

        public KernelResult CreateTransferMemory(ulong address, ulong size, KMemoryPermission permission, out int handle)
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

        private static bool PageAligned(ulong position)
        {
            return (position & (KMemoryManager.PageSize - 1)) == 0;
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

            string str = MemoryHelper.ReadAsciiString(process.CpuMemory, (long)strPtr, (long)size);

            Logger.Warning?.Print(LogClass.KernelSvc, str);
        }

        public KernelResult GetInfo(uint id, int handle, long subId, out long value)
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

                        KProcess currentProcess = KernelStatic.GetCurrentProcess();

                        KProcess process = currentProcess.HandleTable.GetKProcess(handle);

                        if (process == null)
                        {
                            return KernelResult.InvalidHandle;
                        }

                        switch (id)
                        {
                            case 0: value = process.Capabilities.AllowedCpuCoresMask; break;
                            case 1: value = process.Capabilities.AllowedThreadPriosMask; break;

                            case 2: value = (long)process.MemoryManager.AliasRegionStart; break;
                            case 3:
                                value = (long)(process.MemoryManager.AliasRegionEnd -
                                               process.MemoryManager.AliasRegionStart); break;

                            case 4: value = (long)process.MemoryManager.HeapRegionStart; break;
                            case 5:
                                value = (long)(process.MemoryManager.HeapRegionEnd -
                                               process.MemoryManager.HeapRegionStart); break;

                            case 6: value = (long)process.GetMemoryCapacity(); break;

                            case 7: value = (long)process.GetMemoryUsage(); break;

                            case 12: value = (long)process.MemoryManager.GetAddrSpaceBaseAddr(); break;

                            case 13: value = (long)process.MemoryManager.GetAddrSpaceSize(); break;

                            case 14: value = (long)process.MemoryManager.StackRegionStart; break;
                            case 15:
                                value = (long)(process.MemoryManager.StackRegionEnd -
                                               process.MemoryManager.StackRegionStart); break;

                            case 16: value = (long)process.PersonalMmHeapPagesCount * KMemoryManager.PageSize; break;

                            case 17:
                                if (process.PersonalMmHeapPagesCount != 0)
                                {
                                    value = process.MemoryManager.GetMmUsedPages() * KMemoryManager.PageSize;
                                }

                                break;

                            case 18: value = (long)process.TitleId; break;

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

                        value = KernelStatic.GetCurrentProcess().Debug ? 1 : 0;

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

                case 10:
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

                        value = KTimeManager.ConvertHostTicksToTicks(_context.Schedulers[currentCore].TotalIdleTimeTicks);

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

                        KProcess currentProcess = KernelStatic.GetCurrentProcess();

                        value = currentProcess.RandomEntropy[subId];

                        break;
                    }

                case 0xf0000002u:
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
                            value = KTimeManager.ConvertHostTicksToTicks(timeDelta);
                        }
                        else
                        {
                            long totalTimeRunning = thread.TotalTimeRunning;

                            if (thread == currentThread)
                            {
                                totalTimeRunning += timeDelta;
                            }

                            value = KTimeManager.ConvertHostTicksToTicks(totalTimeRunning);
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

        public KernelResult GetProcessList(ulong address, int maxCount, out int count)
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
                        if (!KernelTransfer.KernelToUserInt64(_context, address + (ulong)copyCount * 8, process.Pid))
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

        public KernelResult GetSystemInfo(uint id, int handle, long subId, out long value)
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

                KMemoryRegionManager region = _context.MemoryRegions[subId];

                switch (id)
                {
                    // Memory region capacity.
                    case 0: value = (long)region.Size; break;

                    // Memory region free space.
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
                    case 0: value = _context.PrivilegedProcessLowestId; break;
                    case 1: value = _context.PrivilegedProcessHighestId; break;
                }
            }

            return KernelResult.Success;
        }

        // Thread

        public KernelResult CreateThread(
            ulong entrypoint,
            ulong argsPtr,
            ulong stackTop,
            int priority,
            int cpuCore,
            out int handle)
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
                KernelStatic.GetCurrentThread().Sleep(timeout);
            }
        }

        public KernelResult GetThreadPriority(int handle, out int priority)
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

        public KernelResult GetThreadCoreMask(int handle, out int preferredCore, out long affinityMask)
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

        public KernelResult SetThreadCoreMask(int handle, int preferredCore, long affinityMask)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (preferredCore == -2)
            {
                preferredCore = currentProcess.DefaultCpuCore;

                affinityMask = 1 << preferredCore;
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
                else if ((affinityMask & (1 << preferredCore)) == 0)
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

        public KernelResult GetThreadId(int handle, out long threadUid)
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

            IVirtualMemoryManager memory = currentProcess.CpuMemory;

            memory.Write(address + 0x0, thread.Context.GetX(0));
            memory.Write(address + 0x8, thread.Context.GetX(1));
            memory.Write(address + 0x10, thread.Context.GetX(2));
            memory.Write(address + 0x18, thread.Context.GetX(3));
            memory.Write(address + 0x20, thread.Context.GetX(4));
            memory.Write(address + 0x28, thread.Context.GetX(5));
            memory.Write(address + 0x30, thread.Context.GetX(6));
            memory.Write(address + 0x38, thread.Context.GetX(7));
            memory.Write(address + 0x40, thread.Context.GetX(8));
            memory.Write(address + 0x48, thread.Context.GetX(9));
            memory.Write(address + 0x50, thread.Context.GetX(10));
            memory.Write(address + 0x58, thread.Context.GetX(11));
            memory.Write(address + 0x60, thread.Context.GetX(12));
            memory.Write(address + 0x68, thread.Context.GetX(13));
            memory.Write(address + 0x70, thread.Context.GetX(14));
            memory.Write(address + 0x78, thread.Context.GetX(15));
            memory.Write(address + 0x80, thread.Context.GetX(16));
            memory.Write(address + 0x88, thread.Context.GetX(17));
            memory.Write(address + 0x90, thread.Context.GetX(18));
            memory.Write(address + 0x98, thread.Context.GetX(19));
            memory.Write(address + 0xa0, thread.Context.GetX(20));
            memory.Write(address + 0xa8, thread.Context.GetX(21));
            memory.Write(address + 0xb0, thread.Context.GetX(22));
            memory.Write(address + 0xb8, thread.Context.GetX(23));
            memory.Write(address + 0xc0, thread.Context.GetX(24));
            memory.Write(address + 0xc8, thread.Context.GetX(25));
            memory.Write(address + 0xd0, thread.Context.GetX(26));
            memory.Write(address + 0xd8, thread.Context.GetX(27));
            memory.Write(address + 0xe0, thread.Context.GetX(28));
            memory.Write(address + 0xe8, thread.Context.GetX(29));
            memory.Write(address + 0xf0, thread.Context.GetX(30));
            memory.Write(address + 0xf8, thread.Context.GetX(31));

            memory.Write(address + 0x100, thread.LastPc);

            memory.Write(address + 0x108, (ulong)GetPsr(thread.Context));

            memory.Write(address + 0x110, thread.Context.GetV(0));
            memory.Write(address + 0x120, thread.Context.GetV(1));
            memory.Write(address + 0x130, thread.Context.GetV(2));
            memory.Write(address + 0x140, thread.Context.GetV(3));
            memory.Write(address + 0x150, thread.Context.GetV(4));
            memory.Write(address + 0x160, thread.Context.GetV(5));
            memory.Write(address + 0x170, thread.Context.GetV(6));
            memory.Write(address + 0x180, thread.Context.GetV(7));
            memory.Write(address + 0x190, thread.Context.GetV(8));
            memory.Write(address + 0x1a0, thread.Context.GetV(9));
            memory.Write(address + 0x1b0, thread.Context.GetV(10));
            memory.Write(address + 0x1c0, thread.Context.GetV(11));
            memory.Write(address + 0x1d0, thread.Context.GetV(12));
            memory.Write(address + 0x1e0, thread.Context.GetV(13));
            memory.Write(address + 0x1f0, thread.Context.GetV(14));
            memory.Write(address + 0x200, thread.Context.GetV(15));
            memory.Write(address + 0x210, thread.Context.GetV(16));
            memory.Write(address + 0x220, thread.Context.GetV(17));
            memory.Write(address + 0x230, thread.Context.GetV(18));
            memory.Write(address + 0x240, thread.Context.GetV(19));
            memory.Write(address + 0x250, thread.Context.GetV(20));
            memory.Write(address + 0x260, thread.Context.GetV(21));
            memory.Write(address + 0x270, thread.Context.GetV(22));
            memory.Write(address + 0x280, thread.Context.GetV(23));
            memory.Write(address + 0x290, thread.Context.GetV(24));
            memory.Write(address + 0x2a0, thread.Context.GetV(25));
            memory.Write(address + 0x2b0, thread.Context.GetV(26));
            memory.Write(address + 0x2c0, thread.Context.GetV(27));
            memory.Write(address + 0x2d0, thread.Context.GetV(28));
            memory.Write(address + 0x2e0, thread.Context.GetV(29));
            memory.Write(address + 0x2f0, thread.Context.GetV(30));
            memory.Write(address + 0x300, thread.Context.GetV(31));

            memory.Write(address + 0x310, (int)thread.Context.Fpcr);
            memory.Write(address + 0x314, (int)thread.Context.Fpsr);
            memory.Write(address + 0x318, thread.Context.Tpidr);

            return KernelResult.Success;
        }

        private static int GetPsr(ARMeilleure.State.ExecutionContext context)
        {
            return (context.GetPstateFlag(ARMeilleure.State.PState.NFlag) ? (1 << (int)ARMeilleure.State.PState.NFlag) : 0) |
                   (context.GetPstateFlag(ARMeilleure.State.PState.ZFlag) ? (1 << (int)ARMeilleure.State.PState.ZFlag) : 0) |
                   (context.GetPstateFlag(ARMeilleure.State.PState.CFlag) ? (1 << (int)ARMeilleure.State.PState.CFlag) : 0) |
                   (context.GetPstateFlag(ARMeilleure.State.PState.VFlag) ? (1 << (int)ARMeilleure.State.PState.VFlag) : 0);
        }

        // Thread synchronization

        public KernelResult WaitSynchronization(ulong handlesPtr, int handlesCount, long timeout, out int handleIndex)
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

                if (!KernelTransfer.UserToKernelInt32Array(_context, handlesPtr, handles))
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
