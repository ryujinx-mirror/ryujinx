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

using System.Collections.Generic;

namespace Ryujinx.Audio.Renderer.Device
{
    /// <summary>
    /// Represent an instance containing a registry of <see cref="VirtualDeviceSession"/>.
    /// </summary>
    public class VirtualDeviceSessionRegistry
    {
        /// <summary>
        /// The session registry, used to store the sessions of a given AppletResourceId.
        /// </summary>
        private Dictionary<ulong, VirtualDeviceSession[]> _sessionsRegistry = new Dictionary<ulong, VirtualDeviceSession[]>();

        /// <summary>
        /// The default <see cref="VirtualDevice"/>.
        /// </summary>
        /// <remarks>This is used when the USB device is the default one on older revision.</remarks>
        public VirtualDevice DefaultDevice => VirtualDevice.Devices[0];

        /// <summary>
        /// The current active <see cref="VirtualDevice"/>.
        /// </summary>
        // TODO: make this configurable
        public VirtualDevice ActiveDevice = VirtualDevice.Devices[1];

        /// <summary>
        /// Get the associated <see cref="T:VirtualDeviceSession[]"/> from an AppletResourceId.
        /// </summary>
        /// <param name="resourceAppletId">The AppletResourceId used.</param>
        /// <returns>The associated <see cref="T:VirtualDeviceSession[]"/> from an AppletResourceId.</returns>
        public VirtualDeviceSession[] GetSessionByAppletResourceId(ulong resourceAppletId)
        {
            if (_sessionsRegistry.TryGetValue(resourceAppletId, out VirtualDeviceSession[] result))
            {
                return result;
            }

            result = CreateSessionsFromBehaviourContext();

            _sessionsRegistry.Add(resourceAppletId, result);

            return result;
        }

        /// <summary>
        /// Create a new array of sessions for each <see cref="VirtualDevice"/>.
        /// </summary>
        /// <returns>A new array of sessions for each <see cref="VirtualDevice"/>.</returns>
        private static VirtualDeviceSession[] CreateSessionsFromBehaviourContext()
        {
            VirtualDeviceSession[] virtualDeviceSession = new VirtualDeviceSession[VirtualDevice.Devices.Length];

            for (int i = 0; i < virtualDeviceSession.Length; i++)
            {
                virtualDeviceSession[i] = new VirtualDeviceSession(VirtualDevice.Devices[i]);
            }

            return virtualDeviceSession;
        }
    }
}
