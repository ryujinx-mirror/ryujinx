namespace Ryujinx.Graphics.Shader.Translation
{
    class HostCapabilities
    {
        public readonly bool ReducedPrecision;
        public readonly bool SupportsFragmentShaderInterlock;
        public readonly bool SupportsFragmentShaderOrderingIntel;
        public readonly bool SupportsGeometryShaderPassthrough;
        public readonly bool SupportsShaderBallot;
        public readonly bool SupportsShaderBarrierDivergence;
        public readonly bool SupportsShaderFloat64;
        public readonly bool SupportsTextureShadowLod;
        public readonly bool SupportsViewportMask;

        public HostCapabilities(
            bool reducedPrecision,
            bool supportsFragmentShaderInterlock,
            bool supportsFragmentShaderOrderingIntel,
            bool supportsGeometryShaderPassthrough,
            bool supportsShaderBallot,
            bool supportsShaderBarrierDivergence,
            bool supportsShaderFloat64,
            bool supportsTextureShadowLod,
            bool supportsViewportMask)
        {
            ReducedPrecision = reducedPrecision;
            SupportsFragmentShaderInterlock = supportsFragmentShaderInterlock;
            SupportsFragmentShaderOrderingIntel = supportsFragmentShaderOrderingIntel;
            SupportsGeometryShaderPassthrough = supportsGeometryShaderPassthrough;
            SupportsShaderBallot = supportsShaderBallot;
            SupportsShaderBarrierDivergence = supportsShaderBarrierDivergence;
            SupportsShaderFloat64 = supportsShaderFloat64;
            SupportsTextureShadowLod = supportsTextureShadowLod;
            SupportsViewportMask = supportsViewportMask;
        }
    }
}
