namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// The type of an effect.
    /// </summary>
    public enum EffectType : byte
    {
        /// <summary>
        /// Invalid effect.
        /// </summary>
        Invalid,

        /// <summary>
        /// Effect applying additional mixing capability.
        /// </summary>
        BufferMix,

        /// <summary>
        /// Effect applying custom user effect (via auxiliary buffers).
        /// </summary>
        AuxiliaryBuffer,

        /// <summary>
        /// Effect applying a delay.
        /// </summary>
        Delay,

        /// <summary>
        /// Effect applying a reverberation effect via a given preset.
        /// </summary>
        Reverb,

        /// <summary>
        /// Effect applying a 3D reverberation effect via a given preset.
        /// </summary>
        Reverb3d,

        /// <summary>
        /// Effect applying a biquad filter.
        /// </summary>
        BiquadFilter,

        /// <summary>
        /// Effect applying a limiter (DRC).
        /// </summary>
        Limiter,

        /// <summary>
        /// Effect to capture mixes (via auxiliary buffers).
        /// </summary>
        CaptureBuffer,

        /// <summary>
        /// Effect applying a compressor filter (DRC).
        /// </summary>
        Compressor,
    }
}
