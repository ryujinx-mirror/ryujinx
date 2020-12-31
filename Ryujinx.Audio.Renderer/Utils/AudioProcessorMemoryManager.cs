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

using DspAddress = System.UInt64;
using CpuAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Utils
{
    /// <summary>
    /// The <see cref="Dsp.AudioProcessor"/> memory management
    /// </summary>
    /// <remarks>This is stub for the most part but is kept to permit LLE if wanted.</remarks>
    static class AudioProcessorMemoryManager
    {
        /// <summary>
        /// Map the given <see cref="CpuAddress"/> to the <see cref="Dsp.AudioProcessor"/> address space.
        /// </summary>
        /// <param name="processHandle">The process owning the CPU memory.</param>
        /// <param name="cpuAddress">The <see cref="CpuAddress"/> to map.</param>
        /// <param name="size">The size of the CPU memory region to map.</param>
        /// <returns>The address on the <see cref="Dsp.AudioProcessor"/> address space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DspAddress Map(uint processHandle, CpuAddress cpuAddress, ulong size)
        {
            return cpuAddress;
        }

        /// <summary>
        /// Unmap the given <see cref="CpuAddress"/> from the <see cref="Dsp.AudioProcessor"/> address space.
        /// </summary>
        /// <param name="processHandle">The process owning the CPU memory.</param>
        /// <param name="cpuAddress">The <see cref="CpuAddress"/> to unmap.</param>
        /// <param name="size">The size of the CPU memory region to unmap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unmap(uint processHandle, CpuAddress cpuAddress, ulong size)
        {
        }

        /// <summary>
        /// Invalidate the <see cref="Dsp.AudioProcessor"/> data cache at the given address.
        /// </summary>
        /// <param name="address">The base DSP address to invalidate</param>
        /// <param name="size">The size of the DSP memory region to invalidate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidateDspCache(DspAddress address, ulong size)
        {
        }

        /// <summary>
        /// Invalidate the CPU data cache at the given address.
        /// </summary>
        /// <param name="address">The base <see cref="CpuAddress"/> to invalidate</param>
        /// <param name="size">The size of the CPU memory region to invalidate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidateDataCache(CpuAddress address, ulong size)
        {
        }
    }
}
