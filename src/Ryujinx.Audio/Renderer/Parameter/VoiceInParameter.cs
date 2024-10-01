using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp;
using Ryujinx.Common.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a voice.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x170, Pack = 1)]
    public struct VoiceInParameter
    {
        /// <summary>
        /// Id of the voice.
        /// </summary>
        public int Id;

        /// <summary>
        /// Node id of the voice.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// Set to true if the voice is new.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsNew;

        /// <summary>
        /// Set to true if the voice is used.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool InUse;

        /// <summary>
        /// The voice <see cref="PlayState"/> wanted by the user.
        /// </summary>
        public PlayState PlayState;

        /// <summary>
        /// The <see cref="SampleFormat"/> of the voice.
        /// </summary>
        public SampleFormat SampleFormat;

        /// <summary>
        /// The sample rate of the voice.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// The priority of the voice.
        /// </summary>
        public uint Priority;

        /// <summary>
        /// Target sorting position of the voice. (Used to sort voices with the same <see cref="Priority"/>)
        /// </summary>
        public uint SortingOrder;

        /// <summary>
        /// The total channel count used.
        /// </summary>
        public uint ChannelCount;

        /// <summary>
        /// The pitch used on the voice.
        /// </summary>
        public float Pitch;

        /// <summary>
        /// The output volume of the voice.
        /// </summary>
        public float Volume;

        /// <summary>
        /// Biquad filters to apply to the output of the voice.
        /// </summary>
        public Array2<BiquadFilterParameter> BiquadFilters;

        /// <summary>
        /// Total count of <see cref="WaveBufferInternal"/> of the voice.
        /// </summary>
        public uint WaveBuffersCount;

        /// <summary>
        /// Current playing <see cref="WaveBufferInternal"/> of the voice.
        /// </summary>
        public uint WaveBuffersIndex;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private readonly uint _reserved1;

        /// <summary>
        /// User state address required by the data source.
        /// </summary>
        /// <remarks>Only used for <see cref="SampleFormat.Adpcm"/> as the address of the GC-ADPCM coefficients.</remarks>
        public ulong DataSourceStateAddress;

        /// <summary>
        /// User state size required by the data source.
        /// </summary>
        /// <remarks>Only used for <see cref="SampleFormat.Adpcm"/> as the size of the GC-ADPCM coefficients.</remarks>
        public ulong DataSourceStateSize;

        /// <summary>
        /// The target mix id of the voice.
        /// </summary>
        public int MixId;

        /// <summary>
        /// The target splitter id of the voice.
        /// </summary>
        public uint SplitterId;

        /// <summary>
        /// The wavebuffer parameters of this voice.
        /// </summary>
        public Array4<WaveBufferInternal> WaveBuffers;

        /// <summary>
        /// The channel resource ids associated to the voice.
        /// </summary>
        public Array6<int> ChannelResourceIds;

        /// <summary>
        /// Reset the voice drop flag during voice server update.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool ResetVoiceDropFlag;

        /// <summary>
        /// Flush the amount of wavebuffer specified. This will result in the wavebuffer being skipped and marked played.
        /// </summary>
        /// <remarks>This was added on REV5.</remarks>
        public byte FlushWaveBufferCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private readonly ushort _reserved2;

        /// <summary>
        /// Change the behaviour of the voice.
        /// </summary>
        /// <remarks>This was added on REV5.</remarks>
        public DecodingBehaviour DecodingBehaviourFlags;

        /// <summary>
        /// Change the Sample Rate Conversion (SRC) quality of the voice.
        /// </summary>
        /// <remarks>This was added on REV8.</remarks>
        public SampleRateConversionQuality SrcQuality;

        /// <summary>
        /// This was previously used for opus codec support on the Audio Renderer and was removed on REV3.
        /// </summary>
        public uint ExternalContext;

        /// <summary>
        /// This was previously used for opus codec support on the Audio Renderer and was removed on REV3.
        /// </summary>
        public uint ExternalContextSize;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved3[2];

        /// <summary>
        /// Input information for a voice wavebuffer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 0x38, Pack = 1)]
        public struct WaveBufferInternal
        {
            /// <summary>
            /// Address of the wavebuffer data.
            /// </summary>
            public ulong Address;

            /// <summary>
            /// Size of the wavebuffer data.
            /// </summary>
            public ulong Size;

            /// <summary>
            /// Offset of the first sample to play.
            /// </summary>
            public uint StartSampleOffset;

            /// <summary>
            /// Offset of the last sample to play.
            /// </summary>
            public uint EndSampleOffset;

            /// <summary>
            /// If set to true, the wavebuffer will loop when reaching <see cref="EndSampleOffset"/>.
            /// </summary>
            /// <remarks>
            /// Starting with REV8, you can specify how many times to loop the wavebuffer (<see cref="LoopCount"/>) and where it should start and end when looping (<see cref="LoopFirstSampleOffset"/> and <see cref="LoopLastSampleOffset"/>)
            /// </remarks>
            [MarshalAs(UnmanagedType.I1)]
            public bool ShouldLoop;

            /// <summary>
            /// Indicates that this is the last wavebuffer to play of the voice.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool IsEndOfStream;

            /// <summary>
            /// Indicates if the server should update its internal state.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool SentToServer;

            /// <summary>
            /// Reserved/unused.
            /// </summary>
            private readonly byte _reserved;

            /// <summary>
            /// If set to anything other than 0, specifies how many times to loop the wavebuffer.
            /// </summary>
            /// <remarks>This was added in REV8.</remarks>
            public int LoopCount;

            /// <summary>
            /// Address of the context used by the sample decoder.
            /// </summary>
            /// <remarks>This is only currently used by <see cref="SampleFormat.Adpcm"/>.</remarks>
            public ulong ContextAddress;

            /// <summary>
            /// Size of the context used by the sample decoder.
            /// </summary>
            /// <remarks>This is only currently used by <see cref="SampleFormat.Adpcm"/>.</remarks>
            public ulong ContextSize;

            /// <summary>
            /// If set to anything other than 0, specifies the offset of the first sample to play when looping.
            /// </summary>
            /// <remarks>This was added in REV8.</remarks>
            public uint LoopFirstSampleOffset;

            /// <summary>
            /// If set to anything other than 0, specifies the offset of the last sample to play when looping.
            /// </summary>
            /// <remarks>This was added in REV8.</remarks>
            public uint LoopLastSampleOffset;

            /// <summary>
            /// Check if the sample offsets are in a valid range for generic PCM.
            /// </summary>
            /// <typeparam name="T">The PCM sample type</typeparam>
            /// <returns>Returns true if the sample offset are in range of the size.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private readonly bool IsSampleOffsetInRangeForPcm<T>() where T : unmanaged
            {
                uint dataTypeSize = (uint)Unsafe.SizeOf<T>();

                return (ulong)StartSampleOffset * dataTypeSize <= Size &&
                       (ulong)EndSampleOffset * dataTypeSize <= Size;
            }

            /// <summary>
            /// Check if the sample offsets are in a valid range for the given <see cref="SampleFormat"/>.
            /// </summary>
            /// <param name="format">The target <see cref="SampleFormat"/></param>
            /// <returns>Returns true if the sample offset are in range of the size.</returns>
            public readonly bool IsSampleOffsetValid(SampleFormat format)
            {
                return format switch
                {
                    SampleFormat.PcmInt16 => IsSampleOffsetInRangeForPcm<ushort>(),
                    SampleFormat.PcmFloat => IsSampleOffsetInRangeForPcm<float>(),
                    SampleFormat.Adpcm => AdpcmHelper.GetAdpcmDataSize((int)StartSampleOffset) <= Size && AdpcmHelper.GetAdpcmDataSize((int)EndSampleOffset) <= Size,
                    _ => throw new NotImplementedException($"{format} not implemented!"),
                };
            }
        }

        /// <summary>
        /// Flag altering the behaviour of wavebuffer decoding.
        /// </summary>
        [Flags]
        public enum DecodingBehaviour : ushort
        {
            /// <summary>
            /// Default decoding behaviour.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Reset the played samples accumulator when looping.
            /// </summary>
            PlayedSampleCountResetWhenLooping = 1,

            /// <summary>
            /// Skip pitch and Sample Rate Conversion (SRC).
            /// </summary>
            SkipPitchAndSampleRateConversion = 2,
        }

        /// <summary>
        /// Specify the quality to use during Sample Rate Conversion (SRC) and pitch handling.
        /// </summary>
        /// <remarks>This was added in REV8.</remarks>
        public enum SampleRateConversionQuality : byte
        {
            /// <summary>
            /// Resample interpolating 4 samples per output sample.
            /// </summary>
            Default,

            /// <summary>
            /// Resample interpolating 8 samples per output sample.
            /// </summary>
            High,

            /// <summary>
            /// Resample interpolating 1 samples per output sample.
            /// </summary>
            Low,
        }
    }
}
