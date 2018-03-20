using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle
{
    class GlobalStateTable
    {
        private ConcurrentDictionary<Process, IdDictionary> DictByProcess;

        public GlobalStateTable()
        {
            DictByProcess = new ConcurrentDictionary<Process, IdDictionary>();
        }

        public bool Add(Process Process, int Id, object Data)
        {
            IdDictionary Dict = DictByProcess.GetOrAdd(Process, (Key) => new IdDictionary());

            return Dict.Add(Id, Data);
        }

        public int Add(Process Process, object Data)
        {
            IdDictionary Dict = DictByProcess.GetOrAdd(Process, (Key) => new IdDictionary());

            return Dict.Add(Data);
        }

        public object GetData(Process Process, int Id)
        {
            if (DictByProcess.TryGetValue(Process, out IdDictionary Dict))
            {
                return Dict.GetData(Id);
            }

            return null;
        }

        public T GetData<T>(Process Process, int Id)
        {
            if (DictByProcess.TryGetValue(Process, out IdDictionary Dict))
            {
                return Dict.GetData<T>(Id);
            }

            return default(T);
        }

        public object Delete(Process Process, int Id)
        {
            if (DictByProcess.TryGetValue(Process, out IdDictionary Dict))
            {
                return Dict.Delete(Id);
            }

            return null;
        }

        public ICollection<object> DeleteProcess(Process Process)
        {
            if (DictByProcess.TryRemove(Process, out IdDictionary Dict))
            {
                return Dict.Clear();
            }

            return null;
        }
    }
}