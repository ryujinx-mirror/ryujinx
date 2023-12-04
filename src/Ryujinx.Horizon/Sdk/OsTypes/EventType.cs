using System.Collections.Generic;

namespace Ryujinx.Horizon.Sdk.OsTypes
{
    struct EventType
    {
        public LinkedList<MultiWaitHolderBase> MultiWaitHolders;
        public bool Signaled;
        public bool InitiallySignaled;
        public EventClearMode ClearMode;
        public InitializationState State;
        public ulong BroadcastCounter;
        public object Lock;
    }
}
