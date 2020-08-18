//
// Copyright (c) 2019-2020 Ryujinx
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

namespace Ryujinx.Audio.Renderer
{
    public enum ResultCode
    {
        ModuleId       = 153,
        ErrorCodeShift = 9,

        Success = 0,

        OperationFailed      = (2 << ErrorCodeShift) | ModuleId,
        WorkBufferTooSmall   = (4 << ErrorCodeShift) | ModuleId,
        InvalidUpdateInfo    = (41 << ErrorCodeShift) | ModuleId,
        InvalidAddressInfo   = (42 << ErrorCodeShift) | ModuleId,
        InvalidMixSorting    = (43 << ErrorCodeShift) | ModuleId,
        UnsupportedOperation = (513 << ErrorCodeShift) | ModuleId,
    }
}
