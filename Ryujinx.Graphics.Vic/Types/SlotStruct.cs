namespace Ryujinx.Graphics.Vic.Types
{
    struct SlotStruct
    {
        public SlotConfig SlotConfig;
        public SlotSurfaceConfig SlotSurfaceConfig;
        public LumaKeyStruct LumaKeyStruct;
        public MatrixStruct ColorMatrixStruct;
        public MatrixStruct GamutMatrixStruct;
        public BlendingSlotStruct BlendingSlotStruct;
    }
}
