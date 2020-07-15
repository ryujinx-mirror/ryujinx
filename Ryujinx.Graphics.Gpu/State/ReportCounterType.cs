namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Counter type for GPU counter reporting.
    /// </summary>
    enum ReportCounterType
    {
        Zero                               = 0,
        InputVertices                      = 1,
        InputPrimitives                    = 3,
        VertexShaderInvocations            = 5,
        GeometryShaderInvocations          = 7,
        GeometryShaderPrimitives           = 9,
        ZcullStats0                        = 0xa,
        TransformFeedbackPrimitivesWritten = 0xb,
        ZcullStats1                        = 0xc,
        ZcullStats2                        = 0xe,
        ClipperInputPrimitives             = 0xf,
        ZcullStats3                        = 0x10,
        ClipperOutputPrimitives            = 0x11,
        PrimitivesGenerated                = 0x12,
        FragmentShaderInvocations          = 0x13,
        SamplesPassed                      = 0x15,
        TransformFeedbackOffset            = 0x1a,
        TessControlShaderInvocations       = 0x1b,
        TessEvaluationShaderInvocations    = 0x1d,
        TessEvaluationShaderPrimitives     = 0x1f
    }
}