using Ryujinx.Common.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.System
{
    /// <summary>
    /// Handle Windows Multimedia timer resolution.
    /// </summary>
    public class WindowsMultimediaTimerResolution : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TimeCaps
        {
            public uint wPeriodMin;
            public uint wPeriodMax;
        };

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeGetDevCaps(ref TimeCaps timeCaps, uint sizeTimeCaps);

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uMilliseconds);

        private uint _targetResolutionInMilliseconds;
        private bool _isActive;

        /// <summary>
        /// Create a new <see cref="WindowsMultimediaTimerResolution"/> and activate the given resolution.
        /// </summary>
        /// <param name="targetResolutionInMilliseconds"></param>
        public WindowsMultimediaTimerResolution(uint targetResolutionInMilliseconds)
        {
            _targetResolutionInMilliseconds = targetResolutionInMilliseconds;

            EnsureResolutionSupport();
            Activate();
        }

        private void EnsureResolutionSupport()
        {
            TimeCaps timeCaps = default;

            uint result = timeGetDevCaps(ref timeCaps, (uint)Unsafe.SizeOf<TimeCaps>());

            if (result != 0)
            {
                Logger.Notice.Print(LogClass.Application, $"timeGetDevCaps failed with result: {result}");
            }
            else
            {
                uint supportedTargetResolutionInMilliseconds = Math.Min(Math.Max(timeCaps.wPeriodMin, _targetResolutionInMilliseconds), timeCaps.wPeriodMax);

                if (supportedTargetResolutionInMilliseconds != _targetResolutionInMilliseconds)
                {
                    Logger.Notice.Print(LogClass.Application, $"Target resolution isn't supported by OS, using closest resolution: {supportedTargetResolutionInMilliseconds}ms");

                    _targetResolutionInMilliseconds = supportedTargetResolutionInMilliseconds;
                }
            }
        }

        private void Activate()
        {
            uint result = timeBeginPeriod(_targetResolutionInMilliseconds);

            if (result != 0)
            {
                Logger.Notice.Print(LogClass.Application, $"timeBeginPeriod failed with result: {result}");
            }
            else
            {
                _isActive = true;
            }
        }

        private void Disable()
        {
            if (_isActive)
            {
                uint result = timeEndPeriod(_targetResolutionInMilliseconds);

                if (result != 0)
                {
                    Logger.Notice.Print(LogClass.Application, $"timeEndPeriod failed with result: {result}");
                }
                else
                {
                    _isActive = false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disable();
            }
        }
    }
}
