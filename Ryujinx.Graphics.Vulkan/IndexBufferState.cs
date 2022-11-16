using Silk.NET.Vulkan;

namespace Ryujinx.Graphics.Vulkan
{
    internal struct IndexBufferState
    {
        public static IndexBufferState Null => new IndexBufferState(GAL.BufferHandle.Null, 0, 0);

        private readonly int _offset;
        private readonly int _size;
        private readonly IndexType _type;

        private readonly GAL.BufferHandle _handle;
        private Auto<DisposableBuffer> _buffer;

        public IndexBufferState(GAL.BufferHandle handle, int offset, int size, IndexType type)
        {
            _handle = handle;
            _offset = offset;
            _size = size;
            _type = type;
            _buffer = null;
        }

        public IndexBufferState(GAL.BufferHandle handle, int offset, int size)
        {
            _handle = handle;
            _offset = offset;
            _size = size;
            _type = IndexType.Uint16;
            _buffer = null;
        }

        public void BindIndexBuffer(VulkanRenderer gd, CommandBufferScoped cbs)
        {
            Auto<DisposableBuffer> autoBuffer;
            int offset, size;
            IndexType type = _type;

            if (_type == IndexType.Uint8Ext && !gd.Capabilities.SupportsIndexTypeUint8)
            {
                // Index type is not supported. Convert to I16.
                autoBuffer = gd.BufferManager.GetBufferI8ToI16(cbs, _handle, _offset, _size);

                type = IndexType.Uint16;
                offset = 0;
                size = _size * 2;
            }
            else
            {
                autoBuffer = gd.BufferManager.GetBuffer(cbs.CommandBuffer, _handle, false, out int bufferSize);

                if (_offset >= bufferSize)
                {
                    autoBuffer = null;
                }

                offset = _offset;
                size = _size;
            }

            _buffer = autoBuffer;

            if (autoBuffer != null)
            {
                gd.Api.CmdBindIndexBuffer(cbs.CommandBuffer, autoBuffer.Get(cbs, offset, size).Value, (ulong)offset, type);
            }
        }

        public void BindConvertedIndexBuffer(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            int firstIndex,
            int indexCount,
            int convertedCount,
            IndexBufferPattern pattern)
        {
            Auto<DisposableBuffer> autoBuffer;

            // Convert the index buffer using the given pattern.
            int indexSize = GetIndexSize();

            int firstIndexOffset = firstIndex * indexSize;

            autoBuffer = gd.BufferManager.GetBufferTopologyConversion(cbs, _handle, _offset + firstIndexOffset, indexCount * indexSize, pattern, indexSize);

            int size = convertedCount * 4;

            _buffer = autoBuffer;

            if (autoBuffer != null)
            {
                gd.Api.CmdBindIndexBuffer(cbs.CommandBuffer, autoBuffer.Get(cbs, 0, size).Value, 0, IndexType.Uint32);
            }
        }

        public Auto<DisposableBuffer> BindConvertedIndexBufferIndirect(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            GAL.BufferRange indirectBuffer,
            GAL.BufferRange drawCountBuffer,
            IndexBufferPattern pattern,
            bool hasDrawCount,
            int maxDrawCount,
            int indirectDataStride)
        {
            // Convert the index buffer using the given pattern.
            int indexSize = GetIndexSize();

            (var indexBufferAuto, var indirectBufferAuto) = gd.BufferManager.GetBufferTopologyConversionIndirect(
                gd,
                cbs,
                new GAL.BufferRange(_handle, _offset, _size),
                indirectBuffer,
                drawCountBuffer,
                pattern,
                indexSize,
                hasDrawCount,
                maxDrawCount,
                indirectDataStride);

            int convertedCount = pattern.GetConvertedCount(_size / indexSize);
            int size = convertedCount * 4;

            _buffer = indexBufferAuto;

            if (indexBufferAuto != null)
            {
                gd.Api.CmdBindIndexBuffer(cbs.CommandBuffer, indexBufferAuto.Get(cbs, 0, size).Value, 0, IndexType.Uint32);
            }

            return indirectBufferAuto;
        }

        private int GetIndexSize()
        {
            return _type switch
            {
                IndexType.Uint32 => 4,
                IndexType.Uint16 => 2,
                _ => 1,
            };
        }

        public bool BoundEquals(Auto<DisposableBuffer> buffer)
        {
            return _buffer == buffer;
        }
    }
}
