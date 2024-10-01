using Ryujinx.Memory;
using System;

namespace Ryujinx.Horizon.Common
{
    public interface ISyscallApi
    {
        Result SetHeapSize(out ulong address, ulong size);

        void SleepThread(long timeout);

        Result CloseHandle(int handle);

        Result WaitSynchronization(out int handleIndex, ReadOnlySpan<int> handles, long timeout);
        Result CancelSynchronization(int handle);

        Result GetProcessId(out ulong pid, int handle);

        Result ConnectToNamedPort(out int handle, string name);
        Result SendSyncRequest(int handle);
        Result CreateSession(out int serverSessionHandle, out int clientSessionHandle, bool isLight, string name);
        Result AcceptSession(out int sessionHandle, int portHandle);
        Result ReplyAndReceive(out int handleIndex, ReadOnlySpan<int> handles, int replyTargetHandle, long timeout);

        Result CreateEvent(out int writableHandle, out int readableHandle);
        Result SignalEvent(int handle);
        Result ClearEvent(int handle);
        Result ResetSignal(int handle);

        Result CreatePort(out int serverPortHandle, out int clientPortHandle, int maxSessions, bool isLight, string name);
        Result ManageNamedPort(out int handle, string name, int maxSessions);
        Result ConnectToPort(out int clientSessionHandle, int clientPortHandle);

        IExternalEvent GetExternalEvent(int handle);
        IVirtualMemoryManager GetMemoryManagerByProcessHandle(int handle);
        ulong GetTransferMemoryAddress(int handle);
    }
}
