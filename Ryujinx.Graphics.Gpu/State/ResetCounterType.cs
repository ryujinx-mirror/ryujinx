namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Counter type for GPU counter reset.
    /// </summary>
    enum ResetCounterType
    {
        SamplesPassed                      = 1,
        ZcullStats                         = 2,
        TransformFeedbackPrimitivesWritten = 0x10,
        InputVertices                      = 0x12,
        InputPrimitives                    = 0x13,
        VertexShaderInvocations            = 0x15,
        TessControlShaderInvocations       = 0x16,
        TessEvaluationShaderInvocations    = 0x17,
        TessEvaluationShaderPrimitives     = 0x18,
        GeometryShaderInvocations          = 0x1a,
        GeometryShaderPrimitives           = 0x1b,
        ClipperInputPrimitives             = 0x1c,
        ClipperOutputPrimitives            = 0x1d,
        FragmentShaderInvocations          = 0x1e,
        PrimitivesGenerated                = 0x1f
    }
}