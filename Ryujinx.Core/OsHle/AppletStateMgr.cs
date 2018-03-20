using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Services.Am;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.Core.OsHle
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

            MessageEvent.Handle.Set();
        }

        public bool TryDequeueMessage(out MessageInfo Message)
        {
            if (Messages.Count < 2)
            {
                MessageEvent.Handle.Reset();
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