using System;

namespace Ryujinx.Core.OsHle.Handles
{
    class HDomain : HSession, IDisposable
    {
        private IdDictionary Objects;

        public HDomain(HSession Session) : base(Session)
        {
            Objects = new IdDictionary();
        }

        public int Add(object Obj)
        {
            return Objects.Add(Obj);
        }

        public bool Delete(int Id)
        {
            return Objects.Delete(Id);
        }

        public object GetObject(int Id)
        {
            return Objects.GetData(Id);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                foreach (object Obj in Objects)
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