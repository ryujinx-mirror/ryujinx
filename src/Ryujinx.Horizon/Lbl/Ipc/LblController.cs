using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Lbl;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Lbl.Ipc
{
    partial class LblController : ILblController
    {
        private bool _vrModeEnabled;
        private float _currentBrightnessSettingForVrMode;

        [CmifCommand(17)]
        public Result SetBrightnessReflectionDelayLevel(float unknown0, float unknown1)
        {
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(18)]
        public Result GetBrightnessReflectionDelayLevel(out float unknown1, float unknown0)
        {
            // NOTE: Stubbed in system module.

            unknown1 = 0.0f;

            return Result.Success;
        }

        [CmifCommand(19)]
        public Result SetCurrentBrightnessMapping(float unknown0, float unknown1, float unknown2)
        {
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(20)]
        public Result GetCurrentBrightnessMapping(out float unknown0, out float unknown1, out float unknown2)
        {
            // NOTE: Stubbed in system module.

            unknown0 = 0.0f;
            unknown1 = 0.0f;
            unknown2 = 0.0f;

            return Result.Success;
        }

        [CmifCommand(21)]
        public Result SetCurrentAmbientLightSensorMapping(float unknown0, float unknown1, float unknown2)
        {
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(22)]
        public Result GetCurrentAmbientLightSensorMapping(out float unknown0, out float unknown1, out float unknown2)
        {
            // NOTE: Stubbed in system module.

            unknown0 = 0.0f;
            unknown1 = 0.0f;
            unknown2 = 0.0f;

            return Result.Success;
        }

        [CmifCommand(24)]
        public Result SetCurrentBrightnessSettingForVrMode(float currentBrightnessSettingForVrMode)
        {
            if (float.IsNaN(currentBrightnessSettingForVrMode) || float.IsInfinity(currentBrightnessSettingForVrMode))
            {
                _currentBrightnessSettingForVrMode = 0.0f;
            }
            else
            {
                _currentBrightnessSettingForVrMode = currentBrightnessSettingForVrMode;
            }

            return Result.Success;
        }

        [CmifCommand(25)]
        public Result GetCurrentBrightnessSettingForVrMode(out float currentBrightnessSettingForVrMode)
        {
            if (float.IsNaN(_currentBrightnessSettingForVrMode) || float.IsInfinity(_currentBrightnessSettingForVrMode))
            {
                currentBrightnessSettingForVrMode = 0.0f;
            }
            else
            {
                currentBrightnessSettingForVrMode = _currentBrightnessSettingForVrMode;
            }

            return Result.Success;
        }

        [CmifCommand(26)]
        public Result EnableVrMode()
        {
            _vrModeEnabled = true;

            // NOTE: The service checks _vrModeEnabled field value in a thread and then changes the screen brightness.
            //       Since we don't support that, it's fine to do nothing.

            return Result.Success;
        }

        [CmifCommand(27)]
        public Result DisableVrMode()
        {
            _vrModeEnabled = false;

            // NOTE: The service checks _vrModeEnabled field value in a thread and then changes the screen brightness.
            //       Since we don't support that, it's fine to do nothing.

            return Result.Success;
        }

        [CmifCommand(28)]
        public Result IsVrModeEnabled(out bool vrModeEnabled)
        {
            vrModeEnabled = _vrModeEnabled;

            return Result.Success;
        }
    }
}
