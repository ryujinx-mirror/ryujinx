using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.VDec;

namespace Ryujinx.Graphics.Vic
{
    class VideoImageComposer
    {
        private ulong _configStructAddress;
        private ulong _outputSurfaceLumaAddress;
        private ulong _outputSurfaceChromaUAddress;
        private ulong _outputSurfaceChromaVAddress;

        private VideoDecoder _vdec;

        public VideoImageComposer(VideoDecoder vdec)
        {
            _vdec = vdec;
        }

        public void Process(GpuContext gpu, int methodOffset, int[] arguments)
        {
            VideoImageComposerMeth method = (VideoImageComposerMeth)methodOffset;

            switch (method)
            {
                case VideoImageComposerMeth.Execute:                       Execute(gpu);                             break;
                case VideoImageComposerMeth.SetConfigStructOffset:         SetConfigStructOffset(arguments);         break;
                case VideoImageComposerMeth.SetOutputSurfaceLumaOffset:    SetOutputSurfaceLumaOffset(arguments);    break;
                case VideoImageComposerMeth.SetOutputSurfaceChromaUOffset: SetOutputSurfaceChromaUOffset(arguments); break;
                case VideoImageComposerMeth.SetOutputSurfaceChromaVOffset: SetOutputSurfaceChromaVOffset(arguments); break;
            }
        }

        private void Execute(GpuContext gpu)
        {
            StructUnpacker unpacker = new StructUnpacker(gpu.MemoryAccessor, _configStructAddress + 0x20);

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

            _vdec.CopyPlanes(gpu, outputConfig);
        }

        private void SetConfigStructOffset(int[] arguments)
        {
            _configStructAddress = GetAddress(arguments);
        }

        private void SetOutputSurfaceLumaOffset(int[] arguments)
        {
            _outputSurfaceLumaAddress = GetAddress(arguments);
        }

        private void SetOutputSurfaceChromaUOffset(int[] arguments)
        {
            _outputSurfaceChromaUAddress = GetAddress(arguments);
        }

        private void SetOutputSurfaceChromaVOffset(int[] arguments)
        {
            _outputSurfaceChromaVAddress = GetAddress(arguments);
        }

        private static ulong GetAddress(int[] arguments)
        {
            return (ulong)(uint)arguments[0] << 8;
        }
    }
}