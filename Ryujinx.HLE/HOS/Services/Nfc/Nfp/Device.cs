using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.Input;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    class Device
    {
        public KEvent ActivateEvent;
        public int    ActivateEventHandle;

        public KEvent DeactivateEvent;
        public int    DeactivateEventHandle;

        public DeviceState State = DeviceState.Unavailable;

        public ControllerId Handle;
        public NpadIdType   NpadIdType;
    }
}