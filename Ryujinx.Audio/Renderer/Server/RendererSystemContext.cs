//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

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
