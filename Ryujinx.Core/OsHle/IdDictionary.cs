using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle
{
    class IdDictionary : IEnumerable<object>
    {
        private ConcurrentDictionary<int, object> Objs;

        private int FreeIdHint = 1;

        public IdDictionary()
        {
            Objs = new ConcurrentDictionary<int, object>();
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

        public bool ReplaceData(int Id, object Data)
        {
            if (Objs.ContainsKey(Id))
            {
                Objs[Id] = Data;

                return true;
            }

            return false;
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

        public bool Delete(int Id)
        {
            if (Objs.TryRemove(Id, out object Obj))
            {
                if (Obj is IDisposable DisposableObj)
                {
                    DisposableObj.Dispose();
                }

                FreeIdHint = Id;

                return true;
            }

            return false;
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return Objs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Objs.Values.GetEnumerator();
        }
    }
}