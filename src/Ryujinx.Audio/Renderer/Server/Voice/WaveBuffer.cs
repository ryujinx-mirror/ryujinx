using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Voice
{
    /// <summary>
    /// A wavebuffer used for server update.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x58, Pack = 1)]
    public struct WaveBuffer
    {
        /// <summary>
        /// The <see cref="AddressInfo"/> of the sample data of the wavebuffer.
        /// </summary>
        public AddressInfo BufferAddressInfo;

        /// <summary>
        /// The <see cref="AddressInfo"/> of the context of the wavebuffer.
        /// </summary>
        /// <remarks>Only used by <see cref="Common.SampleFormat.Adpcm"/>.</remarks>
        public AddressInfo ContextAddressInfo;


        /// <summary>
        /// First sample to play of the wavebuffer.
        /// </summary>
        public uint StartSampleOffset;

        /// <summary>
        /// Last sample to play of the wavebuffer.
        /// </summary>
        public uint EndSampleOffset;

        /// <summary>
        /// Set to true if the wavebuffer is looping.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool ShouldLoop;

        /// <summary>
        /// Set to true if the wavebuffer is the end of stream.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsEndOfStream;

        /// <summary>
        /// Set to true if the wavebuffer wasn't sent to the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsSendToAudioProcessor;

        /// <summary>
        /// First sample to play when looping the wavebuffer.
        /// </summary>
        public uint LoopStartSampleOffset;

        /// <summary>
        /// Last sample to play when looping the wavebuffer.
        /// </summary>
        public uint LoopEndSampleOffset;

        /// <summary>
        /// The max loop count.
        /// </summary>
        public int LoopCount;

        /// <summary>
        /// Create a new <see cref="Common.WaveBuffer"/> for use by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        /// <param name="version">The target version of the wavebuffer.</param>
        /// <returns>A new <see cref="Common.WaveBuffer"/> for use by the <see cref="Dsp.AudioProcessor"/>.</returns>
        public Common.WaveBuffer ToCommon(int version)
        {
            Common.WaveBuffer waveBuffer = new()
            {
                Buffer = BufferAddressInfo.GetReference(true),
                BufferSize = (uint)BufferAddressInfo.Size,
            };

            if (ContextAddressInfo.CpuAddress != 0)
            {
                waveBuffer.Context = ContextAddressInfo.GetReference(true);
                waveBuffer.ContextSize = (uint)ContextAddressInfo.Size;
            }

            waveBuffer.StartSampleOffset = StartSampleOffset;
            waveBuffer.EndSampleOffset = EndSampleOffset;
            waveBuffer.Looping = ShouldLoop;
            waveBuffer.IsEndOfStream = IsEndOfStream;

            if (version == 2)
            {
                waveBuffer.LoopCount = LoopCount;
                waveBuffer.LoopStartSampleOffset = LoopStartSampleOffset;
                waveBuffer.LoopEndSampleOffset = LoopEndSampleOffset;
            }
            else
            {
                waveBuffer.LoopCount = -1;
            }

            return waveBuffer;
        }
    }
}
