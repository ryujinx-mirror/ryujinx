using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS
{
    class IdDictionary
    {
        private readonly ConcurrentDictionary<int, object> _objs;

        public ICollection<object> Values => _objs.Values;

        public IdDictionary()
        {
            _objs = new ConcurrentDictionary<int, object>();
        }

        public bool Add(int id, object data)
        {
            return _objs.TryAdd(id, data);
        }

        public int Add(object data)
        {
            for (int id = 1; id < int.MaxValue; id++)
            {
                if (_objs.TryAdd(id, data))
                {
                    return id;
                }
            }

            throw new InvalidOperationException();
        }

        public object GetData(int id)
        {
            if (_objs.TryGetValue(id, out object data))
            {
                return data;
            }

            return null;
        }

        public T GetData<T>(int id)
        {
            if (_objs.TryGetValue(id, out object dataObject) && dataObject is T data)
            {
                return data;
            }

            return default;
        }

        public object Delete(int id)
        {
            if (_objs.TryRemove(id, out object obj))
            {
                return obj;
            }

            return null;
        }

        public ICollection<object> Clear()
        {
            ICollection<object> values = _objs.Values;

            _objs.Clear();

            return values;
        }
    }
}
