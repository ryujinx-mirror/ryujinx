using Ryujinx.HLE.HOS.Services.Ldn.Types;
using Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.Network.Types;
using Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.RyuLdn.Types;
using System;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.RyuLdn
{
    class DisabledLdnClient : INetworkClient
    {
        public event EventHandler<NetworkChangeEventArgs> NetworkChange;

        public NetworkError Connect(ConnectRequest request)
        {
            NetworkChange?.Invoke(this, new NetworkChangeEventArgs(new NetworkInfo(), false));

            return NetworkError.None;
        }

        public NetworkError ConnectPrivate(ConnectPrivateRequest request)
        {
            NetworkChange?.Invoke(this, new NetworkChangeEventArgs(new NetworkInfo(), false));

            return NetworkError.None;
        }

        public bool CreateNetwork(CreateAccessPointRequest request, byte[] advertiseData)
        {
            NetworkChange?.Invoke(this, new NetworkChangeEventArgs(new NetworkInfo(), false));

            return true;
        }

        public bool CreateNetworkPrivate(CreateAccessPointPrivateRequest request, byte[] advertiseData)
        {
            NetworkChange?.Invoke(this, new NetworkChangeEventArgs(new NetworkInfo(), false));

            return true;
        }

        public void DisconnectAndStop() { }

        public void DisconnectNetwork() { }

        public ResultCode Reject(DisconnectReason disconnectReason, uint nodeId)
        {
            return ResultCode.Success;
        }

        public NetworkInfo[] Scan(ushort channel, ScanFilter scanFilter)
        {
            return Array.Empty<NetworkInfo>();
        }

        public void SetAdvertiseData(byte[] data) { }

        public void SetGameVersion(byte[] versionString) { }

        public void SetStationAcceptPolicy(AcceptPolicy acceptPolicy) { }

        public void Dispose() { }
    }
}
