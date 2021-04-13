namespace Ryujinx.HLE.HOS.Services.Lbl
{
    abstract class ILblController : IpcService
    {
        public ILblController(ServiceCtx context) { }

        protected abstract void SetCurrentBrightnessSettingForVrMode(float currentBrightnessSettingForVrMode);
        protected abstract float GetCurrentBrightnessSettingForVrMode();
        internal abstract void EnableVrMode();
        internal abstract void DisableVrMode();
        protected abstract bool IsVrModeEnabled();

        [CommandHipc(17)]
        // SetBrightnessReflectionDelayLevel(float, float)
        public ResultCode SetBrightnessReflectionDelayLevel(ServiceCtx context)
        {
            return ResultCode.Success;
        }

        [CommandHipc(18)]
        // GetBrightnessReflectionDelayLevel(float) -> float
        public ResultCode GetBrightnessReflectionDelayLevel(ServiceCtx context)
        {
            context.ResponseData.Write(0.0f);

            return ResultCode.Success;
        }

        [CommandHipc(21)]
        // SetCurrentAmbientLightSensorMapping(unknown<0xC>)
        public ResultCode SetCurrentAmbientLightSensorMapping(ServiceCtx context)
        {
            return ResultCode.Success;
        }

        [CommandHipc(22)]
        // GetCurrentAmbientLightSensorMapping() -> unknown<0xC>
        public ResultCode GetCurrentAmbientLightSensorMapping(ServiceCtx context)
        {
            return ResultCode.Success;
        }

        [CommandHipc(24)] // 3.0.0+
        // SetCurrentBrightnessSettingForVrMode(float)
        public ResultCode SetCurrentBrightnessSettingForVrMode(ServiceCtx context)
        {
            float currentBrightnessSettingForVrMode = context.RequestData.ReadSingle();

            SetCurrentBrightnessSettingForVrMode(currentBrightnessSettingForVrMode);

            return ResultCode.Success;
        }

        [CommandHipc(25)] // 3.0.0+
        // GetCurrentBrightnessSettingForVrMode() -> float
        public ResultCode GetCurrentBrightnessSettingForVrMode(ServiceCtx context)
        {
            float currentBrightnessSettingForVrMode = GetCurrentBrightnessSettingForVrMode();

            context.ResponseData.Write(currentBrightnessSettingForVrMode);

            return ResultCode.Success;
        }

        [CommandHipc(26)] // 3.0.0+
        // EnableVrMode()
        public ResultCode EnableVrMode(ServiceCtx context)
        {
            EnableVrMode();

            return ResultCode.Success;
        }

        [CommandHipc(27)] // 3.0.0+
        // DisableVrMode()
        public ResultCode DisableVrMode(ServiceCtx context)
        {
            DisableVrMode();

            return ResultCode.Success;
        }

        [CommandHipc(28)] // 3.0.0+
        // IsVrModeEnabled() -> bool
        public ResultCode IsVrModeEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(IsVrModeEnabled());

            return ResultCode.Success;
        }
    }
}