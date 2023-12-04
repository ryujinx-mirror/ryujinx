using Ryujinx.Horizon.Sdk.OsTypes.Impl;

namespace Ryujinx.Horizon.Sdk.OsTypes
{
    class MultiWaitHolderBase
    {
        protected MultiWaitImpl MultiWait;

        public bool IsLinked => MultiWait != null;

        public virtual TriBool Signaled => TriBool.False;

        public virtual int Handle => 0;

        public void SetMultiWait(MultiWaitImpl multiWait)
        {
            MultiWait = multiWait;
        }

        public MultiWaitImpl GetMultiWait()
        {
            return MultiWait;
        }

        public virtual TriBool LinkToObjectList()
        {
            return TriBool.Undefined;
        }

        public virtual void UnlinkFromObjectList()
        {
        }

        public virtual long GetAbsoluteTimeToWakeup()
        {
            return long.MaxValue;
        }
    }
}
