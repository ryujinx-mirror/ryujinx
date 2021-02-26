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

namespace Ryujinx.Audio.Integration
{
    /// <summary>
    /// Represent a writable event with manual clear.
    /// </summary>
    public interface IWritableEvent
    {
        /// <summary>
        /// Signal the event.
        /// </summary>
        void Signal();

        /// <summary>
        /// Clear the signaled state of the event.
        /// </summary>
        void Clear();
    }
}
