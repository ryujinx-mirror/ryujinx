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

namespace Ryujinx.Audio.Renderer.Server.Types
{
    /// <summary>
    /// The internal play state of a <see cref="Voice.VoiceState"/>
    /// </summary>
    public enum PlayState
    {
        /// <summary>
        /// The voice has been started and is playing.
        /// </summary>
        Started,

        /// <summary>
        /// The voice has been stopped.
        /// </summary>
        /// <remarks>
        /// This cannot be directly set by user.
        /// See <see cref="Stopping"/> for correct usage.
        /// </remarks>
        Stopped,

        /// <summary>
        /// The user asked the voice to be stopped.
        /// </summary>
        /// <remarks>
        /// This is changed to the <see cref="Stopped"/> state after command generation.
        /// <seealso cref="Voice.VoiceState.UpdateForCommandGeneration(Voice.VoiceContext)"/>
        /// </remarks>
        Stopping,

        /// <summary>
        /// The voice has been paused by user request.
        /// </summary>
        /// <remarks>
        /// The user can resume to the <see cref="Started"/> state.
        /// </remarks>
        Paused
    }
}
