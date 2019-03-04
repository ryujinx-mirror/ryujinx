using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Vic
{
    class VideoImageComposer
    {
        private NvGpu _gpu;

        private long _configStructAddress;
        private long _outputSurfaceLumaAddress;
        private long _outputSurfaceChromaUAddress;
        private long _outputSurfaceChromaVAddress;

        public VideoImageComposer(NvGpu gpu)
        {
            _gpu = gpu;
        }

        public void Process(NvGpuVmm vmm, int methodOffset, int[] arguments)
        {
            VideoImageComposerMeth method = (VideoImageComposerMeth)methodOffset;

            switch (method)
            {
                case VideoImageComposerMeth.Execute:
                    Execute(vmm, arguments);
                    break;

                case VideoImageComposerMeth.SetConfigStructOffset:
                    SetConfigStructOffset(vmm, arguments);
                    break;

                case VideoImageComposerMeth.SetOutputSurfaceLumaOffset:
                    SetOutputSurfaceLumaOffset(vmm, arguments);
                    break;

                case VideoImageComposerMeth.SetOutputSurfaceChromaUOffset:
                    SetOutputSurfaceChromaUOffset(vmm, arguments);
                    break;

                case VideoImageComposerMeth.SetOutputSurfaceChromaVOffset:
                    SetOutputSurfaceChromaVOffset(vmm, arguments);
                    break;
            }
        }

        private void Execute(NvGpuVmm vmm, int[] arguments)
        {
            StructUnpacker unpacker = new StructUnpacker(vmm, _configStructAddress + 0x20);

            SurfacePixelFormat pixelFormat = (SurfacePixelFormat)unpacker.Read(7);

            int chromaLocHoriz = unpacker.Read(2);
            int chromaLocVert  = unpacker.Read(2);

            int blockLinearKind       = unpacker.Read(4);
            int blockLinearHeightLog2 = unpacker.Read(4);

            int reserved0 = unpacker.Read(3);
            int reserved1 = unpacker.Read(10);

            int surfaceWidthMinus1  = unpacker.Read(14);
            int surfaceHeightMinus1 = unpacker.Read(14);

            int gobBlockHeight = 1 << blockLinearHeightLog2;

            int surfaceWidth  = surfaceWidthMinus1  + 1;
            int surfaceHeight = surfaceHeightMinus1 + 1;

            SurfaceOutputConfig outputConfig = new SurfaceOutputConfig(
                pixelFormat,
                surfaceWidth,
                surfaceHeight,
                gobBlockHeight,
                _outputSurfaceLumaAddress,
                _outputSurfaceChromaUAddress,
                _outputSurfaceChromaVAddress);

            _gpu.VideoDecoder.CopyPlanes(vmm, outputConfig);
        }

        private void SetConfigStructOffset(NvGpuVmm vmm, int[] arguments)
        {
            _configStructAddress = GetAddress(arguments);
        }

        private void SetOutputSurfaceLumaOffset(NvGpuVmm vmm, int[] arguments)
        {
            _outputSurfaceLumaAddress = GetAddress(arguments);
        }

        private void SetOutputSurfaceChromaUOffset(NvGpuVmm vmm, int[] arguments)
        {
            _outputSurfaceChromaUAddress = GetAddress(arguments);
        }

        private void SetOutputSurfaceChromaVOffset(NvGpuVmm vmm, int[] arguments)
        {
            _outputSurfaceChromaVAddress = GetAddress(arguments);
        }

        private static long GetAddress(int[] arguments)
        {
            return (long)(uint)arguments[0] << 8;
        }
    }
}