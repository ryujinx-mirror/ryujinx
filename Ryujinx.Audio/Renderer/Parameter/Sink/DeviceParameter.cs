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
using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Sink
{
    /// <summary>
    /// <see cref="SinkInParameter.SpecificData"/> for <see cref="Common.SinkType.Device"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeviceParameter
    {
        /// <summary>
        /// Device name storage.
        /// </summary>
        private DeviceNameStruct _deviceName;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private byte _padding;

        /// <summary>
        /// The total count of channels to output to the device.
        /// </summary>
        public uint InputCount;

        /// <summary>
        /// The input channels index that will be used.
        /// </summary>
        public Array6<byte> Input;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private byte _reserved;

        /// <summary>
        /// Set to true if the user controls Surround to Stereo downmixing coefficients.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool DownMixParameterEnabled;

        /// <summary>
        /// The user Surround to Stereo downmixing coefficients.
        /// </summary>
        public Array4<float> DownMixParameter;

        [StructLayout(LayoutKind.Sequential, Size = 0xFF, Pack = 1)]
        private struct DeviceNameStruct { }

        /// <summary>
        /// The output device name.
        /// </summary>
        public Span<byte> DeviceName => SpanHelpers.AsSpan<DeviceNameStruct, byte>(ref _deviceName);
    }
}
