using OpenTK;
using OpenTK.Graphics;
using Ryujinx.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Graphics.OpenGL
{
    class BackgroundContextWorker : IDisposable
    {
        [ThreadStatic]
        public static bool InBackground;

        private GameWindow _window;
        private GraphicsContext _context;
        private Thread _thread;
        private bool _running;

        private AutoResetEvent _signal;
        private Queue<Action> _work;
        private ObjectPool<ManualResetEventSlim> _invokePool;

        public BackgroundContextWorker(IGraphicsContext baseContext)
        {
            _window = new GameWindow(
                100, 100, GraphicsMode.Default,
                "Background Window", OpenTK.GameWindowFlags.FixedWindow, OpenTK.DisplayDevice.Default,
                3, 3, GraphicsContextFlags.ForwardCompatible, baseContext, false);

            _window.Visible = false;
            _context = (GraphicsContext)_window.Context;
            _context.MakeCurrent(null);

            _running = true;

            _signal = new AutoResetEvent(false);
            _work = new Queue<Action>();
            _invokePool = new ObjectPool<ManualResetEventSlim>(() => new ManualResetEventSlim(), 10);

            _thread = new Thread(Run);
            _thread.Start();
        }

        private void Run()
        {
            InBackground = true;
            _context.MakeCurrent(_window.WindowInfo);

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

            _window.Dispose();
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
