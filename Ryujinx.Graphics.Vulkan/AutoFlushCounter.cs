using System;
using System.Diagnostics;
using System.Linq;

namespace Ryujinx.Graphics.Vulkan
{
    internal class AutoFlushCounter
    {
        // How often to flush on framebuffer change.
        private readonly static long FramebufferFlushTimer = Stopwatch.Frequency / 1000;

        private const int MinDrawCountForFlush = 10;
        private const int MinConsecutiveQueryForFlush = 10;
        private const int InitialQueryCountForFlush = 32;

        private long _lastFlush;
        private ulong _lastDrawCount;
        private bool _hasPendingQuery;
        private int _consecutiveQueries;
        private int _queryCount;

        private int[] _queryCountHistory = new int[3];
        private int _queryCountHistoryIndex;
        private int _remainingQueries;

        public void RegisterFlush(ulong drawCount)
        {
            _lastFlush = Stopwatch.GetTimestamp();
            _lastDrawCount = drawCount;

            _hasPendingQuery = false;
            _consecutiveQueries = 0;
        }

        public bool RegisterPendingQuery()
        {
            _hasPendingQuery = true;
            _consecutiveQueries++;
            _remainingQueries--;

            _queryCountHistory[_queryCountHistoryIndex]++;

            // Interrupt render passes to flush queries, so that early results arrive sooner.
            if (++_queryCount == InitialQueryCountForFlush)
            {
                return true;
            }

            return false;
        }

        public int GetRemainingQueries()
        {
            if (_remainingQueries <= 0)
            {
                _remainingQueries = 16;
            }

            if (_queryCount < InitialQueryCountForFlush)
            {
                return Math.Min(InitialQueryCountForFlush - _queryCount, _remainingQueries);
            }

            return _remainingQueries;
        }

        public bool ShouldFlushQuery()
        {
            return _hasPendingQuery;
        }

        public bool ShouldFlushAttachmentChange(ulong drawCount)
        {
            _queryCount = 0;

            // Flush when there's an attachment change out of a large block of queries.
            if (_consecutiveQueries > MinConsecutiveQueryForFlush)
            {
                return true;
            }

            _consecutiveQueries = 0;

            long draws = (long)(drawCount - _lastDrawCount);

            if (draws < MinDrawCountForFlush)
            {
                if (draws == 0)
                {
                    _lastFlush = Stopwatch.GetTimestamp();
                }

                return false;
            }

            long flushTimeout = FramebufferFlushTimer;

            long now = Stopwatch.GetTimestamp();

            return now > _lastFlush + flushTimeout;
        }

        public void Present()
        {
            _queryCountHistoryIndex = (_queryCountHistoryIndex + 1) % 3;

            _remainingQueries = _queryCountHistory.Max() + 10;

            _queryCountHistory[_queryCountHistoryIndex] = 0;
        }
    }
}
