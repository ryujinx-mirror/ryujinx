using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;
using System.Threading;

namespace Ryujinx.Graphics.OpenGL.Queries
{
    class CounterQueueEvent : ICounterEvent
    {
        public event EventHandler<ulong> OnResult;

        public QueryTarget Type { get; }
        public bool ClearCounter { get; private set; }
        public int Query => _counter.Query;

        public bool Disposed { get; private set; }
        public bool Invalid { get; set; }

        public ulong DrawIndex { get; }

        private CounterQueue _queue;
        private BufferedQuery _counter;

        private object _lock = new object();

        public CounterQueueEvent(CounterQueue queue, QueryTarget type, ulong drawIndex)
        {
            _queue = queue;

            _counter = queue.GetQueryObject();
            Type = type;

            DrawIndex = drawIndex;

            _counter.Begin();
        }

        internal void Clear()
        {
            _counter.Reset();
            ClearCounter = true;
        }

        internal void Complete(bool withResult)
        {
            _counter.End(withResult);
        }

        internal bool TryConsume(ref ulong result, bool block, AutoResetEvent wakeSignal = null)
        {
            lock (_lock)
            {
                if (Disposed)
                {
                    return true;
                }

                if (ClearCounter || Type == QueryTarget.Timestamp)
                {
                    result = 0;
                }

                long queryResult;

                if (block)
                {
                    queryResult = _counter.AwaitResult(wakeSignal);
                }
                else
                {
                    if (!_counter.TryGetResult(out queryResult))
                    {
                        return false;
                    }
                }

                result += (ulong)queryResult;

                OnResult?.Invoke(this, result);

                Dispose(); // Return the our resources to the pool.

                return true;
            }
        }

        public void Flush()
        {
            if (Disposed)
            {
                return;
            }

            // Tell the queue to process all events up to this one.
            _queue.FlushTo(this);
        }

        public void Dispose()
        {
            Disposed = true;
            _queue.ReturnQueryObject(_counter);
        }
    }
}
