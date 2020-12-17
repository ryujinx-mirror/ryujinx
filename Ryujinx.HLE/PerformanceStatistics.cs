using Ryujinx.Common;
using System.Diagnostics;
using System.Timers;

namespace Ryujinx.HLE
{
    public class PerformanceStatistics
    {
        private const double FrameRateWeight = 0.5;

        private const int FrameTypeGame   = 0;
        private const int PercentTypeFifo = 0;

        private double[] _averageFrameRate;
        private double[] _accumulatedFrameTime;
        private double[] _previousFrameTime;

        private double[] _averagePercent;
        private double[] _accumulatedActiveTime;
        private double[] _percentLastEndTime;
        private double[] _percentStartTime;

        private long[]   _framesRendered;
        private double[] _percentTime;

        private object[] _frameLock;
        private object[] _percentLock;

        private double _ticksToSeconds;

        private Timer _resetTimer;

        public PerformanceStatistics()
        {
            _averageFrameRate     = new double[1];
            _accumulatedFrameTime = new double[1];
            _previousFrameTime    = new double[1];

            _averagePercent        = new double[1];
            _accumulatedActiveTime = new double[1];
            _percentLastEndTime    = new double[1];
            _percentStartTime      = new double[1];

            _framesRendered = new long[1];
            _percentTime    = new double[1];

            _frameLock   = new object[] { new object() };
            _percentLock = new object[] { new object() };

            _resetTimer = new Timer(1000);

            _resetTimer.Elapsed += ResetTimerElapsed;

            _resetTimer.AutoReset = true;

            _resetTimer.Start();

            _ticksToSeconds = 1.0 / PerformanceCounter.TicksPerSecond;
        }

        private void ResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CalculateAverageFrameRate(FrameTypeGame);
            CalculateAveragePercent(PercentTypeFifo);
        }

        private void CalculateAverageFrameRate(int frameType)
        {
            double frameRate = 0;

            lock (_frameLock[frameType])
            {
                if (_accumulatedFrameTime[frameType] > 0)
                {
                    frameRate = _framesRendered[frameType] / _accumulatedFrameTime[frameType];
                }

                _averageFrameRate[frameType] = LinearInterpolate(_averageFrameRate[frameType], frameRate);

                _framesRendered[frameType] = 0;

                _accumulatedFrameTime[frameType] = 0;
            }
        }

        private void CalculateAveragePercent(int percentType)
        {
            // If start time is non-zero, a percent reading is still being measured.
            // If there aren't any readings, the default should be 100% if still being measured, or 0% if not.
            double percent = (_percentStartTime[percentType] == 0) ? 0 : 100;

            lock (_percentLock[percentType])
            {
                if (_percentTime[percentType] > 0)
                {
                    percent = (_accumulatedActiveTime[percentType] / _percentTime[percentType]) * 100;
                }

                _averagePercent[percentType] = percent;

                _percentTime[percentType] = 0;

                _accumulatedActiveTime[percentType] = 0;
            }
        }

        private double LinearInterpolate(double lhs, double rhs)
        {
            return lhs * (1.0 - FrameRateWeight) + rhs * FrameRateWeight;
        }

        public void RecordGameFrameTime()
        {
            RecordFrameTime(FrameTypeGame);
        }

        public void RecordFifoStart()
        {
            StartPercentTime(PercentTypeFifo);
        }

        public void RecordFifoEnd()
        {
            EndPercentTime(PercentTypeFifo);
        }

        private void StartPercentTime(int percentType)
        {
            double currentTime = PerformanceCounter.ElapsedTicks * _ticksToSeconds;

            _percentStartTime[percentType] = currentTime;
        }

        private void EndPercentTime(int percentType)
        {
            double currentTime = PerformanceCounter.ElapsedTicks * _ticksToSeconds;

            double elapsedTime = currentTime - _percentLastEndTime[percentType];
            double elapsedActiveTime = currentTime - _percentStartTime[percentType];

            lock (_percentLock[percentType])
            {
                _accumulatedActiveTime[percentType] += elapsedActiveTime;

                _percentTime[percentType] += elapsedTime;
            }

            _percentLastEndTime[percentType] = currentTime;
            _percentStartTime[percentType] = 0;
        }

        private void RecordFrameTime(int frameType)
        {
            double currentFrameTime = PerformanceCounter.ElapsedTicks * _ticksToSeconds;

            double elapsedFrameTime = currentFrameTime - _previousFrameTime[frameType];

            _previousFrameTime[frameType] = currentFrameTime;

            lock (_frameLock[frameType])
            {
                _accumulatedFrameTime[frameType] += elapsedFrameTime;

                _framesRendered[frameType]++;
            }
        }

        public double GetGameFrameRate()
        {
            return _averageFrameRate[FrameTypeGame];
        }

        public double GetFifoPercent()
        {
            return _averagePercent[PercentTypeFifo];
        }
    }
}
