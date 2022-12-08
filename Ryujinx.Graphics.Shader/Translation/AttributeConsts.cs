namespace Ryujinx.Graphics.Shader.Translation
{
    static class AttributeConsts
    {
        public const int TessLevelOuter0     = 0x000;
        public const int TessLevelOuter1     = 0x004;
        public const int TessLevelOuter2     = 0x008;
        public const int TessLevelOuter3     = 0x00c;
        public const int TessLevelInner0     = 0x010;
        public const int TessLevelInner1     = 0x014;
        public const int PrimitiveId         = 0x060;
        public const int Layer               = 0x064;
        public const int ViewportIndex       = 0x068;
        public const int PointSize           = 0x06c;
        public const int PositionX           = 0x070;
        public const int PositionY           = 0x074;
        public const int PositionZ           = 0x078;
        public const int PositionW           = 0x07c;
        public const int FrontColorDiffuseR  = 0x280;
        public const int FrontColorDiffuseG  = 0x284;
        public const int FrontColorDiffuseB  = 0x288;
        public const int FrontColorDiffuseA  = 0x28c;
        public const int FrontColorSpecularR = 0x290;
        public const int FrontColorSpecularG = 0x294;
        public const int FrontColorSpecularB = 0x298;
        public const int FrontColorSpecularA = 0x29c;
        public const int BackColorDiffuseR   = 0x2a0;
        public const int BackColorDiffuseG   = 0x2a4;
        public const int BackColorDiffuseB   = 0x2a8;
        public const int BackColorDiffuseA   = 0x2ac;
        public const int BackColorSpecularR  = 0x2b0;
        public const int BackColorSpecularG  = 0x2b4;
        public const int BackColorSpecularB  = 0x2b8;
        public const int BackColorSpecularA  = 0x2bc;
        public const int ClipDistance0       = 0x2c0;
        public const int ClipDistance1       = 0x2c4;
        public const int ClipDistance2       = 0x2c8;
        public const int ClipDistance3       = 0x2cc;
        public const int ClipDistance4       = 0x2d0;
        public const int ClipDistance5       = 0x2d4;
        public const int ClipDistance6       = 0x2d8;
        public const int ClipDistance7       = 0x2dc;
        public const int PointCoordX         = 0x2e0;
        public const int PointCoordY         = 0x2e4;
        public const int TessCoordX          = 0x2f0;
        public const int TessCoordY          = 0x2f4;
        public const int InstanceId          = 0x2f8;
        public const int VertexId            = 0x2fc;
        public const int TexCoordCount       = 10;
        public const int TexCoordBase        = 0x300;
        public const int TexCoordEnd         = TexCoordBase + TexCoordCount * 16;
        public const int FrontFacing         = 0x3fc;

        public const int UserAttributesCount = 32;
        public const int UserAttributeBase   = 0x80;
        public const int UserAttributeEnd    = UserAttributeBase + UserAttributesCount * 16;

        public const int UserAttributePerPatchBase = 0x18;
        public const int UserAttributePerPatchEnd  = 0x200;

        public const int LoadOutputMask = 1 << 30;
        public const int Mask = 0x3fffffff;


        // Note: Those attributes are used internally by the translator
        // only, they don't exist on Maxwell.
        public const int SpecialMask             = 0xf << 24;
        public const int FragmentOutputDepth     = 0x1000000;
        public const int FragmentOutputColorBase = 0x1000010;
        public const int FragmentOutputColorEnd  = FragmentOutputColorBase + 8 * 16;

        public const int FragmentOutputIsBgraBase = 0x1000100;
        public const int FragmentOutputIsBgraEnd  = FragmentOutputIsBgraBase + 8 * 4;

        public const int SupportBlockViewInverseX = 0x1000200;
        public const int SupportBlockViewInverseY = 0x1000204;

        public const int ThreadIdX = 0x2000000;
        public const int ThreadIdY = 0x2000004;
        public const int ThreadIdZ = 0x2000008;

        public const int CtaIdX = 0x2000010;
        public const int CtaIdY = 0x2000014;
        public const int CtaIdZ = 0x2000018;

        public const int LaneId = 0x2000020;

        public const int InvocationId = 0x2000024;
        public const int PatchVerticesIn = 0x2000028;

        public const int EqMask = 0x2000030;
        public const int GeMask = 0x2000034;
        public const int GtMask = 0x2000038;
        public const int LeMask = 0x200003c;
        public const int LtMask = 0x2000040;

        public const int ThreadKill = 0x2000044;

        public const int BaseInstance = 0x2000050;
        public const int BaseVertex = 0x2000054;
        public const int InstanceIndex = 0x2000058;
        public const int VertexIndex = 0x200005c;
        public const int DrawIndex = 0x2000060;
    }
}