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

using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Common.Memory;
using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Represent the update state of a voice.
    /// </summary>
    /// <remarks>This is shared between the server and audio processor.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = Align)]
    public struct VoiceUpdateState
    {
        public const int Align = 0x10;
        public const int BiquadStateOffset = 0x0;
        public const int BiquadStateSize = 0x10;

        /// <summary>
        /// The state of the biquad filters of this voice.
        /// </summary>
        public Array2<BiquadFilterState> BiquadFilterState;

        /// <summary>
        /// The total amount of samples that was played.
        /// </summary>
        /// <remarks>This is reset to 0 when a <see cref="WaveBuffer"/> finishes playing and <see cref="WaveBuffer.IsEndOfStream"/> is set.</remarks>
        /// <remarks>This is reset to 0 when looping while <see cref="Parameter.VoiceInParameter.DecodingBehaviour.PlayedSampleCountResetWhenLooping"/> is set.</remarks>
        public ulong PlayedSampleCount;

        /// <summary>
        /// The current sample offset in the <see cref="WaveBuffer"/> pointed by <see cref="WaveBufferIndex"/>.
        /// </summary>
        public int Offset;

        /// <summary>
        /// The current index of the <see cref="WaveBuffer"/> in use.
        /// </summary>
        public uint WaveBufferIndex;

        private WaveBufferValidArray _isWaveBufferValid;

        /// <summary>
        /// The total amount of <see cref="WaveBuffer"/> consumed.
        /// </summary>
        public uint WaveBufferConsumed;

        /// <summary>
        /// Pitch used for Sample Rate Conversion.
        /// </summary>
        public Array8<short> Pitch;

        public float Fraction;

        /// <summary>
        /// The ADPCM loop context when <see cref="SampleFormat.Adpcm"/> is in use.
        /// </summary>
        public AdpcmLoopContext LoopContext;

        /// <summary>
        /// The last samples after a mix ramp.
        /// </summary>
        /// <remarks>This is used for depop (to perform voice drop).</remarks>
        public Array24<float> LastSamples;

        /// <summary>
        /// The current count of loop performed.
        /// </summary>
        public int LoopCount;

        [StructLayout(LayoutKind.Sequential, Size = 1 * Constants.VoiceWaveBufferCount, Pack = 1)]
        private struct WaveBufferValidArray { }

        /// <summary>
        /// Contains information of <see cref="WaveBuffer"/> validity.
        /// </summary>
        public Span<bool> IsWaveBufferValid => SpanHelpers.AsSpan<WaveBufferValidArray, bool>(ref _isWaveBufferValid);

        /// <summary>
        /// Mark the current <see cref="WaveBuffer"/> as played and switch to the next one.
        /// </summary>
        /// <param name="waveBuffer">The current <see cref="WaveBuffer"/></param>
        /// <param name="waveBufferIndex">The wavebuffer index.</param>
        /// <param name="waveBufferConsumed">The amount of wavebuffers consumed.</param>
        /// <param name="playedSampleCount">The total count of sample played.</param>
        public void MarkEndOfBufferWaveBufferProcessing(ref WaveBuffer waveBuffer, ref int waveBufferIndex, ref uint waveBufferConsumed, ref ulong playedSampleCount)
        {
            IsWaveBufferValid[waveBufferIndex++] = false;
            LoopCount = 0;
            waveBufferConsumed++;

            if (waveBufferIndex >= Constants.VoiceWaveBufferCount)
            {
                waveBufferIndex = 0;
            }

            if (waveBuffer.IsEndOfStream)
            {
                playedSampleCount = 0;
            }
        }
    }
}
