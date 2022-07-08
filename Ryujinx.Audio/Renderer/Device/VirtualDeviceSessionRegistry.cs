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
        public VirtualDevice ActiveDevice = VirtualDevice.Devices[2];

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
