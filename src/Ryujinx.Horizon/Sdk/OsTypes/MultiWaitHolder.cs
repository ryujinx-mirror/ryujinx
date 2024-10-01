namespace Ryujinx.Horizon.Sdk.OsTypes
{
    class MultiWaitHolder : MultiWaitHolderBase
    {
        public object UserData { get; set; }

        public void UnlinkFromMultiWaitHolder()
        {
            DebugUtil.Assert(IsLinked);

            MultiWait.UnlinkMultiWaitHolder(this);

            SetMultiWait(null);
        }
    }
}
