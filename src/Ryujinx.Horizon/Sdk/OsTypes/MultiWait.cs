using Ryujinx.Horizon.Sdk.OsTypes.Impl;
using System.Collections.Generic;

namespace Ryujinx.Horizon.Sdk.OsTypes
{
    class MultiWait
    {
        private readonly MultiWaitImpl _impl;

        public IEnumerable<MultiWaitHolderBase> MultiWaits => _impl.MultiWaits;

        public MultiWait()
        {
            _impl = new MultiWaitImpl();
        }

        public void LinkMultiWaitHolder(MultiWaitHolderBase multiWaitHolder)
        {
            DebugUtil.Assert(!multiWaitHolder.IsLinked);

            _impl.LinkMultiWaitHolder(multiWaitHolder);

            multiWaitHolder.SetMultiWait(_impl);
        }

        public void MoveAllFrom(MultiWait other)
        {
            _impl.MoveAllFrom(other._impl);
        }

        public MultiWaitHolder WaitAny()
        {
            return (MultiWaitHolder)_impl.WaitAnyImpl(true, -1L);
        }

        public MultiWaitHolder TryWaitAny()
        {
            return (MultiWaitHolder)_impl.WaitAnyImpl(false, 0);
        }

        public MultiWaitHolder TimedWaitAny(long timeout)
        {
            return (MultiWaitHolder)_impl.WaitAnyImpl(false, timeout);
        }
    }
}
