using System.Collections.Concurrent;

namespace ARMeilleure.Translation
{
    class PriorityQueue<T>
    {
        private ConcurrentQueue<T>[] _queues;

        public PriorityQueue(int priorities)
        {
            _queues = new ConcurrentQueue<T>[priorities];

            for (int index = 0; index < priorities; index++)
            {
                _queues[index] = new ConcurrentQueue<T>();
            }
        }

        public void Enqueue(int priority, T value)
        {
            _queues[priority].Enqueue(value);
        }

        public bool TryDequeue(out T value)
        {
            for (int index = 0; index < _queues.Length; index++)
            {
                if (_queues[index].TryDequeue(out value))
                {
                    return true;
                }
            }

            value = default(T);

            return false;
        }
    }
}