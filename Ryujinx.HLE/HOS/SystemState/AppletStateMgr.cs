using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletStateMgr
    {
        private ConcurrentQueue<MessageInfo> _messages;

        public FocusState FocusState { get; private set; }

        public KEvent MessageEvent { get; private set; }

        public AppletStateMgr(Horizon system)
        {
            _messages = new ConcurrentQueue<MessageInfo>();

            MessageEvent = new KEvent(system);
        }

        public void SetFocus(bool isFocused)
        {
            FocusState = isFocused
                ? FocusState.InFocus
                : FocusState.OutOfFocus;

            EnqueueMessage(MessageInfo.FocusStateChanged);
        }

        public void EnqueueMessage(MessageInfo message)
        {
            _messages.Enqueue(message);

            MessageEvent.ReadableEvent.Signal();
        }

        public bool TryDequeueMessage(out MessageInfo message)
        {
            if (_messages.Count < 2)
            {
                MessageEvent.ReadableEvent.Clear();
            }

            return _messages.TryDequeue(out message);
        }
    }
}