using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information about a voice.
    /// </summary>
    /// <remarks>See <seealso cref="Server.StateUpdater.UpdateVoices(Server.Voice.VoiceContext, System.Memory{Server.MemoryPool.MemoryPoolState})"/></remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VoiceOutStatus
    {
        /// <summary>
        /// The total amount of samples that was played.
        /// </summary>
        /// <remarks>This is reset to 0 when a <see cref="Common.WaveBuffer"/> finishes playing and <see cref="Common.WaveBuffer.IsEndOfStream"/> is set.</remarks>
        /// <remarks>This is reset to 0 when looping while <see cref="Parameter.VoiceInParameter.DecodingBehaviour.PlayedSampleCountResetWhenLooping"/> is set.</remarks>
        public ulong PlayedSampleCount;

        /// <summary>
        /// The total amount of <see cref="WaveBuffer"/> consumed.
        /// </summary>
        public uint PlayedWaveBuffersCount;

        /// <summary>
        /// If set to true, the voice was dropped.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool VoiceDropFlag;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed byte _reserved[3];
    }
}
