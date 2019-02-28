using ChocolArm64.State;
using System.Collections.Concurrent;
using System.Threading;

namespace ChocolArm64.Translation
{
    class TranslatorQueue
    {
        private ConcurrentStack<TranslatorQueueItem>[] _translationQueue;

        private ManualResetEvent _queueDataReceivedEvent;

        private bool _signaled;

        public TranslatorQueue()
        {
            _translationQueue = new ConcurrentStack<TranslatorQueueItem>[(int)TranslationTier.Count];

            for (int prio = 0; prio < _translationQueue.Length; prio++)
            {
                _translationQueue[prio] = new ConcurrentStack<TranslatorQueueItem>();
            }

            _queueDataReceivedEvent = new ManualResetEvent(false);
        }

        public void Enqueue(long position, ExecutionMode mode, TranslationTier tier, bool isComplete)
        {
            TranslatorQueueItem item = new TranslatorQueueItem(position, mode, tier, isComplete);

            ConcurrentStack<TranslatorQueueItem> queue = _translationQueue[(int)tier];

            queue.Push(item);

            _queueDataReceivedEvent.Set();
        }

        public bool TryDequeue(out TranslatorQueueItem item)
        {
            for (int prio = 0; prio < _translationQueue.Length; prio++)
            {
                if (_translationQueue[prio].TryPop(out item))
                {
                    return true;
                }
            }

            item = default(TranslatorQueueItem);

            return false;
        }

        public void WaitForItems()
        {
            _queueDataReceivedEvent.WaitOne();

            lock (_queueDataReceivedEvent)
            {
                if (!_signaled)
                {
                    _queueDataReceivedEvent.Reset();
                }
            }
        }

        public void ForceSignal()
        {
            lock (_queueDataReceivedEvent)
            {
                _signaled = true;

                _queueDataReceivedEvent.Set();
                _queueDataReceivedEvent.Close();
            }
        }
    }
}