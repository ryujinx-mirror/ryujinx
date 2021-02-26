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

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Sample format definition.
    /// </summary>
    public enum SampleFormat : byte
    {
        /// <summary>
        /// Invalid sample format.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// PCM8 sample format. (unsupported)
        /// </summary>
        PcmInt8 = 1,

        /// <summary>
        /// PCM16 sample format.
        /// </summary>
        PcmInt16 = 2,

        /// <summary>
        /// PCM24 sample format. (unsupported)
        /// </summary>
        PcmInt24 = 3,

        /// <summary>
        /// PCM32 sample format.
        /// </summary>
        PcmInt32 = 4,

        /// <summary>
        /// PCM Float sample format.
        /// </summary>
        PcmFloat = 5,

        /// <summary>
        /// ADPCM sample format. (Also known as GC-ADPCM)
        /// </summary>
        Adpcm = 6
    }
}
