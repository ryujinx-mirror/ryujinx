using System.Collections.Generic;

namespace Ryujinx.Horizon.Sdk.OsTypes
{
    class MultiWaitHolderOfEvent : MultiWaitHolder
    {
        private readonly Event _event;
        private LinkedListNode<MultiWaitHolderBase> _node;

        public override TriBool Signaled
        {
            get
            {
                lock (_event.EventLock)
                {
                    return _event.IsSignaledThreadUnsafe();
                }
            }
        }

        public MultiWaitHolderOfEvent(Event evnt)
        {
            _event = evnt;
        }

        public override TriBool LinkToObjectList()
        {
            lock (_event.EventLock)
            {
                _node = _event.MultiWaitHolders.AddLast(this);

                return _event.IsSignaledThreadUnsafe();
            }
        }

        public override void UnlinkFromObjectList()
        {
            lock (_event.EventLock)
            {
                _event.MultiWaitHolders.Remove(_node);
                _node = null;
            }
        }
    }
}
