namespace Ryujinx.OsHle.Handles
{
    class HSessionObj : HSession
    {
        public object Obj { get; private set; }

        public HSessionObj(HSession Session, object Obj) : base(Session)
        {
            this.Obj = Obj;
        }
    }
}