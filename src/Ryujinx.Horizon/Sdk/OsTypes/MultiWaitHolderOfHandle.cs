namespace Ryujinx.Horizon.Sdk.OsTypes
{
    class MultiWaitHolderOfHandle : MultiWaitHolder
    {
        private int _handle;

        public override int Handle => _handle;

        public MultiWaitHolderOfHandle(int handle)
        {
            _handle = handle;
        }
    }
}
