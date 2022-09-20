using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    interface ICacheKey : IDisposable
    {
        bool KeyEqual(ICacheKey other);
    }

    struct I8ToI16CacheKey : ICacheKey
    {
        // Used to notify the pipeline that bindings have invalidated on dispose.
        private readonly VulkanRenderer _gd;
        private Auto<DisposableBuffer> _buffer;

        public I8ToI16CacheKey(VulkanRenderer gd)
        {
            _gd = gd;
            _buffer = null;
        }

        public bool KeyEqual(ICacheKey other)
        {
            return other is I8ToI16CacheKey;
        }

        public void Dispose()
        {
            _gd.PipelineInternal.DirtyIndexBuffer(_buffer);
        }
    }

    struct AlignedVertexBufferCacheKey : ICacheKey
    {
        private readonly int _stride;
        private readonly int _alignment;

        // Used to notify the pipeline that bindings have invalidated on dispose.
        private readonly VulkanRenderer _gd;
        private Auto<DisposableBuffer> _buffer;

        public AlignedVertexBufferCacheKey(VulkanRenderer gd, int stride, int alignment)
        {
            _gd = gd;
            _stride = stride;
            _alignment = alignment;
            _buffer = null;
        }

        public bool KeyEqual(ICacheKey other)
        {
            return other is AlignedVertexBufferCacheKey entry &&
                entry._stride == _stride &&
                entry._alignment == _alignment;
        }

        public void SetBuffer(Auto<DisposableBuffer> buffer)
        {
            _buffer = buffer;
        }

        public void Dispose()
        {
            _gd.PipelineInternal.DirtyVertexBuffer(_buffer);
        }
    }

    struct TopologyConversionCacheKey : ICacheKey
    {
        private IndexBufferPattern _pattern;
        private int _indexSize;

        // Used to notify the pipeline that bindings have invalidated on dispose.
        private readonly VulkanRenderer _gd;
        private Auto<DisposableBuffer> _buffer;

        public TopologyConversionCacheKey(VulkanRenderer gd, IndexBufferPattern pattern, int indexSize)
        {
            _gd = gd;
            _pattern = pattern;
            _indexSize = indexSize;
            _buffer = null;
        }

        public bool KeyEqual(ICacheKey other)
        {
            return other is TopologyConversionCacheKey entry &&
                entry._pattern == _pattern &&
                entry._indexSize == _indexSize;
        }

        public void SetBuffer(Auto<DisposableBuffer> buffer)
        {
            _buffer = buffer;
        }

        public void Dispose()
        {
            _gd.PipelineInternal.DirtyIndexBuffer(_buffer);
        }
    }

    struct CacheByRange<T> where T : IDisposable
    {
        private struct Entry
        {
            public ICacheKey Key;
            public T Value;

            public Entry(ICacheKey key, T value)
            {
                Key = key;
                Value = value;
            }
        }

        private Dictionary<ulong, List<Entry>> _ranges;

        public void Add(int offset, int size, ICacheKey key, T value)
        {
            List<Entry> entries = GetEntries(offset, size);

            entries.Add(new Entry(key, value));
        }

        public bool TryGetValue(int offset, int size, ICacheKey key, out T value)
        {
            List<Entry> entries = GetEntries(offset, size);

            foreach (Entry entry in entries)
            {
                if (entry.Key.KeyEqual(key))
                {
                    value = entry.Value;

                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Clear()
        {
            if (_ranges != null)
            {
                foreach (List<Entry> entries in _ranges.Values)
                {
                    foreach (Entry entry in entries)
                    {
                        entry.Key.Dispose();
                        entry.Value.Dispose();
                    }
                }

                _ranges.Clear();
                _ranges = null;
            }
        }

        private List<Entry> GetEntries(int offset, int size)
        {
            if (_ranges == null)
            {
                _ranges = new Dictionary<ulong, List<Entry>>();
            }

            ulong key = PackRange(offset, size);

            List<Entry> value;
            if (!_ranges.TryGetValue(key, out value))
            {
                value = new List<Entry>();
                _ranges.Add(key, value);
            }

            return value;
        }

        private static ulong PackRange(int offset, int size)
        {
            return (uint)offset | ((ulong)size << 32);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
