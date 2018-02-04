using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Utilities
{
    class IdPoolWithObj : IEnumerable<KeyValuePair<int, object>>
    {
        private IdPool Ids;

        private ConcurrentDictionary<int, object> Objs;

        public IdPoolWithObj()
        {
            Ids = new IdPool();

            Objs = new ConcurrentDictionary<int, object>();
        }

        public int GenerateId(object Data)
        {
            int Id = Ids.GenerateId();

            if (Id == -1 || !Objs.TryAdd(Id, Data))
            {
                throw new InvalidOperationException();
            }

            return Id;
        }

        public bool ReplaceData(int Id, object Data)
        {
            if (Objs.ContainsKey(Id))
            {
                Objs[Id] = Data;

                return true;
            }

            return false;
        }

        public T GetData<T>(int Id)
        {
            if (Objs.TryGetValue(Id, out object Data) && Data is T)
            {
                return (T)Data;
            }

            return default(T);
        }

        public void Delete(int Id)
        {
            if (Objs.TryRemove(Id, out object Obj))
            {
                if (Obj is IDisposable DisposableObj)
                {
                    DisposableObj.Dispose();
                }

                Ids.DeleteId(Id);
            }
        }

        IEnumerator<KeyValuePair<int, object>> IEnumerable<KeyValuePair<int, object>>.GetEnumerator()
        {
            return Objs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Objs.GetEnumerator();
        }
    }
}