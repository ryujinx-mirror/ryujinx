namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Sample format definition.
    /// </summary>
    public enum SampleFormat : byte
    {
        /// <summary>
        /// Invalid sample format.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// PCM8 sample format. (unsupported)
        /// </summary>
        PcmInt8 = 1,

        /// <summary>
        /// PCM16 sample format.
        /// </summary>
        PcmInt16 = 2,

        /// <summary>
        /// PCM24 sample format. (unsupported)
        /// </summary>
        PcmInt24 = 3,

        /// <summary>
        /// PCM32 sample format.
        /// </summary>
        PcmInt32 = 4,

        /// <summary>
        /// PCM Float sample format.
        /// </summary>
        PcmFloat = 5,

        /// <summary>
        /// ADPCM sample format. (Also known as GC-ADPCM)
        /// </summary>
        Adpcm = 6,
    }
}
