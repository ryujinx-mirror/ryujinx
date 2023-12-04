namespace Ryujinx.Horizon.Sdk.OsTypes
{
    class MultiWaitHolderOfHandle : MultiWaitHolder
    {
        private readonly int _handle;

        public override int Handle => _handle;

        public MultiWaitHolderOfHandle(int handle)
        {
            _handle = handle;
        }
    }
}
