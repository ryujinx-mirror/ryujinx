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
    /// The rendering device of an <see cref="AudioRenderSystem"/>.
    /// </summary>
    public enum AudioRendererRenderingDevice : byte
    {
        /// <summary>
        /// Rendering is performed on the DSP.
        /// </summary>
        /// <remarks>
        /// Only supports <see cref="AudioRendererExecutionMode.Auto"/>.
        /// </remarks>
        Dsp,

        /// <summary>
        /// Rendering is performed on the CPU.
        /// </summary>
        /// <remarks>
        /// Only supports <see cref="AudioRendererExecutionMode.Manual"/>.
        /// </remarks>
        Cpu
    }
}
