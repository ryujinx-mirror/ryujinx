using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad;

namespace Ryujinx.HLE.UI.Input
{
    delegate void NpadButtonHandler(int npadIndex, NpadButton button);
}
