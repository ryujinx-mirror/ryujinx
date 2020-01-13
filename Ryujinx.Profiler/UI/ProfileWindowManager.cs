using System.Threading;
using OpenTK;
using OpenTK.Input;
using Ryujinx.Common;

namespace Ryujinx.Profiler.UI
{
    public class ProfileWindowManager
    {
        private ProfileWindow _window;
        private Thread _profileThread;
        private Thread _renderThread;
        private bool _profilerRunning;

        // Timing
        private double _prevTime;

        public ProfileWindowManager()
        {
            if (Profile.ProfilingEnabled())
            {
                _profilerRunning = true;
                _prevTime        = 0;
                _profileThread   = new Thread(ProfileLoop)
                {
                    Name = "Profiler.ProfileThread"
                };
                _profileThread.Start();
            }
        }

        public void ToggleVisible()
        {
            if (Profile.ProfilingEnabled())
            {
                _window.ToggleVisible();
            }
        }

        public void Close()
        {
            if (_window != null)
            {
                _profilerRunning = false;
                _window.Close();
                _window.Dispose();
            }

            _window = null;
        }

        public void UpdateKeyInput(KeyboardState keyboard)
        {
            if (Profile.Controls.TogglePressed(keyboard))
            {
                ToggleVisible();
            }
            Profile.Controls.SetPrevKeyboardState(keyboard);
        }

        private void ProfileLoop()
        {
            using (_window = new ProfileWindow())
            {
                // Create thread for render loop
                _renderThread = new Thread(RenderLoop)
                {
                    Name = "Profiler.RenderThread"
                };
                _renderThread.Start();

                while (_profilerRunning)
                {
                    double time = (double)PerformanceCounter.ElapsedTicks / PerformanceCounter.TicksPerSecond;
                    _window.Update(new FrameEventArgs(time - _prevTime));
                    _prevTime = time;

                    // Sleep to be less taxing, update usually does very little
                    Thread.Sleep(1);
                }
            }
        }

        private void RenderLoop()
        {
            _window.Context.MakeCurrent(_window.WindowInfo);

            while (_profilerRunning)
            {
                _window.Draw();
                Thread.Sleep(1);
            }
        }
    }
}
