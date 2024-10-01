using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Lbl
{
    interface ILblController : IServiceObject
    {
        Result SetBrightnessReflectionDelayLevel(float unknown0, float unknown1);
        Result GetBrightnessReflectionDelayLevel(out float unknown1, float unknown0);
        Result SetCurrentBrightnessMapping(float unknown0, float unknown1, float unknown2);
        Result GetCurrentBrightnessMapping(out float unknown0, out float unknown1, out float unknown2);
        Result SetCurrentAmbientLightSensorMapping(float unknown0, float unknown1, float unknown2);
        Result GetCurrentAmbientLightSensorMapping(out float unknown0, out float unknown1, out float unknown2);
        Result SetCurrentBrightnessSettingForVrMode(float currentBrightnessSettingForVrMode);
        Result GetCurrentBrightnessSettingForVrMode(out float currentBrightnessSettingForVrMode);
        Result EnableVrMode();
        Result DisableVrMode();
        Result IsVrModeEnabled(out bool vrModeEnabled);
    }
}
