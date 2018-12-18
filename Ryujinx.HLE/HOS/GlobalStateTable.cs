using Ryujinx.HLE.HOS.Kernel.Process;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS
{
    class GlobalStateTable
    {
        private ConcurrentDictionary<KProcess, IdDictionary> _dictByProcess;

        public GlobalStateTable()
        {
            _dictByProcess = new ConcurrentDictionary<KProcess, IdDictionary>();
        }

        public bool Add(KProcess process, int id, object data)
        {
            IdDictionary dict = _dictByProcess.GetOrAdd(process, (key) => new IdDictionary());

            return dict.Add(id, data);
        }

        public int Add(KProcess process, object data)
        {
            IdDictionary dict = _dictByProcess.GetOrAdd(process, (key) => new IdDictionary());

            return dict.Add(data);
        }

        public object GetData(KProcess process, int id)
        {
            if (_dictByProcess.TryGetValue(process, out IdDictionary dict))
            {
                return dict.GetData(id);
            }

            return null;
        }

        public T GetData<T>(KProcess process, int id)
        {
            if (_dictByProcess.TryGetValue(process, out IdDictionary dict))
            {
                return dict.GetData<T>(id);
            }

            return default(T);
        }

        public object Delete(KProcess process, int id)
        {
            if (_dictByProcess.TryGetValue(process, out IdDictionary dict))
            {
                return dict.Delete(id);
            }

            return null;
        }

        public ICollection<object> DeleteProcess(KProcess process)
        {
            if (_dictByProcess.TryRemove(process, out IdDictionary dict))
            {
                return dict.Clear();
            }

            return null;
        }
    }
}