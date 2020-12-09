using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KServerSession : KSynchronizationObject
    {
        private static readonly MemoryState[] IpcMemoryStates = new MemoryState[]
        {
            MemoryState.IpcBuffer3,
            MemoryState.IpcBuffer0,
            MemoryState.IpcBuffer1,
            (MemoryState)0xfffce5d4 //This is invalid, shouldn't be accessed.
        };

        private struct Message
        {
            public ulong Address     { get; }
            public ulong DramAddress { get; }
            public ulong Size        { get; }
            public bool  IsCustom    { get; }

            public Message(KThread thread, ulong customCmdBuffAddress, ulong customCmdBuffSize)
            {
                IsCustom = customCmdBuffAddress != 0;

                if (IsCustom)
                {
                    Address = customCmdBuffAddress;
                    Size    = customCmdBuffSize;

                    KProcess process = thread.Owner;

                    DramAddress = process.MemoryManager.GetDramAddressFromVa(Address);
                }
                else
                {
                    Address     = thread.TlsAddress;
                    DramAddress = thread.TlsDramAddress;
                    Size        = 0x100;
                }
            }

            public Message(KSessionRequest request) : this(
                request.ClientThread,
                request.CustomCmdBuffAddr,
                request.CustomCmdBuffSize) { }
        }

        private struct MessageHeader
        {
            public uint Word0 { get; }
            public uint Word1 { get; }
            public uint Word2 { get; }

            public uint PointerBuffersCount  { get; }
            public uint SendBuffersCount     { get; }
            public uint ReceiveBuffersCount  { get; }
            public uint ExchangeBuffersCount { get; }

            public uint RawDataSizeInWords { get; }

            public uint ReceiveListType { get; }

            public uint MessageSizeInWords       { get; }
            public uint ReceiveListOffsetInWords { get; }
            public uint ReceiveListOffset        { get; }

            public bool HasHandles { get; }

            public bool HasPid { get; }

            public uint CopyHandlesCount { get; }
            public uint MoveHandlesCount { get; }

            public MessageHeader(uint word0, uint word1, uint word2)
            {
                Word0 = word0;
                Word1 = word1;
                Word2 = word2;

                HasHandles = word1 >> 31 != 0;

                uint handleDescSizeInWords = 0;

                if (HasHandles)
                {
                    uint pidSize = (word2 & 1) * 8;

                    HasPid = pidSize != 0;

                    CopyHandlesCount = (word2 >> 1) & 0xf;
                    MoveHandlesCount = (word2 >> 5) & 0xf;

                    handleDescSizeInWords = (pidSize + CopyHandlesCount * 4 + MoveHandlesCount * 4) / 4;
                }
                else
                {
                    HasPid = false;

                    CopyHandlesCount = 0;
                    MoveHandlesCount = 0;
                }

                PointerBuffersCount  = (word0 >> 16) & 0xf;
                SendBuffersCount     = (word0 >> 20) & 0xf;
                ReceiveBuffersCount  = (word0 >> 24) & 0xf;
                ExchangeBuffersCount =  word0 >> 28;

                uint pointerDescSizeInWords  = PointerBuffersCount  * 2;
                uint sendDescSizeInWords     = SendBuffersCount     * 3;
                uint receiveDescSizeInWords  = ReceiveBuffersCount  * 3;
                uint exchangeDescSizeInWords = ExchangeBuffersCount * 3;

                RawDataSizeInWords = word1 & 0x3ff;

                ReceiveListType = (word1 >> 10) & 0xf;

                ReceiveListOffsetInWords = (word1 >> 20) & 0x7ff;

                uint paddingSizeInWords = HasHandles ? 3u : 2u;

                MessageSizeInWords = pointerDescSizeInWords  +
                                     sendDescSizeInWords     +
                                     receiveDescSizeInWords  +
                                     exchangeDescSizeInWords +
                                     RawDataSizeInWords      +
                                     paddingSizeInWords      +
                                     handleDescSizeInWords;

                if (ReceiveListOffsetInWords == 0)
                {
                    ReceiveListOffsetInWords = MessageSizeInWords;
                }

                ReceiveListOffset = ReceiveListOffsetInWords * 4;
            }
        }

        private struct PointerBufferDesc
        {
            public uint ReceiveIndex { get; }

            public uint  BufferSize    { get; }
            public ulong BufferAddress { get; set; }

            public PointerBufferDesc(ulong dword)
            {
                ReceiveIndex = (uint)dword & 0xf;
                BufferSize   = (uint)dword >> 16;

                BufferAddress  = (dword >> 2)  & 0x70;
                BufferAddress |= (dword >> 12) & 0xf;

                BufferAddress = (BufferAddress << 32) | (dword >> 32);
            }

            public ulong Pack()
            {
                ulong dword = (ReceiveIndex & 0xf) | ((BufferSize & 0xffff) << 16);

                dword |=  BufferAddress << 32;
                dword |= (BufferAddress >> 20) & 0xf000;
                dword |= (BufferAddress >> 30) & 0xffc0;

                return dword;
            }
        }

        private KSession _parent;

        private LinkedList<KSessionRequest> _requests;

        private KSessionRequest _activeRequest;

        public KServerSession(KernelContext context, KSession parent) : base(context)
        {
            _parent = parent;

            _requests = new LinkedList<KSessionRequest>();
        }

        public KernelResult EnqueueRequest(KSessionRequest request)
        {
            if (_parent.ClientSession.State != ChannelState.Open)
            {
                return KernelResult.PortRemoteClosed;
            }

            if (request.AsyncEvent == null)
            {
                if (request.ClientThread.ShallBeTerminated ||
                    request.ClientThread.SchedFlags == ThreadSchedState.TerminationPending)
                {
                    return KernelResult.ThreadTerminating;
                }

                request.ClientThread.Reschedule(ThreadSchedState.Paused);
            }

            _requests.AddLast(request);

            if (_requests.Count == 1)
            {
                Signal();
            }

            return KernelResult.Success;
        }

        public KernelResult Receive(ulong customCmdBuffAddr = 0, ulong customCmdBuffSize = 0)
        {
            KThread  serverThread  = KernelStatic.GetCurrentThread();
            KProcess serverProcess = serverThread.Owner;

            KernelContext.CriticalSection.Enter();

            if (_parent.ClientSession.State != ChannelState.Open)
            {
                KernelContext.CriticalSection.Leave();

                return KernelResult.PortRemoteClosed;
            }

            if (_activeRequest != null || !DequeueRequest(out KSessionRequest request))
            {
                KernelContext.CriticalSection.Leave();

                return KernelResult.NotFound;
            }

            if (request.ClientThread == null)
            {
                KernelContext.CriticalSection.Leave();

                return KernelResult.PortRemoteClosed;
            }

            KThread  clientThread  = request.ClientThread;
            KProcess clientProcess = clientThread.Owner;

            KernelContext.CriticalSection.Leave();

            _activeRequest = request;

            request.ServerProcess = serverProcess;

            Message clientMsg = new Message(request);
            Message serverMsg = new Message(serverThread, customCmdBuffAddr, customCmdBuffSize);

            MessageHeader clientHeader = GetClientMessageHeader(clientMsg);
            MessageHeader serverHeader = GetServerMessageHeader(serverMsg);

            KernelResult serverResult = KernelResult.NotFound;
            KernelResult clientResult = KernelResult.Success;

            void CleanUpForError()
            {
                if (request.BufferDescriptorTable.UnmapServerBuffers(serverProcess.MemoryManager) == KernelResult.Success)
                {
                    request.BufferDescriptorTable.RestoreClientBuffers(clientProcess.MemoryManager);
                }

                CloseAllHandles(serverMsg, clientHeader, serverProcess);

                KernelContext.CriticalSection.Enter();

                _activeRequest = null;

                if (_requests.Count != 0)
                {
                    Signal();
                }

                KernelContext.CriticalSection.Leave();

                WakeClientThread(request, clientResult);
            }

            if (clientHeader.ReceiveListType < 2 &&
                clientHeader.ReceiveListOffset > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }
            else if (clientHeader.ReceiveListType == 2 &&
                     clientHeader.ReceiveListOffset + 8 > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }
            else if (clientHeader.ReceiveListType > 2 &&
                     clientHeader.ReceiveListType * 8 - 0x10 + clientHeader.ReceiveListOffset > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }

            if (clientHeader.ReceiveListOffsetInWords < clientHeader.MessageSizeInWords)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }

            if (clientHeader.MessageSizeInWords * 4 > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.CmdBufferTooSmall;
            }

            ulong[] receiveList = GetReceiveList(
                serverMsg,
                serverHeader.ReceiveListType,
                serverHeader.ReceiveListOffset);

            serverProcess.CpuMemory.Write(serverMsg.Address + 0, clientHeader.Word0);
            serverProcess.CpuMemory.Write(serverMsg.Address + 4, clientHeader.Word1);

            uint offset;

            // Copy handles.
            if (clientHeader.HasHandles)
            {
                if (clientHeader.MoveHandlesCount != 0)
                {
                    CleanUpForError();

                    return KernelResult.InvalidCombination;
                }

                serverProcess.CpuMemory.Write(serverMsg.Address + 8, clientHeader.Word2);

                offset = 3;

                if (clientHeader.HasPid)
                {
                    serverProcess.CpuMemory.Write(serverMsg.Address + offset * 4, clientProcess.Pid);

                    offset += 2;
                }

                for (int index = 0; index < clientHeader.CopyHandlesCount; index++)
                {
                    int newHandle = 0;
                    int handle = KernelContext.Memory.Read<int>(clientMsg.DramAddress + offset * 4);

                    if (clientResult == KernelResult.Success && handle != 0)
                    {
                        clientResult = GetCopyObjectHandle(clientThread, serverProcess, handle, out newHandle);
                    }

                    serverProcess.CpuMemory.Write(serverMsg.Address + offset * 4, newHandle);

                    offset++;
                }

                for (int index = 0; index < clientHeader.MoveHandlesCount; index++)
                {
                    int newHandle = 0;
                    int handle = KernelContext.Memory.Read<int>(clientMsg.DramAddress + offset * 4);

                    if (handle != 0)
                    {
                        if (clientResult == KernelResult.Success)
                        {
                            clientResult = GetMoveObjectHandle(clientProcess, serverProcess, handle, out newHandle);
                        }
                        else
                        {
                            clientProcess.HandleTable.CloseHandle(handle);
                        }
                    }

                    serverProcess.CpuMemory.Write(serverMsg.Address + offset * 4, newHandle);

                    offset++;
                }

                if (clientResult != KernelResult.Success)
                {
                    CleanUpForError();

                    return serverResult;
                }
            }
            else
            {
                offset = 2;
            }

            // Copy pointer/receive list buffers.
            uint recvListDstOffset = 0;

            for (int index = 0; index < clientHeader.PointerBuffersCount; index++)
            {
                ulong pointerDesc = KernelContext.Memory.Read<ulong>(clientMsg.DramAddress + offset * 4);

                PointerBufferDesc descriptor = new PointerBufferDesc(pointerDesc);

                if (descriptor.BufferSize != 0)
                {
                    clientResult = GetReceiveListAddress(
                        descriptor,
                        serverMsg,
                        serverHeader.ReceiveListType,
                        clientHeader.MessageSizeInWords,
                        receiveList,
                        ref       recvListDstOffset,
                        out ulong recvListBufferAddress);

                    if (clientResult != KernelResult.Success)
                    {
                        CleanUpForError();

                        return serverResult;
                    }

                    clientResult = clientProcess.MemoryManager.CopyDataToCurrentProcess(
                        recvListBufferAddress,
                        descriptor.BufferSize,
                        descriptor.BufferAddress,
                        MemoryState.IsPoolAllocated,
                        MemoryState.IsPoolAllocated,
                        KMemoryPermission.Read,
                        MemoryAttribute.Uncached,
                        MemoryAttribute.None);

                    if (clientResult != KernelResult.Success)
                    {
                        CleanUpForError();

                        return serverResult;
                    }

                    descriptor.BufferAddress = recvListBufferAddress;
                }
                else
                {
                    descriptor.BufferAddress = 0;
                }

                serverProcess.CpuMemory.Write(serverMsg.Address + offset * 4, descriptor.Pack());

                offset += 2;
            }

            // Copy send, receive and exchange buffers.
            uint totalBuffersCount =
                clientHeader.SendBuffersCount    +
                clientHeader.ReceiveBuffersCount +
                clientHeader.ExchangeBuffersCount;

            for (int index = 0; index < totalBuffersCount; index++)
            {
                ulong clientDescAddress = clientMsg.DramAddress + offset * 4;

                uint descWord0 = KernelContext.Memory.Read<uint>(clientDescAddress + 0);
                uint descWord1 = KernelContext.Memory.Read<uint>(clientDescAddress + 4);
                uint descWord2 = KernelContext.Memory.Read<uint>(clientDescAddress + 8);

                bool isSendDesc     = index <  clientHeader.SendBuffersCount;
                bool isExchangeDesc = index >= clientHeader.SendBuffersCount + clientHeader.ReceiveBuffersCount;

                bool notReceiveDesc = isSendDesc || isExchangeDesc;
                bool isReceiveDesc  = !notReceiveDesc;

                KMemoryPermission permission = index >= clientHeader.SendBuffersCount
                    ? KMemoryPermission.ReadAndWrite
                    : KMemoryPermission.Read;

                uint sizeHigh4 = (descWord2 >> 24) & 0xf;

                ulong bufferSize = descWord0 | (ulong)sizeHigh4 << 32;

                ulong dstAddress = 0;

                if (bufferSize != 0)
                {
                    ulong bufferAddress;

                    bufferAddress  =   descWord2 >> 28;
                    bufferAddress |= ((descWord2 >> 2) & 7) << 4;

                    bufferAddress = (bufferAddress << 32) | descWord1;

                    MemoryState state = IpcMemoryStates[(descWord2 + 1) & 3];

                    clientResult = serverProcess.MemoryManager.MapBufferFromClientProcess(
                        bufferSize,
                        bufferAddress,
                        clientProcess.MemoryManager,
                        permission,
                        state,
                        notReceiveDesc,
                        out dstAddress);

                    if (clientResult != KernelResult.Success)
                    {
                        CleanUpForError();

                        return serverResult;
                    }

                    if (isSendDesc)
                    {
                        clientResult = request.BufferDescriptorTable.AddSendBuffer(bufferAddress, dstAddress, bufferSize, state);
                    }
                    else if (isReceiveDesc)
                    {
                        clientResult = request.BufferDescriptorTable.AddReceiveBuffer(bufferAddress, dstAddress, bufferSize, state);
                    }
                    else /* if (isExchangeDesc) */
                    {
                        clientResult = request.BufferDescriptorTable.AddExchangeBuffer(bufferAddress, dstAddress, bufferSize, state);
                    }

                    if (clientResult != KernelResult.Success)
                    {
                        CleanUpForError();

                        return serverResult;
                    }
                }

                descWord1 = (uint)dstAddress;

                descWord2 &= 3;

                descWord2 |= sizeHigh4 << 24;

                descWord2 |= (uint)(dstAddress >> 34) & 0x3ffffffc;
                descWord2 |= (uint)(dstAddress >> 4)  & 0xf0000000;

                ulong serverDescAddress = serverMsg.Address + offset * 4;

                serverProcess.CpuMemory.Write(serverDescAddress + 0, descWord0);
                serverProcess.CpuMemory.Write(serverDescAddress + 4, descWord1);
                serverProcess.CpuMemory.Write(serverDescAddress + 8, descWord2);

                offset += 3;
            }

            // Copy raw data.
            if (clientHeader.RawDataSizeInWords != 0)
            {
                ulong copySrc = clientMsg.Address + offset * 4;
                ulong copyDst = serverMsg.Address + offset * 4;

                ulong copySize = clientHeader.RawDataSizeInWords * 4;

                if (serverMsg.IsCustom || clientMsg.IsCustom)
                {
                    KMemoryPermission permission = clientMsg.IsCustom
                        ? KMemoryPermission.None
                        : KMemoryPermission.Read;

                    clientResult = clientProcess.MemoryManager.CopyDataToCurrentProcess(
                        copyDst,
                        copySize,
                        copySrc,
                        MemoryState.IsPoolAllocated,
                        MemoryState.IsPoolAllocated,
                        permission,
                        MemoryAttribute.Uncached,
                        MemoryAttribute.None);
                }
                else
                {
                    copySrc = clientProcess.MemoryManager.GetDramAddressFromVa(copySrc);
                    copyDst = serverProcess.MemoryManager.GetDramAddressFromVa(copyDst);

                    KernelContext.Memory.Copy(copyDst, copySrc, copySize);
                }

                if (clientResult != KernelResult.Success)
                {
                    CleanUpForError();

                    return serverResult;
                }
            }

            return KernelResult.Success;
        }

        public KernelResult Reply(ulong customCmdBuffAddr = 0, ulong customCmdBuffSize = 0)
        {
            KThread  serverThread  = KernelStatic.GetCurrentThread();
            KProcess serverProcess = serverThread.Owner;

            KernelContext.CriticalSection.Enter();

            if (_activeRequest == null)
            {
                KernelContext.CriticalSection.Leave();

                return KernelResult.InvalidState;
            }

            KSessionRequest request = _activeRequest;

            _activeRequest = null;

            if (_requests.Count != 0)
            {
                Signal();
            }

            KernelContext.CriticalSection.Leave();

            KThread  clientThread  = request.ClientThread;
            KProcess clientProcess = clientThread.Owner;

            Message clientMsg = new Message(request);
            Message serverMsg = new Message(serverThread, customCmdBuffAddr, customCmdBuffSize);

            MessageHeader clientHeader = GetClientMessageHeader(clientMsg);
            MessageHeader serverHeader = GetServerMessageHeader(serverMsg);

            KernelResult clientResult = KernelResult.Success;
            KernelResult serverResult = KernelResult.Success;

            void CleanUpForError()
            {
                CloseAllHandles(clientMsg, serverHeader, clientProcess);

                FinishRequest(request, clientResult);
            }

            if (clientHeader.ReceiveListType < 2 &&
                clientHeader.ReceiveListOffset > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }
            else if (clientHeader.ReceiveListType == 2 &&
                     clientHeader.ReceiveListOffset + 8 > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }
            else if (clientHeader.ReceiveListType > 2 &&
                     clientHeader.ReceiveListType * 8 - 0x10 + clientHeader.ReceiveListOffset > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }

            if (clientHeader.ReceiveListOffsetInWords < clientHeader.MessageSizeInWords)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }

            if (serverHeader.MessageSizeInWords * 4 > clientMsg.Size)
            {
                CleanUpForError();

                return KernelResult.CmdBufferTooSmall;
            }

            if (serverHeader.SendBuffersCount     != 0 ||
                serverHeader.ReceiveBuffersCount  != 0 ||
                serverHeader.ExchangeBuffersCount != 0)
            {
                CleanUpForError();

                return KernelResult.InvalidCombination;
            }

            // Read receive list.
            ulong[] receiveList = GetReceiveList(
                clientMsg,
                clientHeader.ReceiveListType,
                clientHeader.ReceiveListOffset);

            // Copy receive and exchange buffers.
            clientResult = request.BufferDescriptorTable.CopyBuffersToClient(clientProcess.MemoryManager);

            if (clientResult != KernelResult.Success)
            {
                CleanUpForError();

                return serverResult;
            }

            // Copy header.
            KernelContext.Memory.Write(clientMsg.DramAddress + 0, serverHeader.Word0);
            KernelContext.Memory.Write(clientMsg.DramAddress + 4, serverHeader.Word1);

            // Copy handles.
            uint offset;

            if (serverHeader.HasHandles)
            {
                offset = 3;

                KernelContext.Memory.Write(clientMsg.DramAddress + 8, serverHeader.Word2);

                if (serverHeader.HasPid)
                {
                    KernelContext.Memory.Write(clientMsg.DramAddress + offset * 4, serverProcess.Pid);

                    offset += 2;
                }

                for (int index = 0; index < serverHeader.CopyHandlesCount; index++)
                {
                    int newHandle = 0;

                    int handle = serverProcess.CpuMemory.Read<int>(serverMsg.Address + offset * 4);

                    if (handle != 0)
                    {
                        GetCopyObjectHandle(serverThread, clientProcess, handle, out newHandle);
                    }

                    KernelContext.Memory.Write(clientMsg.DramAddress + offset * 4, newHandle);

                    offset++;
                }

                for (int index = 0; index < serverHeader.MoveHandlesCount; index++)
                {
                    int newHandle = 0;

                    int handle = serverProcess.CpuMemory.Read<int>(serverMsg.Address + offset * 4);

                    if (handle != 0)
                    {
                        if (clientResult == KernelResult.Success)
                        {
                            clientResult = GetMoveObjectHandle(serverProcess, clientProcess, handle, out newHandle);
                        }
                        else
                        {
                            serverProcess.HandleTable.CloseHandle(handle);
                        }
                    }

                    KernelContext.Memory.Write(clientMsg.DramAddress + offset * 4, newHandle);

                    offset++;
                }
            }
            else
            {
                offset = 2;
            }

            // Copy pointer/receive list buffers.
            uint recvListDstOffset = 0;

            for (int index = 0; index < serverHeader.PointerBuffersCount; index++)
            {
                ulong pointerDesc = serverProcess.CpuMemory.Read<ulong>(serverMsg.Address + offset * 4);

                PointerBufferDesc descriptor = new PointerBufferDesc(pointerDesc);

                ulong recvListBufferAddress = 0;

                if (descriptor.BufferSize != 0)
                {
                    clientResult = GetReceiveListAddress(
                        descriptor,
                        clientMsg,
                        clientHeader.ReceiveListType,
                        serverHeader.MessageSizeInWords,
                        receiveList,
                        ref recvListDstOffset,
                        out recvListBufferAddress);

                    if (clientResult != KernelResult.Success)
                    {
                        CleanUpForError();

                        return serverResult;
                    }

                    clientResult = clientProcess.MemoryManager.CopyDataFromCurrentProcess(
                        recvListBufferAddress,
                        descriptor.BufferSize,
                        MemoryState.IsPoolAllocated,
                        MemoryState.IsPoolAllocated,
                        KMemoryPermission.Read,
                        MemoryAttribute.Uncached,
                        MemoryAttribute.None,
                        descriptor.BufferAddress);

                    if (clientResult != KernelResult.Success)
                    {
                        CleanUpForError();

                        return serverResult;
                    }
                }

                ulong dstDescAddress = clientMsg.DramAddress + offset * 4;

                ulong clientPointerDesc =
                    (recvListBufferAddress << 32) |
                    ((recvListBufferAddress >> 20) & 0xf000) |
                    ((recvListBufferAddress >> 30) & 0xffc0);

                clientPointerDesc |= pointerDesc & 0xffff000f;

                KernelContext.Memory.Write(dstDescAddress + 0, clientPointerDesc);

                offset += 2;
            }

            // Set send, receive and exchange buffer descriptors to zero.
            uint totalBuffersCount =
                serverHeader.SendBuffersCount    +
                serverHeader.ReceiveBuffersCount +
                serverHeader.ExchangeBuffersCount;

            for (int index = 0; index < totalBuffersCount; index++)
            {
                ulong dstDescAddress = clientMsg.DramAddress + offset * 4;

                KernelContext.Memory.Write(dstDescAddress + 0, 0);
                KernelContext.Memory.Write(dstDescAddress + 4, 0);
                KernelContext.Memory.Write(dstDescAddress + 8, 0);

                offset += 3;
            }

            // Copy raw data.
            if (serverHeader.RawDataSizeInWords != 0)
            {
                ulong copyDst = clientMsg.Address + offset * 4;
                ulong copySrc = serverMsg.Address + offset * 4;

                ulong copySize = serverHeader.RawDataSizeInWords * 4;

                if (serverMsg.IsCustom || clientMsg.IsCustom)
                {
                    KMemoryPermission permission = clientMsg.IsCustom
                        ? KMemoryPermission.None
                        : KMemoryPermission.Read;

                    clientResult = clientProcess.MemoryManager.CopyDataFromCurrentProcess(
                        copyDst,
                        copySize,
                        MemoryState.IsPoolAllocated,
                        MemoryState.IsPoolAllocated,
                        permission,
                        MemoryAttribute.Uncached,
                        MemoryAttribute.None,
                        copySrc);
                }
                else
                {
                    copyDst = clientProcess.MemoryManager.GetDramAddressFromVa(copyDst);
                    copySrc = serverProcess.MemoryManager.GetDramAddressFromVa(copySrc);

                    KernelContext.Memory.Copy(copyDst, copySrc, copySize);
                }
            }

            // Unmap buffers from server.
            FinishRequest(request, clientResult);

            return serverResult;
        }

        private MessageHeader GetClientMessageHeader(Message clientMsg)
        {
            uint word0 = KernelContext.Memory.Read<uint>(clientMsg.DramAddress + 0);
            uint word1 = KernelContext.Memory.Read<uint>(clientMsg.DramAddress + 4);
            uint word2 = KernelContext.Memory.Read<uint>(clientMsg.DramAddress + 8);

            return new MessageHeader(word0, word1, word2);
        }

        private MessageHeader GetServerMessageHeader(Message serverMsg)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            uint word0 = currentProcess.CpuMemory.Read<uint>(serverMsg.Address + 0);
            uint word1 = currentProcess.CpuMemory.Read<uint>(serverMsg.Address + 4);
            uint word2 = currentProcess.CpuMemory.Read<uint>(serverMsg.Address + 8);

            return new MessageHeader(word0, word1, word2);
        }

        private KernelResult GetCopyObjectHandle(KThread srcThread, KProcess dstProcess, int srcHandle, out int dstHandle)
        {
            dstHandle = 0;

            KProcess srcProcess = srcThread.Owner;

            KAutoObject obj;

            if (srcHandle == KHandleTable.SelfProcessHandle)
            {
                obj = srcProcess;
            }
            else if (srcHandle == KHandleTable.SelfThreadHandle)
            {
                obj = srcThread;
            }
            else
            {
                obj = srcProcess.HandleTable.GetObject<KAutoObject>(srcHandle);
            }

            if (obj != null)
            {
                return dstProcess.HandleTable.GenerateHandle(obj, out dstHandle);
            }
            else
            {
                return KernelResult.InvalidHandle;
            }
        }

        private KernelResult GetMoveObjectHandle(KProcess srcProcess, KProcess dstProcess, int srcHandle, out int dstHandle)
        {
            dstHandle = 0;

            KAutoObject obj = srcProcess.HandleTable.GetObject<KAutoObject>(srcHandle);

            if (obj != null)
            {
                KernelResult result = dstProcess.HandleTable.GenerateHandle(obj, out dstHandle);

                srcProcess.HandleTable.CloseHandle(srcHandle);

                return result;
            }
            else
            {
                return KernelResult.InvalidHandle;
            }
        }

        private ulong[] GetReceiveList(Message message, uint recvListType, uint recvListOffset)
        {
            int recvListSize = 0;

            if (recvListType >= 3)
            {
                recvListSize = (int)recvListType - 2;
            }
            else if (recvListType == 2)
            {
                recvListSize = 1;
            }

            ulong[] receiveList = new ulong[recvListSize];

            ulong recvListAddress = message.DramAddress + recvListOffset;

            for (int index = 0; index < recvListSize; index++)
            {
                receiveList[index] = KernelContext.Memory.Read<ulong>(recvListAddress + (ulong)index * 8);
            }

            return receiveList;
        }

        private KernelResult GetReceiveListAddress(
            PointerBufferDesc descriptor,
            Message           message,
            uint              recvListType,
            uint              messageSizeInWords,
            ulong[]           receiveList,
            ref uint          dstOffset,
            out ulong         address)
        {
            ulong recvListBufferAddress = address = 0;

            if (recvListType == 0)
            {
                return KernelResult.OutOfResource;
            }
            else if (recvListType == 1 || recvListType == 2)
            {
                ulong recvListBaseAddr;
                ulong recvListEndAddr;

                if (recvListType == 1)
                {
                    recvListBaseAddr = message.Address + messageSizeInWords * 4;
                    recvListEndAddr  = message.Address + message.Size;
                }
                else /* if (recvListType == 2) */
                {
                    ulong packed = receiveList[0];

                    recvListBaseAddr = packed & 0x7fffffffff;

                    uint size = (uint)(packed >> 48);

                    if (size == 0)
                    {
                        return KernelResult.OutOfResource;
                    }

                    recvListEndAddr = recvListBaseAddr + size;
                }

                recvListBufferAddress = BitUtils.AlignUp(recvListBaseAddr + dstOffset, 0x10);

                ulong endAddress = recvListBufferAddress + descriptor.BufferSize;

                dstOffset = (uint)endAddress - (uint)recvListBaseAddr;

                if (recvListBufferAddress + descriptor.BufferSize <= recvListBufferAddress ||
                    recvListBufferAddress + descriptor.BufferSize >  recvListEndAddr)
                {
                    return KernelResult.OutOfResource;
                }
            }
            else /* if (recvListType > 2) */
            {
                if (descriptor.ReceiveIndex >= receiveList.Length)
                {
                    return KernelResult.OutOfResource;
                }

                ulong packed = receiveList[descriptor.ReceiveIndex];

                recvListBufferAddress = packed & 0x7fffffffff;

                uint size = (uint)(packed >> 48);

                if (recvListBufferAddress == 0 || size == 0 || size < descriptor.BufferSize)
                {
                    return KernelResult.OutOfResource;
                }
            }

            address = recvListBufferAddress;

            return KernelResult.Success;
        }

        private void CloseAllHandles(Message message, MessageHeader header, KProcess process)
        {
            if (header.HasHandles)
            {
                uint totalHandeslCount = header.CopyHandlesCount + header.MoveHandlesCount;

                uint offset = 3;

                if (header.HasPid)
                {
                    process.CpuMemory.Write(message.Address + offset * 4, 0L);

                    offset += 2;
                }

                for (int index = 0; index < totalHandeslCount; index++)
                {
                    int handle = process.CpuMemory.Read<int>(message.Address + offset * 4);

                    if (handle != 0)
                    {
                        process.HandleTable.CloseHandle(handle);

                        process.CpuMemory.Write(message.Address + offset * 4, 0);
                    }

                    offset++;
                }
            }
        }

        public override bool IsSignaled()
        {
            if (_parent.ClientSession.State != ChannelState.Open)
            {
                return true;
            }

            return _requests.Count != 0 && _activeRequest == null;
        }

        protected override void Destroy()
        {
            _parent.DisconnectServer();

            CancelAllRequestsServerDisconnected();

            _parent.DecrementReferenceCount();
        }

        private void CancelAllRequestsServerDisconnected()
        {
            foreach (KSessionRequest request in IterateWithRemovalOfAllRequests())
            {
                FinishRequest(request, KernelResult.PortRemoteClosed);
            }
        }

        public void CancelAllRequestsClientDisconnected()
        {
            foreach (KSessionRequest request in IterateWithRemovalOfAllRequests())
            {
                if (request.ClientThread.ShallBeTerminated ||
                    request.ClientThread.SchedFlags == ThreadSchedState.TerminationPending)
                {
                    continue;
                }

                // Client sessions can only be disconnected on async requests (because
                // the client would be otherwise blocked waiting for the response), so
                // we only need to handle the async case here.
                if (request.AsyncEvent != null)
                {
                    SendResultToAsyncRequestClient(request, KernelResult.PortRemoteClosed);
                }
            }

            WakeServerThreads(KernelResult.PortRemoteClosed);
        }

        private IEnumerable<KSessionRequest> IterateWithRemovalOfAllRequests()
        {
            KernelContext.CriticalSection.Enter();

            if (_activeRequest != null)
            {
                KSessionRequest request = _activeRequest;

                _activeRequest = null;

                KernelContext.CriticalSection.Leave();

                yield return request;
            }
            else
            {
                KernelContext.CriticalSection.Leave();
            }

            while (DequeueRequest(out KSessionRequest request))
            {
                yield return request;
            }
        }

        private bool DequeueRequest(out KSessionRequest request)
        {
            request = null;

            KernelContext.CriticalSection.Enter();

            bool hasRequest = _requests.First != null;

            if (hasRequest)
            {
                request = _requests.First.Value;

                _requests.RemoveFirst();
            }

            KernelContext.CriticalSection.Leave();

            return hasRequest;
        }

        private void FinishRequest(KSessionRequest request, KernelResult result)
        {
            KProcess clientProcess = request.ClientThread.Owner;
            KProcess serverProcess = request.ServerProcess;

            KernelResult unmapResult = KernelResult.Success;

            if (serverProcess != null)
            {
                unmapResult = request.BufferDescriptorTable.UnmapServerBuffers(serverProcess.MemoryManager);
            }

            if (unmapResult == KernelResult.Success)
            {
                request.BufferDescriptorTable.RestoreClientBuffers(clientProcess.MemoryManager);
            }

            WakeClientThread(request, result);
        }

        private void WakeClientThread(KSessionRequest request, KernelResult result)
        {
            // Wait client thread waiting for a response for the given request.
            if (request.AsyncEvent != null)
            {
                SendResultToAsyncRequestClient(request, result);
            }
            else
            {
                KernelContext.CriticalSection.Enter();

                WakeAndSetResult(request.ClientThread, result);

                KernelContext.CriticalSection.Leave();
            }
        }

        private void SendResultToAsyncRequestClient(KSessionRequest request, KernelResult result)
        {
            KProcess clientProcess = request.ClientThread.Owner;

            if (result != KernelResult.Success)
            {
                ulong address = clientProcess.MemoryManager.GetDramAddressFromVa(request.CustomCmdBuffAddr);

                KernelContext.Memory.Write<ulong>(address, 0);
                KernelContext.Memory.Write(address + 8, (int)result);
            }

            clientProcess.MemoryManager.UnborrowIpcBuffer(request.CustomCmdBuffAddr, request.CustomCmdBuffSize);

            request.AsyncEvent.Signal();
        }

        private void WakeServerThreads(KernelResult result)
        {
            // Wake all server threads waiting for requests.
            KernelContext.CriticalSection.Enter();

            foreach (KThread thread in WaitingThreads)
            {
                WakeAndSetResult(thread, result);
            }

            KernelContext.CriticalSection.Leave();
        }

        private void WakeAndSetResult(KThread thread, KernelResult result)
        {
            if ((thread.SchedFlags & ThreadSchedState.LowMask) == ThreadSchedState.Paused)
            {
                thread.SignaledObj   = null;
                thread.ObjSyncResult = result;

                thread.Reschedule(ThreadSchedState.Running);
            }
        }
    }
}