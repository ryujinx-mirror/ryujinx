namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class BufferSlotArray
    {
        // TODO: move to BufferQueue
        public const int NumBufferSlots     = 0x40;
        public const int MaxAcquiredBuffers = NumBufferSlots - 2;
        public const int InvalidBufferSlot  = -1;

        private BufferSlot[] _raw = new BufferSlot[NumBufferSlots];

        public BufferSlotArray()
        {
            for (int i = 0; i < _raw.Length; i++)
            {
                _raw[i] = new BufferSlot();
            }
        }

        public BufferSlot this[int index]
        {
            get => _raw[index];
            set => _raw[index] = value;
        }

        public int Length => NumBufferSlots;
    }
}
