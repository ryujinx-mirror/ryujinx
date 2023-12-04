using Ryujinx.Horizon.Sdk.OsTypes;
using Ryujinx.Horizon.Sdk.Sf.Cmif;

namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    class ServerSession : MultiWaitHolderOfHandle
    {
        public ServiceObjectHolder ServiceObjectHolder { get; set; }
        public PointerAndSize PointerBuffer { get; set; }
        public PointerAndSize SavedMessage { get; set; }
        public int SessionIndex { get; }
        public int SessionHandle { get; }
        public bool IsClosed { get; set; }
        public bool HasReceived { get; set; }

        public ServerSession(int index, int handle, ServiceObjectHolder obj) : base(handle)
        {
            ServiceObjectHolder = obj;
            SessionIndex = index;
            SessionHandle = handle;
        }
    }
}
