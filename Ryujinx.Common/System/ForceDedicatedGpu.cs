using System.Runtime.InteropServices;

namespace Ryujinx.Common.System
{
    public static class ForceDedicatedGpu
    {
        public static void Nvidia()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // NOTE: If the DLL exists, we can load it to force the usage of the dedicated Nvidia Gpu.
                NativeLibrary.TryLoad("nvapi64.dll", out _);
            }
        }
    }
}