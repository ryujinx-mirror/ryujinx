using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    struct CacheByRange<T> where T : IDisposable
    {
        private Dictionary<ulong, T> _ranges;

        public void Add(int offset, int size, T value)
        {
            EnsureInitialized();
            _ranges.Add(PackRange(offset, size),  value);
        }

        public bool TryGetValue(int offset, int size, out T value)
        {
            EnsureInitialized();
            return _ranges.TryGetValue(PackRange(offset, size), out value);
        }

        public void Clear()
        {
            if (_ranges != null)
            {
                foreach (T value in _ranges.Values)
                {
                    value.Dispose();
                }

                _ranges.Clear();
                _ranges = null;
            }
        }

        private void EnsureInitialized()
        {
            if (_ranges == null)
            {
                _ranges = new Dictionary<ulong, T>();
            }
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
