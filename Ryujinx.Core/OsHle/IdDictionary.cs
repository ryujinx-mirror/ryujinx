using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle
{
    class IdDictionary
    {
        private ConcurrentDictionary<int, object> Objs;

        private int FreeIdHint = 1;

        public IdDictionary()
        {
            Objs = new ConcurrentDictionary<int, object>();
        }

        public bool Add(int Id, object Data)
        {
            return Objs.TryAdd(Id, Data);
        }

        public int Add(object Data)
        {
            if (Objs.TryAdd(FreeIdHint, Data))
            {
                return FreeIdHint++;
            }

            return AddSlow(Data);
        }

        private int AddSlow(object Data)
        {
            for (int Id = 1; Id < int.MaxValue; Id++)
            {
                if (Objs.TryAdd(Id, Data))
                {
                    return Id;
                }
            }

            throw new InvalidOperationException();
        }

        public object GetData(int Id)
        {
            if (Objs.TryGetValue(Id, out object Data))
            {
                return Data;
            }

            return null;
        }

        public T GetData<T>(int Id)
        {
            if (Objs.TryGetValue(Id, out object Data) && Data is T)
            {
                return (T)Data;
            }

            return default(T);
        }

        public object Delete(int Id)
        {
            if (Objs.TryRemove(Id, out object Obj))
            {
                FreeIdHint = Id;

                return Obj;
            }

            return null;
        }

        public ICollection<object> Clear()
        {
            ICollection<object> Values = Objs.Values;

            Objs.Clear();

            return Values;
        }
    }
}