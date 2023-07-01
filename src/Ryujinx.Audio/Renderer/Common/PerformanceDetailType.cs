namespace Ryujinx.Audio.Renderer.Common
{
    public enum PerformanceDetailType : byte
    {
        Unknown,
        PcmInt16,
        Adpcm,
        VolumeRamp,
        BiquadFilter,
        Mix,
        Delay,
        Aux,
        Reverb,
        Reverb3d,
        PcmFloat,
        Limiter,
        CaptureBuffer,
        Compressor,
    }
}
