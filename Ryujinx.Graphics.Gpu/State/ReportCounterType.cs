namespace Ryujinx.Graphics.Gpu.State
{
    enum ReportCounterType
    {
        Zero                               = 0,
        InputVertices                      = 1,
        InputPrimitives                    = 3,
        VertexShaderInvocations            = 5,
        GeometryShaderInvocations          = 7,
        GeometryShaderPrimitives           = 9,
        TransformFeedbackPrimitivesWritten = 0xb,
        ClipperInputPrimitives             = 0xf,
        ClipperOutputPrimitives            = 0x11,
        PrimitivesGenerated                = 0x12,
        FragmentShaderInvocations          = 0x13,
        SamplesPassed                      = 0x15,
        TessControlShaderInvocations       = 0x1b,
        TessEvaluationShaderInvocations    = 0x1d,
        TessEvaluationShaderPrimitives     = 0x1f,
        ZcullStats0                        = 0x2a,
        ZcullStats1                        = 0x2c,
        ZcullStats2                        = 0x2e,
        ZcullStats3                        = 0x30
    }
}