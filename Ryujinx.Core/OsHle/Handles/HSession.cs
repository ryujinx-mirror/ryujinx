using Ryujinx.Core.OsHle.IpcServices;

namespace Ryujinx.Core.OsHle.Handles
{
    class HSession
    {
        public IIpcService Service { get; private set; }

        public bool IsInitialized { get; private set; }

        public int State { get; set; }

        public HSession(IIpcService Service)
        {
            this.Service = Service;
        }

        public HSession(HSession Session)
        {
            Service       = Session.Service;
            IsInitialized = Session.IsInitialized;
        }

        public void Initialize()
        {
            IsInitialized = true;
        }
    }
}