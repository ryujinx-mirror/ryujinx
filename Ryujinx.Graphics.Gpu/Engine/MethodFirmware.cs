using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void FirmwareCall4(GpuState state, int argument)
        {
            state.Write(0xd00, 1);
        }
    }
}
