using System;
using System.Diagnostics;

namespace Ryujinx.Graphics.Vulkan
{
    internal class AutoFlushCounter
    {
        // How often to flush on framebuffer change.
        private readonly static long FramebufferFlushTimer = Stopwatch.Frequency / 1000;

        private const int MinDrawCountForFlush = 10;
        private const int InitialQueryCountForFlush = 32;

        private long _lastFlush;
        private ulong _lastDrawCount;
        private bool _hasPendingQuery;
        private int _queryCount;

        public void RegisterFlush(ulong drawCount)
        {
            _lastFlush = Stopwatch.GetTimestamp();
            _lastDrawCount = drawCount;

            _hasPendingQuery = false;
        }

        public bool RegisterPendingQuery()
        {
            _hasPendingQuery = true;

            // Interrupt render passes to flush queries, so that early results arrive sooner.
            if (++_queryCount == InitialQueryCountForFlush)
            {
                return true;
            }

            return false;
        }

        public bool ShouldFlushQuery()
        {
            return _hasPendingQuery;
        }

        public bool ShouldFlush(ulong drawCount)
        {
            _queryCount = 0;

            if (_hasPendingQuery)
            {
                return true;
            }

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
    }
}
