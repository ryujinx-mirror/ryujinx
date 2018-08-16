using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KProcessHandleTable
    {
        private IdDictionary Handles;

        public KProcessHandleTable()
        {
            Handles = new IdDictionary();
        }

        public int OpenHandle(object Obj)
        {
            return Handles.Add(Obj);
        }

        public T GetData<T>(int Handle)
        {
            return Handles.GetData<T>(Handle);
        }

        public object CloseHandle(int Handle)
        {
            return Handles.Delete(Handle);
        }

        public ICollection<object> Clear()
        {
            return Handles.Clear();
        }
    }
}