using System;
using System.Collections.Generic;

namespace Ryujinx.Profiler
{
    public struct Timestamp
    {
        public long BeginTime;
        public long EndTime;
    }

    public class TimingInfo
    {
        // Timestamps
        public long TotalTime { get; set; }
        public long Instant   { get; set; }

        // Measurement counts
        public int  Count        { get; set; }
        public int  InstantCount { get; set; }

        // Work out average
        public long AverageTime => (Count == 0) ? -1 : TotalTime / Count;

        // Intentionally not locked as it's only a get count
        public bool IsActive => _timestamps.Count > 0;

        public long BeginTime
        {
            get
            {
                lock (_timestampLock)
                {
                    if (_depth > 0)
                    {
                        return _currentTimestamp.BeginTime;
                    }

                    return -1;
                }
            }
        }

        // Timestamp collection
        private List<Timestamp> _timestamps;
        private readonly object _timestampLock     = new object();
        private readonly object _timestampListLock = new object();
        private Timestamp _currentTimestamp;

        // Depth of current timer,
        // each begin call increments and each end call decrements
        private int _depth;

        public TimingInfo()
        {
            _timestamps = new List<Timestamp>();
            _depth      = 0;
        }

        public void Begin(long beginTime)
        {
            lock (_timestampLock)
            {
                // Finish current timestamp if already running
                if (_depth > 0)
                {
                    EndUnsafe(beginTime);
                }

                BeginUnsafe(beginTime);
                _depth++;
            }
        }

        private void BeginUnsafe(long beginTime)
        {
            _currentTimestamp.BeginTime = beginTime;
            _currentTimestamp.EndTime   = -1;
        }

        public void End(long endTime)
        {
            lock (_timestampLock)
            {
                _depth--;

                if (_depth < 0)
                {
                    throw new Exception("Timing info end called without corresponding begin");
                }

                EndUnsafe(endTime);

                // Still have others using this timing info so recreate start for them
                if (_depth > 0)
                {
                    BeginUnsafe(endTime);
                }
            }
        }

        private void EndUnsafe(long endTime)
        {
            _currentTimestamp.EndTime = endTime;
            lock (_timestampListLock)
            {
                _timestamps.Add(_currentTimestamp);
            }

            var delta  = _currentTimestamp.EndTime - _currentTimestamp.BeginTime;
            TotalTime += delta;
            Instant   += delta;

            Count++;
            InstantCount++;
        }

        // Remove any timestamps before given timestamp to free memory
        public void Cleanup(long before, long preserveStart, long preserveEnd)
        {
            lock (_timestampListLock)
            {
                int toRemove        = 0;
                int toPreserveStart = 0;
                int toPreserveLen   = 0;

                for (int i = 0; i < _timestamps.Count; i++)
                {
                    if (_timestamps[i].EndTime < preserveStart)
                    {
                        toPreserveStart++;
                        InstantCount--;
                        Instant -= _timestamps[i].EndTime - _timestamps[i].BeginTime;
                    }
                    else if (_timestamps[i].EndTime < preserveEnd)
                    {
                        toPreserveLen++;
                    }
                    else if (_timestamps[i].EndTime < before)
                    {
                        toRemove++;
                        InstantCount--;
                        Instant -= _timestamps[i].EndTime - _timestamps[i].BeginTime;
                    }
                    else
                    {
                        // Assume timestamps are in chronological order so no more need to be removed
                        break;
                    }
                }

                if (toPreserveStart > 0)
                {
                    _timestamps.RemoveRange(0, toPreserveStart);
                }

                if (toRemove > 0)
                {
                    _timestamps.RemoveRange(toPreserveLen, toRemove);
                }
            }
        }

        public Timestamp[] GetAllTimestamps()
        {
            lock (_timestampListLock)
            {
                Timestamp[] returnTimestamps = new Timestamp[_timestamps.Count];
                _timestamps.CopyTo(returnTimestamps);
                return returnTimestamps;
            }
        }
    }
}
