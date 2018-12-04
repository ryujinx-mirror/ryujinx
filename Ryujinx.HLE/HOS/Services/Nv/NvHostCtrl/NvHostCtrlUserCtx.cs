namespace Ryujinx.HLE.HOS.Services.Nv.NvHostCtrl
{
    class NvHostCtrlUserCtx
    {
        public const int LocksCount  = 16;
        public const int EventsCount = 64;

        public NvHostSyncpt Syncpt { get; }

        public NvHostEvent[] Events { get; }

        public NvHostCtrlUserCtx()
        {
            Syncpt = new NvHostSyncpt();

            Events = new NvHostEvent[EventsCount];
        }
    }
}