namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// GPU commands FIFO processor commands.
    /// </summary>
    enum NvGpuFifoMeth
    {
        BindChannel            = 0,
        WaitForIdle            = 0x44,
        SetMacroUploadAddress  = 0x45,
        SendMacroCodeData      = 0x46,
        SetMacroBindingIndex   = 0x47,
        BindMacro              = 0x48,
        SetMmeShadowRamControl = 0x49
    }
}