using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    partial class SvcHandler
    {
        private struct HleIpcMessage
        {
            public KThread        Thread     { get; private set; }
            public KClientSession Session    { get; private set; }
            public IpcMessage     Message    { get; private set; }
            public long           MessagePtr { get; private set; }

            public HleIpcMessage(
                KThread        thread,
                KClientSession session,
                IpcMessage     message,
                long           messagePtr)
            {
                Thread     = thread;
                Session    = session;
                Message    = message;
                MessagePtr = messagePtr;
            }
        }

        public KernelResult ConnectToNamedPort64([R(1)] ulong namePtr, [R(1)] out int handle)
        {
            return ConnectToNamedPort(namePtr, out handle);
        }

        public KernelResult ConnectToNamedPort32([R(1)] uint namePtr, [R(1)] out int handle)
        {
            return ConnectToNamedPort(namePtr, out handle);
        }

        private KernelResult ConnectToNamedPort(ulong namePtr, out int handle)
        {
            handle = 0;

            if (!KernelTransfer.UserToKernelString(_system, namePtr, 12, out string name))
            {
                return KernelResult.UserCopyFailed;
            }

            if (name.Length > 11)
            {
                return KernelResult.MaximumExceeded;
            }

            KAutoObject autoObj = KAutoObject.FindNamedObject(_system, name);

            if (!(autoObj is KClientPort clientPort))
            {
                return KernelResult.NotFound;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

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

        public KernelResult SendSyncRequest64([R(0)] int handle)
        {
            return SendSyncRequest((ulong)_system.Scheduler.GetCurrentThread().Context.Tpidr, 0x100, handle);
        }

        public KernelResult SendSyncRequest32([R(0)] int handle)
        {
            return SendSyncRequest((ulong)_system.Scheduler.GetCurrentThread().Context.Tpidr, 0x100, handle);
        }

        public KernelResult SendSyncRequestWithUserBuffer64([R(0)] ulong messagePtr, [R(1)] ulong size, [R(2)] int handle)
        {
            return SendSyncRequest(messagePtr, size, handle);
        }

        public KernelResult SendSyncRequestWithUserBuffer32([R(0)] uint messagePtr, [R(1)] uint size, [R(2)] int handle)
        {
            return SendSyncRequest(messagePtr, size, handle);
        }

        private KernelResult SendSyncRequest(ulong messagePtr, ulong size, int handle)
        {
            byte[] messageData = _process.CpuMemory.ReadBytes((long)messagePtr, (long)size);

            KClientSession clientSession = _process.HandleTable.GetObject<KClientSession>(handle);

            if (clientSession == null || clientSession.Service == null)
            {
                return SendSyncRequest_(handle);
            }

            if (clientSession != null)
            {
                _system.CriticalSection.Enter();

                KThread currentThread = _system.Scheduler.GetCurrentThread();

                currentThread.SignaledObj   = null;
                currentThread.ObjSyncResult = KernelResult.Success;

                currentThread.Reschedule(ThreadSchedState.Paused);

                IpcMessage message = new IpcMessage(messageData, (long)messagePtr);

                ThreadPool.QueueUserWorkItem(ProcessIpcRequest, new HleIpcMessage(
                    currentThread,
                    clientSession,
                    message,
                    (long)messagePtr));

                _system.ThreadCounter.AddCount();

                _system.CriticalSection.Leave();

                return currentThread.ObjSyncResult;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid session handle 0x{handle:x8}!");

                return KernelResult.InvalidHandle;
            }
        }

        private void ProcessIpcRequest(object state)
        {
            HleIpcMessage ipcMessage = (HleIpcMessage)state;

            ipcMessage.Thread.ObjSyncResult = IpcHandler.IpcCall(
                _device,
                _process,
                _process.CpuMemory,
                ipcMessage.Thread,
                ipcMessage.Session,
                ipcMessage.Message,
                ipcMessage.MessagePtr);

            _system.ThreadCounter.Signal();

            ipcMessage.Thread.Reschedule(ThreadSchedState.Running);
        }

        private KernelResult SendSyncRequest_(int handle)
        {
            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KClientSession session = currentProcess.HandleTable.GetObject<KClientSession>(handle);

            if (session == null)
            {
                return KernelResult.InvalidHandle;
            }

            return session.SendSyncRequest();
        }

        public KernelResult CreateSession64(
            [R(2)] bool    isLight,
            [R(3)] ulong   namePtr,
            [R(1)] out int serverSessionHandle,
            [R(2)] out int clientSessionHandle)
        {
            return CreateSession(isLight, namePtr, out serverSessionHandle, out clientSessionHandle);
        }

        public KernelResult CreateSession32(
            [R(2)] bool isLight,
            [R(3)] uint namePtr,
            [R(1)] out int serverSessionHandle,
            [R(2)] out int clientSessionHandle)
        {
            return CreateSession(isLight, namePtr, out serverSessionHandle, out clientSessionHandle);
        }

        private KernelResult CreateSession(
            bool    isLight,
            ulong   namePtr,
            out int serverSessionHandle,
            out int clientSessionHandle)
        {
            serverSessionHandle = 0;
            clientSessionHandle = 0;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KResourceLimit resourceLimit = currentProcess.ResourceLimit;

            KernelResult result = KernelResult.Success;

            if (resourceLimit != null && !resourceLimit.Reserve(LimitableResource.Session, 1))
            {
                return KernelResult.ResLimitExceeded;
            }

            if (isLight)
            {
                KLightSession session = new KLightSession(_system);

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
                KSession session = new KSession(_system);

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

        public KernelResult AcceptSession64([R(1)] int portHandle, [R(1)] out int sessionHandle)
        {
            return AcceptSession(portHandle, out sessionHandle);
        }

        public KernelResult AcceptSession32([R(1)] int portHandle, [R(1)] out int sessionHandle)
        {
            return AcceptSession(portHandle, out sessionHandle);
        }

        private KernelResult AcceptSession(int portHandle, out int sessionHandle)
        {
            sessionHandle = 0;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

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

        public KernelResult ReplyAndReceive64(
            [R(1)] ulong   handlesPtr,
            [R(2)] int     handlesCount,
            [R(3)] int     replyTargetHandle,
            [R(4)] long    timeout,
            [R(1)] out int handleIndex)
        {
            return ReplyAndReceive(handlesPtr, handlesCount, replyTargetHandle, timeout, out handleIndex);
        }

        public KernelResult ReplyAndReceive32(
            [R(0)] uint    timeoutLow,
            [R(1)] ulong   handlesPtr,
            [R(2)] int     handlesCount,
            [R(3)] int     replyTargetHandle,
            [R(4)] uint    timeoutHigh,
            [R(1)] out int handleIndex)
        {
            long timeout = (long)(timeoutLow | ((ulong)timeoutHigh << 32));

            return ReplyAndReceive(handlesPtr, handlesCount, replyTargetHandle, timeout, out handleIndex);
        }

        public KernelResult ReplyAndReceive(
            ulong   handlesPtr,
            int     handlesCount,
            int     replyTargetHandle,
            long    timeout,
            out int handleIndex)
        {
            handleIndex = 0;

            if ((uint)handlesCount > 0x40)
            {
                return KernelResult.MaximumExceeded;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

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

            if (!KernelTransfer.UserToKernelInt32Array(_system, handlesPtr, handles))
            {
                return KernelResult.UserCopyFailed;
            }

            KSynchronizationObject[] syncObjs = new KSynchronizationObject[handlesCount];

            for (int index = 0; index < handlesCount; index++)
            {
                KSynchronizationObject obj = currentProcess.HandleTable.GetObject<KSynchronizationObject>(handles[index]);

                if (obj == null)
                {
                    return KernelResult.InvalidHandle;
                }

                syncObjs[index] = obj;
            }

            KernelResult result;

            if (replyTargetHandle != 0)
            {
                KServerSession replyTarget = currentProcess.HandleTable.GetObject<KServerSession>(replyTargetHandle);

                if (replyTarget == null)
                {
                    return KernelResult.InvalidHandle;
                }

                result = replyTarget.Reply();

                if (result != KernelResult.Success)
                {
                    return result;
                }
            }

            while ((result = _system.Synchronization.WaitFor(syncObjs, timeout, out handleIndex)) == KernelResult.Success)
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

            return result;
        }

        public KernelResult CreatePort64(
            [R(2)] int     maxSessions,
            [R(3)] bool    isLight,
            [R(4)] ulong   namePtr,
            [R(1)] out int serverPortHandle,
            [R(2)] out int clientPortHandle)
        {
            return CreatePort(maxSessions, isLight, namePtr, out serverPortHandle, out clientPortHandle);
        }

        public KernelResult CreatePort32(
            [R(0)] uint    namePtr,
            [R(2)] int     maxSessions,
            [R(3)] bool    isLight,
            [R(1)] out int serverPortHandle,
            [R(2)] out int clientPortHandle)
        {
            return CreatePort(maxSessions, isLight, namePtr, out serverPortHandle, out clientPortHandle);
        }

        private KernelResult CreatePort(
            int     maxSessions,
            bool    isLight,
            ulong   namePtr,
            out int serverPortHandle,
            out int clientPortHandle)
        {
            serverPortHandle = clientPortHandle = 0;

            if (maxSessions < 1)
            {
                return KernelResult.MaximumExceeded;
            }

            KPort port = new KPort(_system, maxSessions, isLight, (long)namePtr);

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

        public KernelResult ManageNamedPort64([R(1)] ulong namePtr, [R(2)] int maxSessions, [R(1)] out int handle)
        {
            return ManageNamedPort(namePtr, maxSessions, out handle);
        }

        public KernelResult ManageNamedPort32([R(1)] uint namePtr, [R(2)] int maxSessions, [R(1)] out int handle)
        {
            return ManageNamedPort(namePtr, maxSessions, out handle);
        }

        private KernelResult ManageNamedPort(ulong namePtr, int maxSessions, out int handle)
        {
            handle = 0;

            if (!KernelTransfer.UserToKernelString(_system, namePtr, 12, out string name))
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

            KPort port = new KPort(_system, maxSessions, false, 0);

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

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

        public KernelResult ConnectToPort64([R(1)] int clientPortHandle, [R(1)] out int clientSessionHandle)
        {
            return ConnectToPort(clientPortHandle, out clientSessionHandle);
        }

        public KernelResult ConnectToPort32([R(1)] int clientPortHandle, [R(1)] out int clientSessionHandle)
        {
            return ConnectToPort(clientPortHandle, out clientSessionHandle);
        }

        private KernelResult ConnectToPort(int clientPortHandle, out int clientSessionHandle)
        {
            clientSessionHandle = 0;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

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
    }
}
