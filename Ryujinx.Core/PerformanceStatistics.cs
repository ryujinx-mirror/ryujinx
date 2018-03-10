using System.Diagnostics;
using System.Timers;

namespace Ryujinx.Core
{
    public class PerformanceStatistics
    {
        Stopwatch ExecutionTime = new Stopwatch();
        Timer ResetTimer = new Timer(1000);

        long CurrentGameFrameEnded;
        long CurrentSystemFrameEnded;
        long CurrentSystemFrameStart;
        long LastGameFrameEnded;
        long LastSystemFrameEnded;

        double AccumulatedGameFrameTime;
        double AccumulatedSystemFrameTime;
        double CurrentGameFrameTime;
        double CurrentSystemFrameTime;
        double PreviousGameFrameTime;
        double PreviousSystemFrameTime;
        public double GameFrameRate   { get; private set; }
        public double SystemFrameRate { get; private set; }
        public long SystemFramesRendered;
        public long GameFramesRendered;
        public long ElapsedMilliseconds => ExecutionTime.ElapsedMilliseconds;
        public long ElapsedMicroseconds => (long)
                (((double)ExecutionTime.ElapsedTicks / Stopwatch.Frequency) * 1000000);
        public long ElapsedNanoseconds => (long)
                (((double)ExecutionTime.ElapsedTicks / Stopwatch.Frequency) * 1000000000);

        public PerformanceStatistics()
        {
            ExecutionTime.Start();
            ResetTimer.Elapsed += ResetTimerElapsed;
            ResetTimer.AutoReset = true;
            ResetTimer.Start();
        }

        private void ResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ResetStatistics();
        }

        public void StartSystemFrame()
        {
            PreviousSystemFrameTime = CurrentSystemFrameTime;
            LastSystemFrameEnded = CurrentSystemFrameEnded;
            CurrentSystemFrameStart = ElapsedMicroseconds;
        }

        public void EndSystemFrame()
        {
            CurrentSystemFrameEnded = ElapsedMicroseconds;
            CurrentSystemFrameTime = CurrentSystemFrameEnded - CurrentSystemFrameStart;
            AccumulatedSystemFrameTime += CurrentSystemFrameTime;
            SystemFramesRendered++;
        }

        public void RecordGameFrameTime()
        {
            CurrentGameFrameEnded = ElapsedMicroseconds;
            CurrentGameFrameTime = CurrentGameFrameEnded - LastGameFrameEnded;
            PreviousGameFrameTime = CurrentGameFrameTime;
            LastGameFrameEnded = CurrentGameFrameEnded;
            AccumulatedGameFrameTime += CurrentGameFrameTime;
            GameFramesRendered++;
        }

        public void ResetStatistics()
        {
            GameFrameRate = 1000 / ((AccumulatedGameFrameTime / GameFramesRendered) / 1000);
            GameFrameRate = double.IsNaN(GameFrameRate) ? 0 : GameFrameRate;
            SystemFrameRate = 1000 / ((AccumulatedSystemFrameTime / SystemFramesRendered) / 1000);
            SystemFrameRate = double.IsNaN(SystemFrameRate) ? 0 : SystemFrameRate;

            GameFramesRendered = 0;
            SystemFramesRendered = 0;
            AccumulatedGameFrameTime = 0;
            AccumulatedSystemFrameTime = 0;
        }
    }
}
