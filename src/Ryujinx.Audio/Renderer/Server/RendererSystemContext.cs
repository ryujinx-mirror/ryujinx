using Ryujinx.Audio.Renderer.Server.Upsampler;
using System;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// Represents a lite version of <see cref="AudioRenderSystem"/> used by the <see cref="Dsp.AudioProcessor"/>
    /// </summary>
    /// <remarks>
    /// This also allows to reduce dependencies on the <see cref="AudioRenderSystem"/> for unit testing.
    /// </remarks>
    public sealed class RendererSystemContext
    {
        /// <summary>
        /// The session id of the current renderer.
        /// </summary>
        public int SessionId;

        /// <summary>
        /// The target channel count for sink.
        /// </summary>
        /// <remarks>See <see cref="CommandGenerator.GenerateDevice(Sink.DeviceSink, ref Mix.MixState)"/> for usage.</remarks>
        public uint ChannelCount;

        /// <summary>
        /// The total count of mix buffer.
        /// </summary>
        public uint MixBufferCount;

        /// <summary>
        /// Instance of the <see cref="BehaviourContext"/> used to derive bug fixes and features of the current audio renderer revision.
        /// </summary>
        public BehaviourContext BehaviourContext;

        /// <summary>
        /// Instance of the <see cref="UpsamplerManager"/> used for upsampling (see <see cref="UpsamplerState"/>)
        /// </summary>
        public UpsamplerManager UpsamplerManager;

        /// <summary>
        /// The memory to use for depop processing.
        /// </summary>
        /// <remarks>
        /// See <see cref="Dsp.Command.DepopForMixBuffersCommand"/> and <see cref="Dsp.Command.DepopPrepareCommand"/>
        /// </remarks>
        public Memory<float> DepopBuffer;
    }
}
