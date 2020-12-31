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
    /// The execution mode of an <see cref="AudioRenderSystem"/>.
    /// </summary>
    public enum AudioRendererExecutionMode : byte
    {
        /// <summary>
        /// Automatically send commands to the DSP at a fixed rate (see <see cref="AudioRenderSystem.SendCommands"/>
        /// </summary>
        Auto,

        /// <summary>
        /// Audio renderer operation needs to be done manually via ExecuteAudioRenderer.
        /// </summary>
        /// <remarks>This is not supported on the DSP and is as such stubbed.</remarks>
        Manual
    }
}