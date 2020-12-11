using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Graphics.OpenGL.Queries
{
    class CounterQueue : IDisposable
    {
        private const int QueryPoolInitialSize = 100;

        public CounterType Type { get; }
        public bool Disposed { get; private set; }

        private Queue<CounterQueueEvent> _events = new Queue<CounterQueueEvent>();
        private CounterQueueEvent _current;

        private ulong _accumulatedCounter;

        private object _lock = new object();

        private Queue<BufferedQuery> _queryPool;
        private AutoResetEvent _queuedEvent = new AutoResetEvent(false);
        private AutoResetEvent _wakeSignal = new AutoResetEvent(false);

        private Thread _consumerThread;

        internal CounterQueue(CounterType type)
        {
            Type = type;

            QueryTarget glType = GetTarget(Type);

            _queryPool = new Queue<BufferedQuery>(QueryPoolInitialSize);
            for (int i = 0; i < QueryPoolInitialSize; i++)
            {
                _queryPool.Enqueue(new BufferedQuery(glType));
            }

            _current = new CounterQueueEvent(this, glType, 0);

            _consumerThread = new Thread(EventConsumer);
            _consumerThread.Start();
        }

        private void EventConsumer()
        {
            while (!Disposed)
            {
                CounterQueueEvent evt = null;
                lock (_lock)
                {
                    if (_events.Count > 0)
                    {
                        evt = _events.Dequeue();
                    }
                }

                if (evt == null)
                {
                    _queuedEvent.WaitOne(); // No more events to go through, wait for more.
                }
                else
                {
                    evt.TryConsume(ref _accumulatedCounter, true, _wakeSignal);
                }
            }
        }

        internal BufferedQuery GetQueryObject()
        {
            // Creating/disposing query objects on a context we're sharing with will cause issues.
            // So instead, make a lot of query objects on the main thread and reuse them.

            lock (_lock)
            {
                if (_queryPool.Count > 0)
                {
                    BufferedQuery result = _queryPool.Dequeue();
                    return result;
                }
                else
                {
                    return new BufferedQuery(GetTarget(Type));
                }
            }
        }

        internal void ReturnQueryObject(BufferedQuery query)
        {
            lock (_lock)
            {
                _queryPool.Enqueue(query);
            }
        }

        public CounterQueueEvent QueueReport(EventHandler<ulong> resultHandler, ulong lastDrawIndex)
        {
            CounterQueueEvent result;
            ulong draws = lastDrawIndex - _current.DrawIndex;

            lock (_lock)
            {
                // A query's result only matters if more than one draw was performed during it.
                // Otherwise, dummy it out and return 0 immediately.

                if (draws > 0)
                {
                    _current.Complete(true);
                    _events.Enqueue(_current);

                    _current.OnResult += resultHandler;
                }
                else
                {
                    _current.Complete(false);
                    _current.Dispose();
                    resultHandler(_current, 0);
                }

                result = _current;

                _current = new CounterQueueEvent(this, GetTarget(Type), lastDrawIndex);
            }

            _queuedEvent.Set();

            return result;
        }

        public void QueueReset()
        {
            lock (_lock)
            {
                _current.Clear();
            }
        }

        private static QueryTarget GetTarget(CounterType type)
        {
            switch (type)
            {
                case CounterType.SamplesPassed: return QueryTarget.SamplesPassed;
                case CounterType.PrimitivesGenerated: return QueryTarget.PrimitivesGenerated;
                case CounterType.TransformFeedbackPrimitivesWritten: return QueryTarget.TransformFeedbackPrimitivesWritten;
            }

            return QueryTarget.SamplesPassed;
        }

        public void Flush(bool blocking)
        {
            if (!blocking)
            {
                // Just wake the consumer thread - it will update the queries.
                _wakeSignal.Set();
                return;
            }

            lock (_lock)
            {
                // Tell the queue to process all events.
                while (_events.Count > 0)
                {
                    CounterQueueEvent flush = _events.Peek();
                    if (!flush.TryConsume(ref _accumulatedCounter, true))
                    {
                        return; // If not blocking, then return when we encounter an event that is not ready yet.
                    }
                    _events.Dequeue();
                }
            }
        }

        public void FlushTo(CounterQueueEvent evt)
        {
            lock (_lock)
            {
                if (evt.Disposed)
                {
                    return;
                }

                // Tell the queue to process all events up to this one.
                while (_events.Count > 0)
                {
                    CounterQueueEvent flush = _events.Dequeue();
                    flush.TryConsume(ref _accumulatedCounter, true);

                    if (flush == evt)
                    {
                        return;
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                while (_events.Count > 0)
                {
                    CounterQueueEvent evt = _events.Dequeue();

                    evt.Dispose();
                }

                Disposed = true;
            }

            _queuedEvent.Set();

            _consumerThread.Join();

            foreach (BufferedQuery query in _queryPool)
            {
                query.Dispose();
            }
        }
    }
}
