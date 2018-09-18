using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Am;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletStateMgr
    {
        private ConcurrentQueue<MessageInfo> Messages;

        public FocusState FocusState { get; private set; }

        public KEvent MessageEvent { get; private set; }

        public AppletStateMgr(Horizon System)
        {
            Messages = new ConcurrentQueue<MessageInfo>();

            MessageEvent = new KEvent(System);
        }

        public void SetFocus(bool IsFocused)
        {
            FocusState = IsFocused
                ? FocusState.InFocus
                : FocusState.OutOfFocus;

            EnqueueMessage(MessageInfo.FocusStateChanged);
        }

        public void EnqueueMessage(MessageInfo Message)
        {
            Messages.Enqueue(Message);

            MessageEvent.Signal();
        }

        public bool TryDequeueMessage(out MessageInfo Message)
        {
            if (Messages.Count < 2)
            {
                MessageEvent.Reset();
            }

            return Messages.TryDequeue(out Message);
        }
    }
}