using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KClientSession : KSynchronizationObject
    {
        public KProcess CreatorProcess { get; }

        private KSession _parent;

        public ChannelState State { get; set; }

        // TODO: Remove that, we need it for now to allow HLE
        // services implementation to work with the new IPC system.
        public IpcService Service { get; set; }

        public KClientSession(KernelContext context, KSession parent) : base(context)
        {
            _parent = parent;

            State = ChannelState.Open;

            CreatorProcess = context.Scheduler.GetCurrentProcess();

            CreatorProcess.IncrementReferenceCount();
        }

        public KernelResult SendSyncRequest(ulong customCmdBuffAddr = 0, ulong customCmdBuffSize = 0)
        {
            KThread currentThread = KernelContext.Scheduler.GetCurrentThread();

            KSessionRequest request = new KSessionRequest(currentThread, customCmdBuffAddr, customCmdBuffSize);

            KernelContext.CriticalSection.Enter();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.Success;

            KernelResult result = _parent.ServerSession.EnqueueRequest(request);

            KernelContext.CriticalSection.Leave();

            if (result == KernelResult.Success)
            {
                result = currentThread.ObjSyncResult;
            }

            return result;
        }

        protected override void Destroy()
        {
            _parent.DisconnectClient();
            _parent.DecrementReferenceCount();
        }
    }
}