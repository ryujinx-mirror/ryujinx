using System.Diagnostics.Tracing;

namespace ARMeilleure.Diagnostics.EventSources
{
    [EventSource(Name = "ARMeilleure")]
    class AddressTableEventSource : EventSource
    {
        public static readonly AddressTableEventSource Log = new();

        private ulong _size;
        private ulong _leafSize;
        private PollingCounter _sizeCounter;
        private PollingCounter _leafSizeCounter;

        public AddressTableEventSource()
        {
            _sizeCounter = new PollingCounter("addr-tab-alloc", this, () => _size / 1024d / 1024d)
            {
                DisplayName = "AddressTable Total Bytes Allocated",
                DisplayUnits = "MB"
            };

            _leafSizeCounter = new PollingCounter("addr-tab-leaf-alloc", this, () => _leafSize / 1024d / 1024d)
            {
                DisplayName = "AddressTable Total Leaf Bytes Allocated",
                DisplayUnits = "MB"
            };
        }

        public void Allocated(int bytes, bool leaf)
        {
            _size += (uint)bytes;

            if (leaf)
            {
                _leafSize += (uint)bytes;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _leafSizeCounter.Dispose();
            _leafSizeCounter = null;

            _sizeCounter.Dispose();
            _sizeCounter = null;

            base.Dispose(disposing);
        }
    }
}
