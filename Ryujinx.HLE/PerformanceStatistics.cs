using Ryujinx.Profiler;
using System.Diagnostics;
using System.Timers;

namespace Ryujinx.HLE
{
    public class PerformanceStatistics
    {
        private const double FrameRateWeight = 0.5;

        private const int FrameTypeSystem = 0;
        private const int FrameTypeGame   = 1;

        private double[] _averageFrameRate;
        private double[] _accumulatedFrameTime;
        private double[] _previousFrameTime;

        private long[] _framesRendered;

        private object[] _frameLock;

        private double _ticksToSeconds;

        private Stopwatch _executionTime;

        private Timer _resetTimer;

        public PerformanceStatistics()
        {
            _averageFrameRate     = new double[2];
            _accumulatedFrameTime = new double[2];
            _previousFrameTime    = new double[2];

            _framesRendered = new long[2];

            _frameLock = new object[] { new object(), new object() };

            _executionTime = new Stopwatch();

            _executionTime.Start();

            _resetTimer = new Timer(1000);

            _resetTimer.Elapsed += ResetTimerElapsed;

            _resetTimer.AutoReset = true;

            _resetTimer.Start();

            _ticksToSeconds = 1.0 / Stopwatch.Frequency;
        }

        private void ResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CalculateAverageFrameRate(FrameTypeSystem);
            CalculateAverageFrameRate(FrameTypeGame);
        }

        private void CalculateAverageFrameRate(int frameType)
        {
            double frameRate = 0;

            if (_accumulatedFrameTime[frameType] > 0)
            {
                frameRate = _framesRendered[frameType] / _accumulatedFrameTime[frameType];
            }

            lock (_frameLock[frameType])
            {
                _averageFrameRate[frameType] = LinearInterpolate(_averageFrameRate[frameType], frameRate);

                _framesRendered[frameType] = 0;

                _accumulatedFrameTime[frameType] = 0;
            }
        }

        private double LinearInterpolate(double old, double New)
        {
            return old * (1.0 - FrameRateWeight) + New * FrameRateWeight;
        }

        public void RecordSystemFrameTime()
        {
            RecordFrameTime(FrameTypeSystem);
            Profile.FlagTime(TimingFlagType.SystemFrame);
        }

        public void RecordGameFrameTime()
        {
            RecordFrameTime(FrameTypeGame);
            Profile.FlagTime(TimingFlagType.FrameSwap);
        }

        private void RecordFrameTime(int frameType)
        {
            double currentFrameTime = _executionTime.ElapsedTicks * _ticksToSeconds;

            double elapsedFrameTime = currentFrameTime - _previousFrameTime[frameType];

            _previousFrameTime[frameType] = currentFrameTime;

            lock (_frameLock[frameType])
            {
                _accumulatedFrameTime[frameType] += elapsedFrameTime;

                _framesRendered[frameType]++;
            }
        }

        public double GetSystemFrameRate()
        {
            return _averageFrameRate[FrameTypeSystem];
        }

        public double GetGameFrameRate()
        {
            return _averageFrameRate[FrameTypeGame];
        }
    }
}
