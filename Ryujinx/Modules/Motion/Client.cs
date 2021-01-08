using Force.Crc32;
using Ryujinx.Common;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Logging;
using Ryujinx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;

namespace Ryujinx.Modules.Motion
{
    public class Client : IDisposable
    {
        public const uint   Magic   = 0x43555344; // DSUC
        public const ushort Version = 1001;

        private bool _active;

        private readonly Dictionary<int, IPEndPoint> _hosts;
        private readonly Dictionary<int, Dictionary<int, MotionInput>> _motionData;
        private readonly Dictionary<int, UdpClient> _clients;

        private readonly bool[] _clientErrorStatus = new bool[Enum.GetValues(typeof(PlayerIndex)).Length];
        private readonly long[] _clientRetryTimer  = new long[Enum.GetValues(typeof(PlayerIndex)).Length];

        public Client()
        {
            _hosts      = new Dictionary<int, IPEndPoint>();
            _motionData = new Dictionary<int, Dictionary<int, MotionInput>>();
            _clients    = new Dictionary<int, UdpClient>();

            CloseClients();
        }

        public void CloseClients()
        {
            _active = false;

            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        client.Value?.Dispose();
                    }
                    catch (SocketException socketException)
                    {
                        Logger.Warning?.PrintMsg(LogClass.Hid, $"Unable to dispose motion client. Error: {socketException.ErrorCode}");
                    }
                }

