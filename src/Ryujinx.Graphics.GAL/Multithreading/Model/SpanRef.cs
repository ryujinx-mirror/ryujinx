using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Model
{
    struct SpanRef<T> where T : unmanaged
    {
        private int _packedLengthId;

        public SpanRef(ThreadedRenderer renderer, T[] data)
        {
            _packedLengthId = -(renderer.AddTableRef(data) + 1);
        }

        public SpanRef(int length)
        {
            _packedLengthId = length;
        }

        public Span<T> Get(ThreadedRenderer renderer)
        {
            if (_packedLengthId >= 0)
            {
                return renderer.SpanPool.Get<T>(_packedLengthId);
            }
            else
            {
                return new Span<T>((T[])renderer.RemoveTableRef(-(_packedLengthId + 1)));
            }
        }

        public void Dispose(ThreadedRenderer renderer)
        {
            if (_packedLengthId > 0)
            {
                renderer.SpanPool.Dispose<T>(_packedLengthId);
            }
        }
    }
}
