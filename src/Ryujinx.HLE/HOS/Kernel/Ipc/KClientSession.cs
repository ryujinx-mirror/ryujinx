using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KClientSession : KSynchronizationObject
    {
        public KProcess CreatorProcess { get; }

        private readonly KSession _parent;

        public ChannelState State { get; set; }

        public KClientPort ParentPort { get; }

        public KClientSession(KernelContext context, KSession parent, KClientPort parentPort) : base(context)
        {
            _parent = parent;
            ParentPort = parentPort;

            parentPort?.IncrementReferenceCount();

            State = ChannelState.Open;

            CreatorProcess = KernelStatic.GetCurrentProcess();
            CreatorProcess.IncrementReferenceCount();
        }

        public Result SendSyncRequest(ulong customCmdBuffAddr = 0, ulong customCmdBuffSize = 0)
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            KSessionRequest request = new(currentThread, customCmdBuffAddr, customCmdBuffSize);

            KernelContext.CriticalSection.Enter();

            currentThread.SignaledObj = null;
            currentThread.ObjSyncResult = Result.Success;

            Result result = _parent.ServerSession.EnqueueRequest(request);

            KernelContext.CriticalSection.Leave();

            if (result == Result.Success)
            {
                result = currentThread.ObjSyncResult;
            }

            return result;
        }

        public Result SendAsyncRequest(KWritableEvent asyncEvent, ulong customCmdBuffAddr = 0, ulong customCmdBuffSize = 0)
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            KSessionRequest request = new(currentThread, customCmdBuffAddr, customCmdBuffSize, asyncEvent);

            KernelContext.CriticalSection.Enter();

            Result result = _parent.ServerSession.EnqueueRequest(request);

            KernelContext.CriticalSection.Leave();

            return result;
        }

        public void DisconnectFromPort()
        {
            if (ParentPort != null)
            {
                ParentPort.Disconnect();
                ParentPort.DecrementReferenceCount();
            }
        }

        protected override void Destroy()
        {
            _parent.DisconnectClient();
            _parent.DecrementReferenceCount();
        }
    }
}
