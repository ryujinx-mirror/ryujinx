using System.Diagnostics;
using System.Timers;

namespace Ryujinx.HLE
{
    public class PerformanceStatistics
    {
        private const double FrameRateWeight = 0.5;

        private const int FrameTypeSystem = 0;
        private const int FrameTypeGame   = 1;

        private double[] AverageFrameRate;
        private double[] AccumulatedFrameTime;
        private double[] PreviousFrameTime;

        private long[] FramesRendered;

        private object[] FrameLock;

        private double TicksToSeconds;

        private Stopwatch ExecutionTime;

        private Timer ResetTimer;

        public PerformanceStatistics()
        {
            AverageFrameRate     = new double[2];
            AccumulatedFrameTime = new double[2];
            PreviousFrameTime    = new double[2];

            FramesRendered = new long[2];

            FrameLock = new object[] { new object(), new object() };

            ExecutionTime = new Stopwatch();

            ExecutionTime.Start();

            ResetTimer = new Timer(1000);

            ResetTimer.Elapsed += ResetTimerElapsed;

            ResetTimer.AutoReset = true;

            ResetTimer.Start();

            TicksToSeconds = 1.0 / Stopwatch.Frequency;
        }

        private void ResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CalculateAverageFrameRate(FrameTypeSystem);
            CalculateAverageFrameRate(FrameTypeGame);
        }

        private void CalculateAverageFrameRate(int FrameType)
        {
            double FrameRate = 0;

            if (AccumulatedFrameTime[FrameType] > 0)
            {
                FrameRate = FramesRendered[FrameType] / AccumulatedFrameTime[FrameType];
            }

            lock (FrameLock[FrameType])
            {
                AverageFrameRate[FrameType] = LinearInterpolate(AverageFrameRate[FrameType], FrameRate);

                FramesRendered[FrameType] = 0;

                AccumulatedFrameTime[FrameType] = 0;
            }
        }

        private double LinearInterpolate(double Old, double New)
        {
            return Old * (1.0 - FrameRateWeight) + New * FrameRateWeight;
        }

        public void RecordSystemFrameTime()
        {
            RecordFrameTime(FrameTypeSystem);
        }

        public void RecordGameFrameTime()
        {
            RecordFrameTime(FrameTypeGame);
        }

        private void RecordFrameTime(int FrameType)
        {
            double CurrentFrameTime = ExecutionTime.ElapsedTicks * TicksToSeconds;

            double ElapsedFrameTime = CurrentFrameTime - PreviousFrameTime[FrameType];

            PreviousFrameTime[FrameType] = CurrentFrameTime;

            lock (FrameLock[FrameType])
            {
                AccumulatedFrameTime[FrameType] += ElapsedFrameTime;

                FramesRendered[FrameType]++;
            }
        }

        public double GetSystemFrameRate()
        {
            return AverageFrameRate[FrameTypeSystem];
        }

        public double GetGameFrameRate()
        {
            return AverageFrameRate[FrameTypeGame];
        }
    }
}
