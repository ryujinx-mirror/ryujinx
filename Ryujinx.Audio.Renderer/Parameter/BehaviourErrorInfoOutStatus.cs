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

using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information for behaviour.
    /// </summary>
    /// <remarks>This is used to report errors to the user during <see cref="Server.AudioRenderSystem.Update(Memory{byte}, Memory{byte}, ReadOnlyMemory{byte})"/> processing.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BehaviourErrorInfoOutStatus
    {
        /// <summary>
        /// The reported errors.
        /// </summary>
        public Array10<ErrorInfo> ErrorInfos;

        /// <summary>
        /// The amount of error that got reported.
        /// </summary>
        public uint ErrorInfosCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}
