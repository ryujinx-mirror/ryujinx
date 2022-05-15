using Avalonia.Rendering;
using System;
using System.Threading;
using System.Timers;

namespace Ryujinx.Ava.Ui.Controls
{
    internal class RenderTimer : IRenderTimer, IDisposable
    {
        public event Action<TimeSpan> Tick
        {
            add
            {
                _tick += value;

                if (_subscriberCount++ == 0)
                {
                    Start();
                }
            }

            remove
            {
                if (--_subscriberCount == 0)
                {
                    Stop();
                }

                _tick -= value;
            }
        }

        private Thread _tickThread;
        private readonly System.Timers.Timer _timer;

        private Action<TimeSpan> _tick;
        private int _subscriberCount;

        private bool _isRunning;

        private AutoResetEvent _resetEvent;

        public RenderTimer()
        {
            _timer = new System.Timers.Timer(15);
            _resetEvent = new AutoResetEvent(true);
            _timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TickNow();
        }

        public void Start()
        {
            _timer.Start();
            if (_tickThread == null)
            {
                _tickThread = new Thread(RunTick);
                _tickThread.Name = "RenderTimerTickThread";
                _tickThread.IsBackground = true;
                _isRunning = true;
                _tickThread.Start();
            }
        }

        public void RunTick()
        {
            while (_isRunning)
            {
                _resetEvent.WaitOne();
                _tick?.Invoke(TimeSpan.FromMilliseconds(Environment.TickCount));
            }
        }

        public void TickNow()
        {
            lock (_timer)
            {
                _resetEvent.Set();
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Stop();
            _isRunning = false;
            _resetEvent.Set();
            _tickThread.Join();
            _resetEvent.Dispose();
        }
    }
}
