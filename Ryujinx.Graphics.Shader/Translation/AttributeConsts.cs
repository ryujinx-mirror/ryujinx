namespace Ryujinx.Graphics.Shader.Translation
{
    static class AttributeConsts
    {
        public const int Layer         = 0x064;
        public const int PointSize     = 0x06c;
        public const int PositionX     = 0x070;
        public const int PositionY     = 0x074;
        public const int PositionZ     = 0x078;
        public const int PositionW     = 0x07c;
        public const int ClipDistance0 = 0x2c0;
        public const int ClipDistance1 = 0x2c4;
        public const int ClipDistance2 = 0x2c8;
        public const int ClipDistance3 = 0x2cc;
        public const int ClipDistance4 = 0x2d0;
        public const int ClipDistance5 = 0x2d4;
        public const int ClipDistance6 = 0x2d8;
        public const int ClipDistance7 = 0x2dc;
        public const int PointCoordX   = 0x2e0;
        public const int PointCoordY   = 0x2e4;
        public const int TessCoordX    = 0x2f0;
        public const int TessCoordY    = 0x2f4;
        public const int InstanceId    = 0x2f8;
        public const int VertexId      = 0x2fc;
        public const int FrontFacing   = 0x3fc;

        public const int UserAttributesCount = 32;
        public const int UserAttributeBase   = 0x80;
        public const int UserAttributeEnd    = UserAttributeBase + UserAttributesCount * 16;


        // Note: Those attributes are used internally by the translator
        // only, they don't exist on Maxwell.
        public const int SpecialMask             = 0xff << 24;
        public const int FragmentOutputDepth     = 0x1000000;
        public const int FragmentOutputColorBase = 0x1000010;
        public const int FragmentOutputColorEnd  = FragmentOutputColorBase + 8 * 16;

        public const int FragmentOutputIsBgraBase = 0x1000100;
        public const int FragmentOutputIsBgraEnd  = FragmentOutputIsBgraBase + 8 * 4;

        public const int ThreadIdX = 0x2000000;
        public const int ThreadIdY = 0x2000004;
        public const int ThreadIdZ = 0x2000008;

        public const int CtaIdX = 0x2000010;
        public const int CtaIdY = 0x2000014;
        public const int CtaIdZ = 0x2000018;

        public const int LaneId = 0x2000020;

        public const int EqMask = 0x2000024;
        public const int GeMask = 0x2000028;
        public const int GtMask = 0x200002c;
        public const int LeMask = 0x2000030;
        public const int LtMask = 0x2000034;
    }
}