using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Vic.Types
{
    struct ConfigStruct
    {
        public PipeConfig PipeConfig;
        public OutputConfig OutputConfig;
        public OutputSurfaceConfig OutputSurfaceConfig;
        public MatrixStruct OutColorMatrix;
        public Array4<ClearRectStruct> ClearRectStruct;
        public Array8<SlotStruct> SlotStruct;
    }
}
