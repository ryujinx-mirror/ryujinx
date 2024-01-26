namespace Ryujinx.Graphics.GAL
{
    public readonly struct HardwareInfo
    {
        public string GpuVendor { get; }
        public string GpuModel { get; }
        public string GpuDriver { get; }

        public HardwareInfo(string gpuVendor, string gpuModel, string gpuDriver)
        {
            GpuVendor = gpuVendor;
            GpuModel = gpuModel;
            GpuDriver = gpuDriver;
        }
    }
}
