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

using Ryujinx.Audio.Common;
using System;

namespace Ryujinx.Audio.Backends.Common
{
    public static class BackendHelper
    {
        public static int GetSampleSize(SampleFormat format)
        {
            return format switch
            {
                SampleFormat.PcmInt8 => sizeof(byte),
                SampleFormat.PcmInt16 => sizeof(ushort),
                SampleFormat.PcmInt24 => 3,
                SampleFormat.PcmInt32 => sizeof(int),
                SampleFormat.PcmFloat => sizeof(float),
                _ => throw new ArgumentException($"{format}"),
            };
        }

        public static int GetSampleCount(SampleFormat format, int channelCount, int bufferSize)
        {
            return bufferSize / GetSampleSize(format) / channelCount;
        }
    }
}