                _hosts.Clear();
                _clients.Clear();
                _motionData.Clear();
            }
        }

        public void RegisterClient(int player, string host, int port)
        {
            if (_clients.ContainsKey(player) || !CanConnect(player))
            {
                return;
            }

            lock (_clients)
            {
                if (_clients.ContainsKey(player) || !CanConnect(player))
                {
                    return;
                }

                UdpClient client = null;

                try
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(host), port);

                    client = new UdpClient(host, port);

                    _clients.Add(player, client);
                    _hosts.Add(player, endPoint);

                    _active = true;

                    Task.Run(() =>
                    {
                        ReceiveLoop(player);
                    });
                }
                catch (FormatException formatException)
                {
                    if (!_clientErrorStatus[player])
                    {
                        Logger.Warning?.PrintMsg(LogClass.Hid, $"Unable to connect to motion source at {host}:{port}. Error: {formatException.Message}");

                        _clientErrorStatus[player] = true;
                    }
                }
                catch (SocketException socketException)
                {
                    if (!_clientErrorStatus[player])
                    {
                        Logger.Warning?.PrintMsg(LogClass.Hid, $"Unable to connect to motion source at {host}:{port}. Error: {socketException.ErrorCode}");

                        _clientErrorStatus[player] = true;
                    }

                    RemoveClient(player);

                    client?.Dispose();

                    SetRetryTimer(player);
                }
                catch (Exception exception)
                {
                    Logger.Warning?.PrintMsg(LogClass.Hid, $"Unable to register motion client. Error: {exception.Message}");

                    _clientErrorStatus[player] = true;

                    RemoveClient(player);

                    client?.Dispose();

                    SetRetryTimer(player);
                }
            }
        }

        public bool TryGetData(int player, int slot, out MotionInput input)
        {
            lock (_motionData)
            {
                if (_motionData.ContainsKey(player))
                {
                    if (_motionData[player].TryGetValue(slot, out input))
                    {
                        return true;
                    }
                }
            }

            input = null;

            return false;
        }

        private void RemoveClient(int clientId)
        {
            _clients?.Remove(clientId);

            _hosts?.Remove(clientId);
        }

        private void Send(byte[] data, int clientId)
        {
            if (_clients.TryGetValue(clientId, out UdpClient _client))
            {
                if (_client != null && _client.Client != null && _client.Client.Connected)
                {
                    try
                    {
                        _client?.Send(data, data.Length);
                    }
                    catch (SocketException socketException)
                    {
                        if (!_clientErrorStatus[clientId])
                        {
                            Logger.Warning?.PrintMsg(LogClass.Hid, $"Unable to send data request to motion source at {_client.Client.RemoteEndPoint}. Error: {socketException.ErrorCode}");
                        }

                        _clientErrorStatus[clientId] = true;

                        RemoveClient(clientId);

                        _client?.Dispose();

                        SetRetryTimer(clientId);
                    }
                    catch (ObjectDisposedException)
                    {
                        _clientErrorStatus[clientId] = true;

                        RemoveClient(clientId);

                        _client?.Dispose();

                        SetRetryTimer(clientId);
                    }
                }
            }
        }

        private byte[] Receive(int clientId, int timeout = 0)
        {
            if (_hosts.TryGetValue(clientId, out IPEndPoint endPoint) && _clients.TryGetValue(clientId, out UdpClient _client))
            {
                if (_client != null && _client.Client != null && _client.Client.Connected)
                {
                    _client.Client.ReceiveTimeout = timeout;

                    var result = _client?.Receive(ref endPoint);

                    if (result.Length > 0)
                    {
                        _clientErrorStatus[clientId] = false;
                    }

                    return result;
                }
            }

            throw new Exception($"Client {clientId} is not registered.");
        }

        private void SetRetryTimer(int clientId)
        {
            var elapsedMs = PerformanceCounter.ElapsedMilliseconds;

            _clientRetryTimer[clientId] = elapsedMs;
        }

        private void ResetRetryTimer(int clientId)
        {
            _clientRetryTimer[clientId] = 0;
        }

        private bool CanConnect(int clientId)
        {
            return _clientRetryTimer[clientId] == 0 || PerformanceCounter.ElapsedMilliseconds - 5000 > _clientRetryTimer[clientId];
        }

        public void ReceiveLoop(int clientId)
        {
            if (_hosts.TryGetValue(clientId, out IPEndPoint endPoint) && _clients.TryGetValue(clientId, out UdpClient _client))
            {
                if (_client != null && _client.Client != null && _client.Client.Connected)
                {
                    try
                    {
                        while (_active)
                        {
                            byte[] data = Receive(clientId);

                            if (data.Length == 0)
                            {
                                continue;
                            }

                            Task.Run(() => HandleResponse(data, clientId));
                        }
                    }
                    catch (SocketException socketException)
                    {
                        if (!_clientErrorStatus[clientId])
                        {
                            Logger.Warning?.PrintMsg(LogClass.Hid, $"Unable to receive data from motion source at {endPoint}. Error: {socketException.ErrorCode}");
                        }

                        _clientErrorStatus[clientId] = true;

                        RemoveClient(clientId);

                        _client?.Dispose();

                        SetRetryTimer(clientId);
                    }
                    catch (ObjectDisposedException)
                    {
                        _clientErrorStatus[clientId] = true;

                        RemoveClient(clientId);

                        _client?.Dispose();

                        SetRetryTimer(clientId);
                    }
                }
            }
        }

        public void HandleResponse(byte[] data, int clientId)
        {
            ResetRetryTimer(clientId);

            MessageType type = (MessageType)BitConverter.ToUInt32(data.AsSpan().Slice(16, 4));

            data = data.AsSpan()[16..].ToArray();

            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            switch (type)
            {
                case MessageType.Protocol:
                    break;
                case MessageType.Info:
                    ControllerInfoResponse contollerInfo = reader.ReadStruct<ControllerInfoResponse>();
                    break;
                case MessageType.Data:
                    ControllerDataResponse inputData = reader.ReadStruct<ControllerDataResponse>();

                    Vector3 accelerometer = new Vector3()
                    {
                        X = -inputData.AccelerometerX,
                        Y = inputData.AccelerometerZ,
                        Z = -inputData.AccelerometerY
                    };

                    Vector3 gyroscrope = new Vector3()
                    {
                        X = inputData.GyroscopePitch,
                        Y = inputData.GyroscopeRoll,
                        Z = -inputData.GyroscopeYaw
                    };

                    ulong timestamp = inputData.MotionTimestamp;

                    InputConfig config = ConfigurationState.Instance.Hid.InputConfig.Value.Find(x => x.PlayerIndex == (PlayerIndex)clientId);

                    lock (_motionData)
                    {
                        int slot = inputData.Shared.Slot;

                        if (_motionData.ContainsKey(clientId))
                        {
                            if (_motionData[clientId].ContainsKey(slot))
                            {
                                MotionInput previousData = _motionData[clientId][slot];

                                previousData.Update(accelerometer, gyroscrope, timestamp, config.Sensitivity, (float)config.GyroDeadzone);
                            }
                            else
                            {
                                MotionInput input = new MotionInput();

                                input.Update(accelerometer, gyroscrope, timestamp, config.Sensitivity, (float)config.GyroDeadzone);

                                _motionData[clientId].Add(slot, input);
                            }
                        }
                        else
                        {
                            MotionInput input = new MotionInput();

                            input.Update(accelerometer, gyroscrope, timestamp, config.Sensitivity, (float)config.GyroDeadzone);

                            _motionData.Add(clientId, new Dictionary<int, MotionInput>() { { slot, input } });
                        }
                    }
                    break;
            }
        }

        public void RequestInfo(int clientId, int slot)
        {
            if (!_active)
            {
                return;
            }

            Header header = GenerateHeader(clientId);

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.WriteStruct(header);

                ControllerInfoRequest request = new ControllerInfoRequest()
                {
                    Type       = MessageType.Info,
                    PortsCount = 4
                };

                request.PortIndices[0] = (byte)slot;

                writer.WriteStruct(request);

                header.Length = (ushort)(stream.Length - 16);

                writer.Seek(6, SeekOrigin.Begin);
                writer.Write(header.Length);

                header.Crc32 = Crc32Algorithm.Compute(stream.ToArray());

                writer.Seek(8, SeekOrigin.Begin);
                writer.Write(header.Crc32);

                byte[] data = stream.ToArray();

                Send(data, clientId);
            }
        }

        public unsafe void RequestData(int clientId, int slot)
        {
            if (!_active)
            {
                return;
            }

            Header header = GenerateHeader(clientId);

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.WriteStruct(header);

                ControllerDataRequest request = new ControllerDataRequest()
                {
                    Type           = MessageType.Data,
                    Slot           = (byte)slot,
                    SubscriberType = SubscriberType.Slot
                };

                writer.WriteStruct(request);

                header.Length = (ushort)(stream.Length - 16);

                writer.Seek(6, SeekOrigin.Begin);
                writer.Write(header.Length);

                header.Crc32 = Crc32Algorithm.Compute(stream.ToArray());

                writer.Seek(8, SeekOrigin.Begin);
                writer.Write(header.Crc32);

                byte[] data = stream.ToArray();

                Send(data, clientId);
            }
        }

        private Header GenerateHeader(int clientId)
        {
            Header header = new Header()
            {
                Id          = (uint)clientId,
                MagicString = Magic,
                Version     = Version,
                Length      = 0,
                Crc32       = 0
            };

            return header;
        }

        public void Dispose()
        {
            _active = false;

            CloseClients();
        }
    }
}