using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Hid;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp.UserManager
{
    class Device
    {
        public KEvent ActivateEvent;
        public int    ActivateEventHandle;

        public KEvent DeactivateEvent;
        public int    DeactivateEventHandle;

        public DeviceState State = DeviceState.Unavailable;

        public PlayerIndex Handle;
        public NpadIdType  NpadIdType;
    }
}