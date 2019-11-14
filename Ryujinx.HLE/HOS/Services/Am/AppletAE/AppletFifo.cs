using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    internal class AppletFifo<T> : IEnumerable<T>
    {
        private ConcurrentQueue<T> _dataQueue;

        public int Count => _dataQueue.Count;

        public AppletFifo()
        {
            _dataQueue = new ConcurrentQueue<T>();
        }

        public void Push(T item)
        {
            _dataQueue.Enqueue(item);
        }

        public T Pop()
        {
            if (_dataQueue.TryDequeue(out T result))
            {
                return result;
            }

            throw new InvalidOperationException("FIFO empty.");
        }

        public bool TryPop(out T result)
        {
            return _dataQueue.TryDequeue(out result);
        }

        public T Peek()
        {
            if (_dataQueue.TryPeek(out T result))
            {
                return result;
            }

            throw new InvalidOperationException("FIFO empty.");
        }

        public bool TryPeek(out T result)
        {
            return _dataQueue.TryPeek(out result);
        }

        public void Clear()
        {
            _dataQueue.Clear();
        }

        public T[] ToArray()
        {
            return _dataQueue.ToArray();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _dataQueue.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dataQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dataQueue.GetEnumerator();
        }
    }
}
