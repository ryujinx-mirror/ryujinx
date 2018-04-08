namespace Ryujinx.Graphics.Gpu
{
    enum NvGpuFifoMeth
    {
        BindChannel           = 0,
        SetMacroUploadAddress = 0x45,
        SendMacroCodeData     = 0x46,
        SetMacroBindingIndex  = 0x47,
        BindMacro             = 0x48
    }
}