namespace Ryujinx.Graphics.Shader.Translation
{
    static class AttributeConsts
    {
        public const int PrimitiveId = 0x060;
        public const int Layer = 0x064;
        public const int ViewportIndex = 0x068;
        public const int PositionX = 0x070;
        public const int PositionY = 0x074;
        public const int FrontColorDiffuseR = 0x280;
        public const int BackColorDiffuseR = 0x2a0;
        public const int ClipDistance0 = 0x2c0;
        public const int ClipDistance1 = 0x2c4;
        public const int ClipDistance2 = 0x2c8;
        public const int ClipDistance3 = 0x2cc;
        public const int ClipDistance4 = 0x2d0;
        public const int ClipDistance5 = 0x2d4;
        public const int ClipDistance6 = 0x2d8;
        public const int ClipDistance7 = 0x2dc;
        public const int FogCoord = 0x2e8;
        public const int TessCoordX = 0x2f0;
        public const int TessCoordY = 0x2f4;
        public const int InstanceId = 0x2f8;
        public const int VertexId = 0x2fc;
        public const int TexCoordCount = 10;
        public const int TexCoordBase = 0x300;
        public const int TexCoordEnd = TexCoordBase + TexCoordCount * 16;
        public const int ViewportMask = 0x3a0;
        public const int FrontFacing = 0x3fc;

        public const int UserAttributesCount = 32;
        public const int UserAttributeBase = 0x80;
        public const int UserAttributeEnd = UserAttributeBase + UserAttributesCount * 16;

        public const int UserAttributePerPatchBase = 0x18;
        public const int UserAttributePerPatchEnd = 0x200;
    }
}
