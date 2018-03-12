using System;

namespace Ryujinx.Core.OsHle.Handles
{
    class KProcessHandleTable : IDisposable
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

        public bool ReplaceData(int Id, object Data)
        {
            return Handles.ReplaceData(Id, Data);
        }

        public bool CloseHandle(int Handle)
        {
            object Data = Handles.GetData(Handle);

            if (Data is HTransferMem TMem)
            {
                TMem.Memory.Manager.Reprotect(
                    TMem.Position,
                    TMem.Size,
                    TMem.Perm);
            }

            return Handles.Delete(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                foreach (object Obj in Handles)
                {
                    if (Obj is IDisposable DisposableObj)
                    {
                        DisposableObj.Dispose();
                    }
                }
            }
        }
    }
}