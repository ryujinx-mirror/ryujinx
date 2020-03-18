using System.Collections.Concurrent;

namespace ARMeilleure.Translation
{
    class PriorityQueue<T>
    {
        private ConcurrentStack<T>[] _queues;

        public PriorityQueue(int priorities)
        {
            _queues = new ConcurrentStack<T>[priorities];

            for (int index = 0; index < priorities; index++)
            {
                _queues[index] = new ConcurrentStack<T>();
            }
        }

        public void Enqueue(int priority, T value)
        {
            _queues[priority].Push(value);
        }

        public bool TryDequeue(out T value)
        {
            for (int index = 0; index < _queues.Length; index++)
            {
                if (_queues[index].TryPop(out value))
                {
                    return true;
                }
            }

            value = default(T);

            return false;
        }
    }
}