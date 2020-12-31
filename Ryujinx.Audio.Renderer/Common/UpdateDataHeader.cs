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

using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Update data header used for input and output of <see cref="Server.AudioRenderSystem.Update(System.Memory{byte}, System.Memory{byte}, System.ReadOnlyMemory{byte})"/>.
    /// </summary>
    public struct UpdateDataHeader
    {
        public int Revision;
        public uint BehaviourSize;
        public uint MemoryPoolsSize;
        public uint VoicesSize;
        public uint VoiceResourcesSize;
        public uint EffectsSize;
        public uint MixesSize;
        public uint SinksSize;
        public uint PerformanceBufferSize;
        public uint Unknown24;
        public uint RenderInfoSize;

        private unsafe fixed int _reserved[4];

        public uint TotalSize;

        public void Initialize(int revision)
        {
            Revision = revision;

            TotalSize = (uint)Unsafe.SizeOf<UpdateDataHeader>();
        }
    }
}
