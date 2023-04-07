namespace Ryujinx.HLE.HOS.Services.Lbl
{
    [Service("lbl")]
    class LblControllerServer : ILblController
    {
        private bool  _vrModeEnabled;
        private float _currentBrightnessSettingForVrMode;

        public LblControllerServer(ServiceCtx context) : base(context) { }

        protected override void SetCurrentBrightnessSettingForVrMode(float currentBrightnessSettingForVrMode)
        {
            if (float.IsNaN(currentBrightnessSettingForVrMode) || float.IsInfinity(currentBrightnessSettingForVrMode))
            {
                _currentBrightnessSettingForVrMode = 0.0f;

                return;
            }

            _currentBrightnessSettingForVrMode = currentBrightnessSettingForVrMode;
        }

        protected override float GetCurrentBrightnessSettingForVrMode()
        {
            if (float.IsNaN(_currentBrightnessSettingForVrMode) || float.IsInfinity(_currentBrightnessSettingForVrMode))
            {
                return 0.0f;
            }

            return _currentBrightnessSettingForVrMode;
        }

        internal override void EnableVrMode()
        {
            _vrModeEnabled = true;

            // NOTE: Service check _vrModeEnabled field value in a thread and then change the screen brightness.
            //       Since we don't support that. It's fine to do nothing.
        }

        internal override void DisableVrMode()
        {
            _vrModeEnabled = false;

            // NOTE: Service check _vrModeEnabled field value in a thread and then change the screen brightness.
            //       Since we don't support that. It's fine to do nothing.
        }

        protected override bool IsVrModeEnabled()
        {
            return _vrModeEnabled;
        }
    }
}
