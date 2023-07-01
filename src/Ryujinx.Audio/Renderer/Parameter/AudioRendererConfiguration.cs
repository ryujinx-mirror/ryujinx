using Ryujinx.Audio.Renderer.Server.Types;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Audio Renderer user configuration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioRendererConfiguration
    {
        /// <summary>
        /// The target sample rate of the user.
        /// </summary>
        /// <remarks>Only 32000Hz and 48000Hz are considered valid, other sample rates will cause undefined behaviour.</remarks>
        public uint SampleRate;

        /// <summary>
        /// The target sample count per <see cref="Dsp.AudioProcessor"/> updates.
        /// </summary>
        public uint SampleCount;

        /// <summary>
        /// The maximum mix buffer count.
        /// </summary>
        public uint MixBufferCount;

        /// <summary>
        /// The maximum amount of sub mixes that could be used by the user.
        /// </summary>
        public uint SubMixBufferCount;

        /// <summary>
        /// The maximum amount of voices that could be used by the user.
        /// </summary>
        public uint VoiceCount;

        /// <summary>
        /// The maximum amount of sinks that could be used by the user.
        /// </summary>
        public uint SinkCount;

        /// <summary>
        /// The maximum amount of effects that could be used by the user.
        /// </summary>
        public uint EffectCount;

        /// <summary>
        /// The maximum amount of performance metric frames that could be used by the user.
        /// </summary>
        public uint PerformanceMetricFramesCount;

        /// <summary>
        /// Set to true if the user allows the <see cref="Server.AudioRenderSystem"/> to drop voices.
        /// </summary>
        /// <seealso cref="Server.AudioRenderSystem.ComputeVoiceDrop(Server.CommandBuffer, long, long)"/>
        [MarshalAs(UnmanagedType.I1)]
        public bool VoiceDropEnabled;

        /// <summary>
        /// Reserved/unused
        /// </summary>
        private readonly byte _reserved;

        /// <summary>
        /// The target rendering device.
        /// </summary>
        /// <remarks>Must be <see cref="AudioRendererRenderingDevice.Dsp"/></remarks>
        public AudioRendererRenderingDevice RenderingDevice;

        /// <summary>
        /// The target execution mode.
        /// </summary>
        /// <remarks>Must be <see cref="AudioRendererExecutionMode.Auto"/></remarks>
        public AudioRendererExecutionMode ExecutionMode;

        /// <summary>
        /// The maximum amount of splitters that could be used by the user.
        /// </summary>
        public uint SplitterCount;

        /// <summary>
        /// The maximum amount of splitters destinations that could be used by the user.
        /// </summary>
        public uint SplitterDestinationCount;

        /// <summary>
        /// The size of the external context.
        /// </summary>
        /// <remarks>This is a leftover of the old "codec" interface system that was present between 1.0.0 and 3.0.0. This was entirely removed from the server side with REV8.</remarks>
        public uint ExternalContextSize;

        /// <summary>
        /// The user audio revision
        /// </summary>
        /// <seealso cref="Server.BehaviourContext"/>
        public int Revision;
    }
}
