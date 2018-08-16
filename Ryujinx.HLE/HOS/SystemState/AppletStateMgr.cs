using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Am;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletStateMgr : IDisposable
    {
        private ConcurrentQueue<MessageInfo> Messages;

        public FocusState FocusState { get; private set; }

        public KEvent MessageEvent { get; private set; }

        public AppletStateMgr()
        {
            Messages = new ConcurrentQueue<MessageInfo>();

            MessageEvent = new KEvent();
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

            MessageEvent.WaitEvent.Set();
        }

        public bool TryDequeueMessage(out MessageInfo Message)
        {
            if (Messages.Count < 2)
            {
                MessageEvent.WaitEvent.Reset();
            }

            return Messages.TryDequeue(out Message);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                MessageEvent.Dispose();
            }
        }
    }
}