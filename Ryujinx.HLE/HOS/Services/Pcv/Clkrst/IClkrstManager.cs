namespace Ryujinx.HLE.HOS.Services.Pcv.Clkrst
{
    [Service("clkrst")]   // 8.0.0+
    [Service("clkrst:i")] // 8.0.0+
    class IClkrstManager : IpcService
    {
        public IClkrstManager(ServiceCtx context) { }
    }
}