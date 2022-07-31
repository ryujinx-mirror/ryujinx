namespace Ryujinx.Graphics.Vulkan
{
    internal class BufferUsageBitmap
    {
        private BitMap _bitmap;
        private int _size;
        private int _granularity;
        private int _bits;

        private int _intsPerCb;
        private int _bitsPerCb;

        public BufferUsageBitmap(int size, int granularity)
        {
            _size = size;
            _granularity = granularity;
            _bits = (size + (granularity - 1)) / granularity;

            _intsPerCb = (_bits + (BitMap.IntSize - 1)) / BitMap.IntSize;
            _bitsPerCb = _intsPerCb * BitMap.IntSize;

            _bitmap = new BitMap(_bitsPerCb * CommandBufferPool.MaxCommandBuffers);
        }

        public void Add(int cbIndex, int offset, int size)
        {
            // Some usages can be out of bounds (vertex buffer on amd), so bound if necessary.
            if (offset + size > _size)
            {
                size = _size - offset;
            }

            int cbBase = cbIndex * _bitsPerCb;
            int start = cbBase + offset / _granularity;
            int end = cbBase + (offset + size - 1) / _granularity;

            _bitmap.SetRange(start, end);
        }

        public bool OverlapsWith(int cbIndex, int offset, int size)
        {
            int cbBase = cbIndex * _bitsPerCb;
            int start = cbBase + offset / _granularity;
            int end = cbBase + (offset + size - 1) / _granularity;

            return _bitmap.IsSet(start, end);
        }

        public bool OverlapsWith(int offset, int size)
        {
            for (int i = 0; i < CommandBufferPool.MaxCommandBuffers; i++)
            {
                if (OverlapsWith(i, offset, size))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear(int cbIndex)
        {
            _bitmap.ClearInt(cbIndex * _intsPerCb, (cbIndex + 1) * _intsPerCb - 1);
        }
    }
}
