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
