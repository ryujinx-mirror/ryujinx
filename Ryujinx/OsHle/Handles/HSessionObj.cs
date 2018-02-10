using System;

namespace Ryujinx.OsHle.Handles
{
    class HSessionObj : HSession, IDisposable
    {
        public object Obj { get; private set; }

        public HSessionObj(HSession Session, object Obj) : base(Session)
        {
            this.Obj = Obj;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if(Disposing && Obj != null)
            {
                if(Obj is IDisposable DisposableObj)
                {
                    DisposableObj.Dispose();
                }
            }
        }
    }
}