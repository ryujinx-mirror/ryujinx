namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    enum IoVariable
    {
        Invalid,

        BackColorDiffuse,
        BackColorSpecular,
        BaseInstance,
        BaseVertex,
        ClipDistance,
        CtaId,
        DrawIndex,
        FogCoord,
        FragmentCoord,
        FragmentOutputColor,
        FragmentOutputDepth,
        FragmentOutputIsBgra, // TODO: Remove and use constant buffer access.
        FrontColorDiffuse,
        FrontColorSpecular,
        FrontFacing,
        InstanceId,
        InstanceIndex,
        InvocationId,
        Layer,
        PatchVertices,
        PointCoord,
        PointSize,
        Position,
        PrimitiveId,
        SubgroupEqMask,
        SubgroupGeMask,
        SubgroupGtMask,
        SubgroupLaneId,
        SubgroupLeMask,
        SubgroupLtMask,
        SupportBlockViewInverse, // TODO: Remove and use constant buffer access.
        SupportBlockRenderScale, // TODO: Remove and use constant buffer access.
        TessellationCoord,
        TessellationLevelInner,
        TessellationLevelOuter,
        TextureCoord,
        ThreadId,
        ThreadKill,
        UserDefined,
        VertexId,
        VertexIndex,
        ViewportIndex,
        ViewportMask
    }
}