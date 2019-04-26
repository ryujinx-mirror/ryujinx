using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ryujinx.Common;

namespace Ryujinx.Profiler
{
    public class InternalProfile
    {
        private struct TimerQueueValue
        {
            public ProfileConfig Config;
            public long Time;
            public bool IsBegin;
        }

        internal Dictionary<ProfileConfig, TimingInfo> Timers { get; set; }

        private readonly object _timerQueueClearLock = new object();
        private ConcurrentQueue<TimerQueueValue> _timerQueue;

        private int _sessionCounter = 0;

        // Cleanup thread
        private readonly Thread _cleanupThread;
        private bool _cleanupRunning;
        private readonly long _history;
        private long _preserve;

        // Timing flags
        private TimingFlag[] _timingFlags;
        private long[] _timingFlagAverages;
        private long[] _timingFlagLast;
        private long[] _timingFlagLastDelta;
        private int _timingFlagCount;
        private int _timingFlagIndex;

        private int _maxFlags;

        private Action<TimingFlag> _timingFlagCallback;

        public InternalProfile(long history, int maxFlags)
        {
            _maxFlags            = maxFlags;
            Timers               = new Dictionary<ProfileConfig, TimingInfo>();
            _timingFlags         = new TimingFlag[_maxFlags];
            _timingFlagAverages  = new long[(int)TimingFlagType.Count];
            _timingFlagLast      = new long[(int)TimingFlagType.Count];
            _timingFlagLastDelta = new long[(int)TimingFlagType.Count];
            _timerQueue          = new ConcurrentQueue<TimerQueueValue>();
            _history             = history;
            _cleanupRunning      = true;

            // Create cleanup thread.
            _cleanupThread = new Thread(CleanupLoop);
            _cleanupThread.Start();
        }

        private void CleanupLoop()
        {
            bool queueCleared = false;

            while (_cleanupRunning)
            {
                // Ensure we only ever have 1 instance modifying timers or timerQueue
                if (Monitor.TryEnter(_timerQueueClearLock))
                {
                    queueCleared = ClearTimerQueue();

                    // Calculate before foreach to mitigate redundant calculations
                    long cleanupBefore = PerformanceCounter.ElapsedTicks - _history;
                    long preserveStart = _preserve - _history;

                    // Each cleanup is self contained so run in parallel for maximum efficiency
                    Parallel.ForEach(Timers, (t) => t.Value.Cleanup(cleanupBefore, preserveStart, _preserve));

                    Monitor.Exit(_timerQueueClearLock);
                }

                // Only sleep if queue was sucessfully cleared
                if (queueCleared)
                {
                    Thread.Sleep(5);
                }
            }
        }

        private bool ClearTimerQueue()
        {
            int count = 0;

            while (_timerQueue.TryDequeue(out var item))
            {
                if (!Timers.TryGetValue(item.Config, out var value))
                {
                    value = new TimingInfo();
                    Timers.Add(item.Config, value);
                }

                if (item.IsBegin)
                {
                    value.Begin(item.Time);
                }
                else
                {
                    value.End(item.Time);
                }

                // Don't block for too long as memory disposal is blocked while this function runs
                if (count++ > 10000)
                {
                    return false;
                }
            }

            return true;
        }

        public void FlagTime(TimingFlagType flagType)
        {
            int flagId = (int)flagType;

            _timingFlags[_timingFlagIndex] = new TimingFlag()
            {
                FlagType  = flagType,
                Timestamp = PerformanceCounter.ElapsedTicks
            };

            _timingFlagCount = Math.Max(_timingFlagCount + 1, _maxFlags);

            // Work out average
            if (_timingFlagLast[flagId] != 0)
            {
                _timingFlagLastDelta[flagId] = _timingFlags[_timingFlagIndex].Timestamp - _timingFlagLast[flagId];
                _timingFlagAverages[flagId]  = (_timingFlagAverages[flagId] == 0) ? _timingFlagLastDelta[flagId] :
                                                                                   (_timingFlagLastDelta[flagId] + _timingFlagAverages[flagId]) >> 1;
            }
            _timingFlagLast[flagId] = _timingFlags[_timingFlagIndex].Timestamp;

            // Notify subscribers
            _timingFlagCallback?.Invoke(_timingFlags[_timingFlagIndex]);

            if (++_timingFlagIndex >= _maxFlags)
            {
                _timingFlagIndex = 0;
            }
        }

        public void BeginProfile(ProfileConfig config)
        {
            _timerQueue.Enqueue(new TimerQueueValue()
            {
                Config  = config,
                IsBegin = true,
                Time    = PerformanceCounter.ElapsedTicks,
            });
        }

        public void EndProfile(ProfileConfig config)
        {
            _timerQueue.Enqueue(new TimerQueueValue()
            {
                Config  = config,
                IsBegin = false,
                Time    = PerformanceCounter.ElapsedTicks,
            });
        }

        public string GetSession()
        {
            // Can be called from multiple threads so we need to ensure no duplicate sessions are generated
            return Interlocked.Increment(ref _sessionCounter).ToString();
        }

        public List<KeyValuePair<ProfileConfig, TimingInfo>> GetProfilingData()
        {
            _preserve = PerformanceCounter.ElapsedTicks;

            lock (_timerQueueClearLock)
            {
                ClearTimerQueue();
                return Timers.ToList();
            }
        }

        public TimingFlag[] GetTimingFlags()
        {
            int count = Math.Max(_timingFlagCount, _maxFlags);
            TimingFlag[] outFlags = new TimingFlag[count];
            
            for (int i = 0, sourceIndex = _timingFlagIndex; i < count; i++, sourceIndex++)
            {
                if (sourceIndex >= _maxFlags)
                    sourceIndex = 0;
                outFlags[i] = _timingFlags[sourceIndex];
            }

            return outFlags;
        }

        public (long[], long[]) GetTimingAveragesAndLast()
        {
            return (_timingFlagAverages, _timingFlagLastDelta);
        }

        public void RegisterFlagReciever(Action<TimingFlag> reciever)
        {
            _timingFlagCallback = reciever;
        }

        public void Dispose()
        {
            _cleanupRunning = false;
            _cleanupThread.Join();
        }
    }
}
