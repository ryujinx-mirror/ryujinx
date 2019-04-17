namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    static class AttributeConsts
    {
        public const int Layer       = 0x064;
        public const int PointSize   = 0x06c;
        public const int PositionX   = 0x070;
        public const int PositionY   = 0x074;
        public const int PositionZ   = 0x078;
        public const int PositionW   = 0x07c;
        public const int PointCoordX = 0x2e0;
        public const int PointCoordY = 0x2e4;
        public const int TessCoordX  = 0x2f0;
        public const int TessCoordY  = 0x2f4;
        public const int InstanceId  = 0x2f8;
        public const int VertexId    = 0x2fc;
        public const int FrontFacing = 0x3fc;

        public const int UserAttributesCount = 32;
        public const int UserAttributeBase   = 0x80;
        public const int UserAttributeEnd    = UserAttributeBase + UserAttributesCount * 16;


        //Note: Those attributes are used internally by the translator
        //only, they don't exist on Maxwell.
        public const int FragmentOutputDepth     = 0x1000000;
        public const int FragmentOutputColorBase = 0x1000010;
        public const int FragmentOutputColorEnd  = FragmentOutputColorBase + 8 * 16;
    }
}