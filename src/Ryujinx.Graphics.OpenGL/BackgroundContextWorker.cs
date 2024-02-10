using Ryujinx.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Graphics.OpenGL
{
    unsafe class BackgroundContextWorker : IDisposable
    {
        [ThreadStatic]
        public static bool InBackground;
        private readonly Thread _thread;
        private bool _running;

        private readonly AutoResetEvent _signal;
        private readonly Queue<Action> _work;
        private readonly ObjectPool<ManualResetEventSlim> _invokePool;
        private readonly IOpenGLContext _backgroundContext;

        public BackgroundContextWorker(IOpenGLContext backgroundContext)
        {
            _backgroundContext = backgroundContext;
            _running = true;

            _signal = new AutoResetEvent(false);
            _work = new Queue<Action>();
            _invokePool = new ObjectPool<ManualResetEventSlim>(() => new ManualResetEventSlim(), 10);

            _thread = new Thread(Run);
            _thread.Start();
        }

        public bool HasContext() => _backgroundContext.HasContext();

        private void Run()
        {
            InBackground = true;

            _backgroundContext.MakeCurrent();

            while (_running)
            {
                Action action;

                lock (_work)
                {
                    _work.TryDequeue(out action);
                }

                if (action != null)
                {
                    action();
                }
                else
                {
                    _signal.WaitOne();
                }
            }

            _backgroundContext.Dispose();
        }

        public void Invoke(Action action)
        {
            ManualResetEventSlim actionComplete = _invokePool.Allocate();

            lock (_work)
            {
                _work.Enqueue(() =>
                {
                    action();
                    actionComplete.Set();
                });
            }

            _signal.Set();

            actionComplete.Wait();
            actionComplete.Reset();

            _invokePool.Release(actionComplete);
        }

        public void Dispose()
        {
            _running = false;
            _signal.Set();

            _thread.Join();
            _signal.Dispose();
        }
    }
}
