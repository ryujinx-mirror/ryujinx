using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Vic
{
    class VideoImageComposer
    {
        private NvGpu Gpu;

        private long ConfigStructAddress;
        private long OutputSurfaceLumaAddress;
        private long OutputSurfaceChromaUAddress;
        private long OutputSurfaceChromaVAddress;

        public VideoImageComposer(NvGpu Gpu)
        {
            this.Gpu = Gpu;
        }

        public void Process(NvGpuVmm Vmm, int MethodOffset, int[] Arguments)
        {
            VideoImageComposerMeth Method = (VideoImageComposerMeth)MethodOffset;

            switch (Method)
            {
                case VideoImageComposerMeth.Execute:
                    Execute(Vmm, Arguments);
                    break;

                case VideoImageComposerMeth.SetConfigStructOffset:
                    SetConfigStructOffset(Vmm, Arguments);
                    break;

                case VideoImageComposerMeth.SetOutputSurfaceLumaOffset:
                    SetOutputSurfaceLumaOffset(Vmm, Arguments);
                    break;

                case VideoImageComposerMeth.SetOutputSurfaceChromaUOffset:
                    SetOutputSurfaceChromaUOffset(Vmm, Arguments);
                    break;

                case VideoImageComposerMeth.SetOutputSurfaceChromaVOffset:
                    SetOutputSurfaceChromaVOffset(Vmm, Arguments);
                    break;
            }
        }

        private void Execute(NvGpuVmm Vmm, int[] Arguments)
        {
            StructUnpacker Unpacker = new StructUnpacker(Vmm, ConfigStructAddress + 0x20);

            SurfacePixelFormat PixelFormat = (SurfacePixelFormat)Unpacker.Read(7);

            int ChromaLocHoriz = Unpacker.Read(2);
            int ChromaLocVert  = Unpacker.Read(2);

            int BlockLinearKind       = Unpacker.Read(4);
            int BlockLinearHeightLog2 = Unpacker.Read(4);

            int Reserved0 = Unpacker.Read(3);
            int Reserved1 = Unpacker.Read(10);

            int SurfaceWidthMinus1  = Unpacker.Read(14);
            int SurfaceHeightMinus1 = Unpacker.Read(14);

            int GobBlockHeight = 1 << BlockLinearHeightLog2;

            int SurfaceWidth  = SurfaceWidthMinus1  + 1;
            int SurfaceHeight = SurfaceHeightMinus1 + 1;

            SurfaceOutputConfig OutputConfig = new SurfaceOutputConfig(
                PixelFormat,
                SurfaceWidth,
                SurfaceHeight,
                GobBlockHeight,
                OutputSurfaceLumaAddress,
                OutputSurfaceChromaUAddress,
                OutputSurfaceChromaVAddress);

            Gpu.VideoDecoder.CopyPlanes(Vmm, OutputConfig);
        }

        private void SetConfigStructOffset(NvGpuVmm Vmm, int[] Arguments)
        {
            ConfigStructAddress = GetAddress(Arguments);
        }

        private void SetOutputSurfaceLumaOffset(NvGpuVmm Vmm, int[] Arguments)
        {
            OutputSurfaceLumaAddress = GetAddress(Arguments);
        }

        private void SetOutputSurfaceChromaUOffset(NvGpuVmm Vmm, int[] Arguments)
        {
            OutputSurfaceChromaUAddress = GetAddress(Arguments);
        }

        private void SetOutputSurfaceChromaVOffset(NvGpuVmm Vmm, int[] Arguments)
        {
            OutputSurfaceChromaVAddress = GetAddress(Arguments);
        }

        private static long GetAddress(int[] Arguments)
        {
            return (long)(uint)Arguments[0] << 8;
        }
    }
}