using Ryujinx.HLE.HOS.Kernel;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS
{
    class GlobalStateTable
    {
        private ConcurrentDictionary<KProcess, IdDictionary> DictByProcess;

        public GlobalStateTable()
        {
            DictByProcess = new ConcurrentDictionary<KProcess, IdDictionary>();
        }

        public bool Add(KProcess Process, int Id, object Data)
        {
            IdDictionary Dict = DictByProcess.GetOrAdd(Process, (Key) => new IdDictionary());

            return Dict.Add(Id, Data);
        }

        public int Add(KProcess Process, object Data)
        {
            IdDictionary Dict = DictByProcess.GetOrAdd(Process, (Key) => new IdDictionary());

            return Dict.Add(Data);
        }

        public object GetData(KProcess Process, int Id)
        {
            if (DictByProcess.TryGetValue(Process, out IdDictionary Dict))
            {
                return Dict.GetData(Id);
            }

            return null;
        }

        public T GetData<T>(KProcess Process, int Id)
        {
            if (DictByProcess.TryGetValue(Process, out IdDictionary Dict))
            {
                return Dict.GetData<T>(Id);
            }

            return default(T);
        }

        public object Delete(KProcess Process, int Id)
        {
            if (DictByProcess.TryGetValue(Process, out IdDictionary Dict))
            {
                return Dict.Delete(Id);
            }

            return null;
        }

        public ICollection<object> DeleteProcess(KProcess Process)
        {
            if (DictByProcess.TryRemove(Process, out IdDictionary Dict))
            {
                return Dict.Clear();
            }

            return null;
        }
    }
}