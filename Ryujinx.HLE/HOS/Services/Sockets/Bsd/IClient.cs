using Ryujinx.Common.Logging;
using Ryujinx.HLE.Utilities;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    [Service("bsd:s", true)]
    [Service("bsd:u", false)]
    class IClient : IpcService
    {
        private static Dictionary<WsaError, LinuxError> _errorMap = new Dictionary<WsaError, LinuxError>
        {
            // WSAEINTR
            {WsaError.WSAEINTR,           LinuxError.EINTR},
            // WSAEWOULDBLOCK
            {WsaError.WSAEWOULDBLOCK,     LinuxError.EWOULDBLOCK},
            // WSAEINPROGRESS
            {WsaError.WSAEINPROGRESS,     LinuxError.EINPROGRESS},
            // WSAEALREADY
            {WsaError.WSAEALREADY,        LinuxError.EALREADY},
            // WSAENOTSOCK
            {WsaError.WSAENOTSOCK,        LinuxError.ENOTSOCK},
            // WSAEDESTADDRREQ
            {WsaError.WSAEDESTADDRREQ,    LinuxError.EDESTADDRREQ},
            // WSAEMSGSIZE
            {WsaError.WSAEMSGSIZE,        LinuxError.EMSGSIZE},
            // WSAEPROTOTYPE
            {WsaError.WSAEPROTOTYPE,      LinuxError.EPROTOTYPE},
            // WSAENOPROTOOPT
            {WsaError.WSAENOPROTOOPT,     LinuxError.ENOPROTOOPT},
            // WSAEPROTONOSUPPORT
            {WsaError.WSAEPROTONOSUPPORT, LinuxError.EPROTONOSUPPORT},
            // WSAESOCKTNOSUPPORT
            {WsaError.WSAESOCKTNOSUPPORT, LinuxError.ESOCKTNOSUPPORT},
            // WSAEOPNOTSUPP
            {WsaError.WSAEOPNOTSUPP,      LinuxError.EOPNOTSUPP},
            // WSAEPFNOSUPPORT
            {WsaError.WSAEPFNOSUPPORT,    LinuxError.EPFNOSUPPORT},
            // WSAEAFNOSUPPORT
            {WsaError.WSAEAFNOSUPPORT,    LinuxError.EAFNOSUPPORT},
            // WSAEADDRINUSE
            {WsaError.WSAEADDRINUSE,      LinuxError.EADDRINUSE},
            // WSAEADDRNOTAVAIL
            {WsaError.WSAEADDRNOTAVAIL,   LinuxError.EADDRNOTAVAIL},
            // WSAENETDOWN
            {WsaError.WSAENETDOWN,        LinuxError.ENETDOWN},
            // WSAENETUNREACH
            {WsaError.WSAENETUNREACH,     LinuxError.ENETUNREACH},
            // WSAENETRESET
            {WsaError.WSAENETRESET,       LinuxError.ENETRESET},
            // WSAECONNABORTED
            {WsaError.WSAECONNABORTED,    LinuxError.ECONNABORTED},
            // WSAECONNRESET
            {WsaError.WSAECONNRESET,      LinuxError.ECONNRESET},
            // WSAENOBUFS
            {WsaError.WSAENOBUFS,         LinuxError.ENOBUFS},
            // WSAEISCONN
            {WsaError.WSAEISCONN,         LinuxError.EISCONN},
            // WSAENOTCONN
            {WsaError.WSAENOTCONN,        LinuxError.ENOTCONN},
            // WSAESHUTDOWN
            {WsaError.WSAESHUTDOWN,       LinuxError.ESHUTDOWN},
            // WSAETOOMANYREFS
            {WsaError.WSAETOOMANYREFS,    LinuxError.ETOOMANYREFS},
            // WSAETIMEDOUT
            {WsaError.WSAETIMEDOUT,       LinuxError.ETIMEDOUT},
            // WSAECONNREFUSED
            {WsaError.WSAECONNREFUSED,    LinuxError.ECONNREFUSED},
            // WSAELOOP
            {WsaError.WSAELOOP,           LinuxError.ELOOP},
            // WSAENAMETOOLONG
            {WsaError.WSAENAMETOOLONG,    LinuxError.ENAMETOOLONG},
            // WSAEHOSTDOWN
            {WsaError.WSAEHOSTDOWN,       LinuxError.EHOSTDOWN},
            // WSAEHOSTUNREACH
            {WsaError.WSAEHOSTUNREACH,    LinuxError.EHOSTUNREACH},
            // WSAENOTEMPTY
            {WsaError.WSAENOTEMPTY,       LinuxError.ENOTEMPTY},
            // WSAEUSERS
            {WsaError.WSAEUSERS,          LinuxError.EUSERS},
            // WSAEDQUOT
            {WsaError.WSAEDQUOT,          LinuxError.EDQUOT},
            // WSAESTALE
            {WsaError.WSAESTALE,          LinuxError.ESTALE},
            // WSAEREMOTE
            {WsaError.WSAEREMOTE,         LinuxError.EREMOTE},
            // WSAEINVAL
            {WsaError.WSAEINVAL,          LinuxError.EINVAL},
            // WSAEFAULT
            {WsaError.WSAEFAULT,          LinuxError.EFAULT},
            // NOERROR
            {0, 0}
        };

        private bool _isPrivileged;

        private List<BsdSocket> _sockets = new List<BsdSocket>();

        public IClient(ServiceCtx context, bool isPrivileged) : base(context.Device.System.BsdServer)
        {
            _isPrivileged = isPrivileged;
        }

        private LinuxError ConvertError(WsaError errorCode)
        {
            if (!_errorMap.TryGetValue(errorCode, out LinuxError errno))
            {
                errno = (LinuxError)errorCode;
            }

            return errno;
        }

        private ResultCode WriteWinSock2Error(ServiceCtx context, WsaError errorCode)
        {
            return WriteBsdResult(context, -1, ConvertError(errorCode));
        }

        private ResultCode WriteBsdResult(ServiceCtx context, int result, LinuxError errorCode = 0)
        {
            if (errorCode != LinuxError.SUCCESS)
            {
                result = -1;
            }

            context.ResponseData.Write(result);
            context.ResponseData.Write((int)errorCode);

            return ResultCode.Success;
        }

        private BsdSocket RetrieveSocket(int socketFd)
        {
            if (socketFd >= 0 && _sockets.Count > socketFd)
            {
                return _sockets[socketFd];
            }

            return null;
        }

        private LinuxError SetResultErrno(Socket socket, int result)
        {
            return result == 0 && !socket.Blocking ? LinuxError.EWOULDBLOCK : LinuxError.SUCCESS;
        }

        private AddressFamily ConvertFromBsd(int domain)
        {
            if (domain == 2)
            {
                return AddressFamily.InterNetwork;
            }

            // FIXME: AF_ROUTE ignored, is that really needed?
            return AddressFamily.Unknown;
        }

        private ResultCode SocketInternal(ServiceCtx context, bool exempt)
        {
            AddressFamily domain   = (AddressFamily)context.RequestData.ReadInt32();
            SocketType    type     = (SocketType)context.RequestData.ReadInt32();
            ProtocolType  protocol = (ProtocolType)context.RequestData.ReadInt32();

            if (domain == AddressFamily.Unknown)
            {
                return WriteBsdResult(context, -1, LinuxError.EPROTONOSUPPORT);
            }
            else if ((type == SocketType.Seqpacket || type == SocketType.Raw) && !_isPrivileged)
            {
                if (domain != AddressFamily.InterNetwork || type != SocketType.Raw || protocol != ProtocolType.Icmp)
                {
                    return WriteBsdResult(context, -1, LinuxError.ENOENT);
                }
            }

            BsdSocket newBsdSocket = new BsdSocket
            {
                Family   = (int)domain,
                Type     = (int)type,
                Protocol = (int)protocol,
                Handle   = new Socket(domain, type, protocol)
            };

            _sockets.Add(newBsdSocket);

            if (exempt)
            {
                newBsdSocket.Handle.Disconnect(true);
            }

            return WriteBsdResult(context, _sockets.Count - 1);
        }

        private IPEndPoint ParseSockAddr(ServiceCtx context, long bufferPosition, long bufferSize)
        {
            int size   = context.Memory.Read<byte>((ulong)bufferPosition);
            int family = context.Memory.Read<byte>((ulong)bufferPosition + 1);
            int port   = BinaryPrimitives.ReverseEndianness(context.Memory.Read<ushort>((ulong)bufferPosition + 2));

            byte[] rawIp = new byte[4];

            context.Memory.Read((ulong)bufferPosition + 4, rawIp);

            return new IPEndPoint(new IPAddress(rawIp), port);
        }

        private void WriteSockAddr(ServiceCtx context, long bufferPosition, IPEndPoint endPoint)
        {
            context.Memory.Write((ulong)bufferPosition, (byte)0);
            context.Memory.Write((ulong)bufferPosition + 1, (byte)endPoint.AddressFamily);
            context.Memory.Write((ulong)bufferPosition + 2, BinaryPrimitives.ReverseEndianness((ushort)endPoint.Port));
            context.Memory.Write((ulong)bufferPosition + 4, endPoint.Address.GetAddressBytes());
        }

        private void WriteSockAddr(ServiceCtx context, long bufferPosition, BsdSocket socket, bool isRemote)
        {
            IPEndPoint endPoint = (isRemote ? socket.Handle.RemoteEndPoint : socket.Handle.LocalEndPoint) as IPEndPoint;

            WriteSockAddr(context, bufferPosition, endPoint);
        }

        [Command(0)]
        // Initialize(nn::socket::BsdBufferConfig config, u64 pid, u64 transferMemorySize, KObject<copy, transfer_memory>, pid) -> u32 bsd_errno
        public ResultCode RegisterClient(ServiceCtx context)
        {
            /*
            typedef struct  {
                u32 version;                // Observed 1 on 2.0 LibAppletWeb, 2 on 3.0.
                u32 tcp_tx_buf_size;        // Size of the TCP transfer (send) buffer (initial or fixed).
                u32 tcp_rx_buf_size;        // Size of the TCP recieve buffer (initial or fixed).
                u32 tcp_tx_buf_max_size;    // Maximum size of the TCP transfer (send) buffer. If it is 0, the size of the buffer is fixed to its initial value.
                u32 tcp_rx_buf_max_size;    // Maximum size of the TCP receive buffer. If it is 0, the size of the buffer is fixed to its initial value.
                u32 udp_tx_buf_size;        // Size of the UDP transfer (send) buffer (typically 0x2400 bytes).
                u32 udp_rx_buf_size;        // Size of the UDP receive buffer (typically 0xA500 bytes).
                u32 sb_efficiency;          // Number of buffers for each socket (standard values range from 1 to 8).
            } BsdBufferConfig;
            */

            // bsd_error
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceBsd);

            // Close transfer memory immediately as we don't use it.
            context.Device.System.KernelContext.Syscall.CloseHandle(context.Request.HandleDesc.ToCopy[0]);

            return ResultCode.Success;
        }

        [Command(1)]
        // StartMonitoring(u64, pid)
        public ResultCode StartMonitoring(ServiceCtx context)
        {
            ulong unknown0 = context.RequestData.ReadUInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceBsd, new { unknown0 });

            return ResultCode.Success;
        }

        [Command(2)]
        // Socket(u32 domain, u32 type, u32 protocol) -> (i32 ret, u32 bsd_errno)
        public ResultCode Socket(ServiceCtx context)
        {
            return SocketInternal(context, false);
        }

        [Command(3)]
        // SocketExempt(u32 domain, u32 type, u32 protocol) -> (i32 ret, u32 bsd_errno)
        public ResultCode SocketExempt(ServiceCtx context)
        {
            return SocketInternal(context, true);
        }

        [Command(4)]
        // Open(u32 flags, array<unknown, 0x21> path) -> (i32 ret, u32 bsd_errno)
        public ResultCode Open(ServiceCtx context)
        {
            (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x21();

            int flags = context.RequestData.ReadInt32();

            byte[] rawPath = new byte[bufferSize];

            context.Memory.Read((ulong)bufferPosition, rawPath);

            string path = Encoding.ASCII.GetString(rawPath);

            WriteBsdResult(context, -1, LinuxError.EOPNOTSUPP);

            Logger.Stub?.PrintStub(LogClass.ServiceBsd, new { path, flags });

            return ResultCode.Success;
        }

        [Command(5)]
        // Select(u32 nfds, nn::socket::timeout timeout, buffer<nn::socket::fd_set, 0x21, 0> readfds_in, buffer<nn::socket::fd_set, 0x21, 0> writefds_in, buffer<nn::socket::fd_set, 0x21, 0> errorfds_in) -> (i32 ret, u32 bsd_errno, buffer<nn::socket::fd_set, 0x22, 0> readfds_out, buffer<nn::socket::fd_set, 0x22, 0> writefds_out, buffer<nn::socket::fd_set, 0x22, 0> errorfds_out)
        public ResultCode Select(ServiceCtx context)
        {
            WriteBsdResult(context, -1, LinuxError.EOPNOTSUPP);

            Logger.Stub?.PrintStub(LogClass.ServiceBsd);

            return ResultCode.Success;
        }

        [Command(6)]
        // Poll(u32 nfds, u32 timeout, buffer<unknown, 0x21, 0> fds) -> (i32 ret, u32 bsd_errno, buffer<unknown, 0x22, 0>)
        public ResultCode Poll(ServiceCtx context)
        {
            int fdsCount = context.RequestData.ReadInt32();
            int timeout  = context.RequestData.ReadInt32();

            (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x21();


            if (timeout < -1 || fdsCount < 0 || (fdsCount * 8) > bufferSize)
            {
                return WriteBsdResult(context, -1, LinuxError.EINVAL);
            }

            PollEvent[] events = new PollEvent[fdsCount];

            for (int i = 0; i < fdsCount; i++)
            {
                int socketFd = context.Memory.Read<int>((ulong)(bufferPosition + i * 8));

                BsdSocket socket = RetrieveSocket(socketFd);

                if (socket == null)
                {
                    return WriteBsdResult(context, -1, LinuxError.EBADF);}

                PollEvent.EventTypeMask inputEvents  = (PollEvent.EventTypeMask)context.Memory.Read<short>((ulong)(bufferPosition + i * 8 + 4));
                PollEvent.EventTypeMask outputEvents = (PollEvent.EventTypeMask)context.Memory.Read<short>((ulong)(bufferPosition + i * 8 + 6));

                events[i] = new PollEvent(socketFd, socket, inputEvents, outputEvents);
            }

            List<Socket> readEvents  = new List<Socket>();
            List<Socket> writeEvents = new List<Socket>();
            List<Socket> errorEvents = new List<Socket>();

            foreach (PollEvent Event in events)
            {
                bool isValidEvent = false;

                if ((Event.InputEvents & PollEvent.EventTypeMask.Input) != 0)
                {
                    readEvents.Add(Event.Socket.Handle);
                    errorEvents.Add(Event.Socket.Handle);

                    isValidEvent = true;
                }

                if ((Event.InputEvents & PollEvent.EventTypeMask.UrgentInput) != 0)
                {
                    readEvents.Add(Event.Socket.Handle);
                    errorEvents.Add(Event.Socket.Handle);

                    isValidEvent = true;
                }

                if ((Event.InputEvents & PollEvent.EventTypeMask.Output) != 0)
                {
                    writeEvents.Add(Event.Socket.Handle);
                    errorEvents.Add(Event.Socket.Handle);

                    isValidEvent = true;
                }

                if ((Event.InputEvents & PollEvent.EventTypeMask.Error) != 0)
                {
                    errorEvents.Add(Event.Socket.Handle);
                    isValidEvent = true;
                }

                if (!isValidEvent)
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Poll input event type: {Event.InputEvents}");
                    return WriteBsdResult(context, -1, LinuxError.EINVAL);
                }
            }

            if (fdsCount != 0)
            {
                try
                {
                    System.Net.Sockets.Socket.Select(readEvents, writeEvents, errorEvents, timeout);
                }
                catch (SocketException exception)
                {
                    return WriteWinSock2Error(context, (WsaError)exception.ErrorCode);
                }
            }
            else if (timeout == -1)
            {
                // FIXME: If we get a timeout of -1 and there is no fds to wait on, this should kill the KProces. (need to check that with re)
                throw new InvalidOperationException();
            }
            else
            {
                // FIXME: We should make the KThread sleep but we can't do much about it yet.
                Thread.Sleep(timeout);
            }

            for (int i = 0; i < fdsCount; i++)
            {
                PollEvent Event = events[i];
                context.Memory.Write((ulong)(bufferPosition + i * 8), Event.SocketFd);
                context.Memory.Write((ulong)(bufferPosition + i * 8 + 4), (short)Event.InputEvents);

                PollEvent.EventTypeMask outputEvents = 0;

                Socket socket = Event.Socket.Handle;

                if (errorEvents.Contains(socket))
                {
                    outputEvents |= PollEvent.EventTypeMask.Error;

                    if (!socket.Connected || !socket.IsBound)
                    {
                        outputEvents |= PollEvent.EventTypeMask.Disconnected;
                    }
                }

                if (readEvents.Contains(socket))
                {
                    if ((Event.InputEvents & PollEvent.EventTypeMask.Input) != 0)
                    {
                        outputEvents |= PollEvent.EventTypeMask.Input;
                    }
                }

                if (writeEvents.Contains(socket))
                {
                    outputEvents |= PollEvent.EventTypeMask.Output;
                }

                context.Memory.Write((ulong)(bufferPosition + i * 8 + 6), (short)outputEvents);
            }

            return WriteBsdResult(context, readEvents.Count + writeEvents.Count + errorEvents.Count, LinuxError.SUCCESS);
        }

        [Command(7)]
        // Sysctl(buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>) -> (i32 ret, u32 bsd_errno, u32, buffer<unknown, 0x22, 0>)
        public ResultCode Sysctl(ServiceCtx context)
        {
            WriteBsdResult(context, -1, LinuxError.EOPNOTSUPP);

            Logger.Stub?.PrintStub(LogClass.ServiceBsd);

            return ResultCode.Success;
        }

        [Command(8)]
        // Recv(u32 socket, u32 flags) -> (i32 ret, u32 bsd_errno, array<i8, 0x22> message)
        public ResultCode Recv(ServiceCtx context)
        {
            int         socketFd    = context.RequestData.ReadInt32();
            SocketFlags socketFlags = (SocketFlags)context.RequestData.ReadInt32();

            (long receivePosition, long receiveLength) = context.Request.GetBufferType0x22();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);
            int        result = -1;

            if (socket != null)
            {
                if (socketFlags != SocketFlags.None && (socketFlags & SocketFlags.OutOfBand) == 0
                    && (socketFlags & SocketFlags.Peek) == 0)
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Recv flags: {socketFlags}");
                    return WriteBsdResult(context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[] receivedBuffer = new byte[receiveLength];

                try
                {
                    result = socket.Handle.Receive(receivedBuffer, socketFlags);
                    errno  = SetResultErrno(socket.Handle, result);

                    context.Memory.Write((ulong)receivePosition, receivedBuffer);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }
            }

            return WriteBsdResult(context, result, errno);
        }

        [Command(9)]
        // RecvFrom(u32 sock, u32 flags) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<i8, 0x22, 0> message, buffer<nn::socket::sockaddr_in, 0x22, 0x10>)
        public ResultCode RecvFrom(ServiceCtx context)
        {
            int         socketFd    = context.RequestData.ReadInt32();
            SocketFlags socketFlags = (SocketFlags)context.RequestData.ReadInt32();

            (long receivePosition,     long receiveLength)   = context.Request.GetBufferType0x22();
            (long sockAddrOutPosition, long sockAddrOutSize) = context.Request.GetBufferType0x22(1);

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);
            int        result = -1;

            if (socket != null)
            {
                if (socketFlags != SocketFlags.None && (socketFlags & SocketFlags.OutOfBand) == 0
                    && (socketFlags & SocketFlags.Peek) == 0)
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Recv flags: {socketFlags}");

                    return WriteBsdResult(context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[]   receivedBuffer = new byte[receiveLength];
                EndPoint endPoint       = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    result = socket.Handle.ReceiveFrom(receivedBuffer, receivedBuffer.Length, socketFlags, ref endPoint);
                    errno  = SetResultErrno(socket.Handle, result);

                    context.Memory.Write((ulong)receivePosition, receivedBuffer);
                    WriteSockAddr(context, sockAddrOutPosition, (IPEndPoint)endPoint);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }
            }

            return WriteBsdResult(context, result, errno);
        }

        [Command(10)]
        // Send(u32 socket, u32 flags, buffer<i8, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public ResultCode Send(ServiceCtx context)
        {
            int         socketFd    = context.RequestData.ReadInt32();
            SocketFlags socketFlags = (SocketFlags)context.RequestData.ReadInt32();

            (long sendPosition, long sendSize) = context.Request.GetBufferType0x21();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);
            int        result = -1;

            if (socket != null)
            {
                if (socketFlags != SocketFlags.None && socketFlags != SocketFlags.OutOfBand
                    && socketFlags != SocketFlags.Peek && socketFlags != SocketFlags.DontRoute)
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Send flags: {socketFlags}");

                    return WriteBsdResult(context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[] sendBuffer = new byte[sendSize];

                context.Memory.Read((ulong)sendPosition, sendBuffer);

                try
                {
                    result = socket.Handle.Send(sendBuffer, socketFlags);
                    errno  = SetResultErrno(socket.Handle, result);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }

            }

            return WriteBsdResult(context, result, errno);
        }

        [Command(11)]
        // SendTo(u32 socket, u32 flags, buffer<i8, 0x21, 0>, buffer<nn::socket::sockaddr_in, 0x21, 0x10>) -> (i32 ret, u32 bsd_errno)
        public ResultCode SendTo(ServiceCtx context)
        {
            int         socketFd    = context.RequestData.ReadInt32();
            SocketFlags socketFlags = (SocketFlags)context.RequestData.ReadInt32();

            (long sendPosition,   long sendSize)   = context.Request.GetBufferType0x21();
            (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x21(1);

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);
            int        result = -1;

            if (socket != null)
            {
                if (socketFlags != SocketFlags.None && socketFlags != SocketFlags.OutOfBand
                    && socketFlags != SocketFlags.Peek && socketFlags != SocketFlags.DontRoute)
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Send flags: {socketFlags}");

                    return WriteBsdResult(context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[] sendBuffer = new byte[sendSize];

                context.Memory.Read((ulong)sendPosition, sendBuffer);

                EndPoint endPoint = ParseSockAddr(context, bufferPosition, bufferSize);

                try
                {
                    result = socket.Handle.SendTo(sendBuffer, sendBuffer.Length, socketFlags, endPoint);
                    errno  = SetResultErrno(socket.Handle, result);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }

            }

            return WriteBsdResult(context, result, errno);
        }

        [Command(12)]
        // Accept(u32 socket) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<nn::socket::sockaddr_in, 0x22, 0x10> addr)
        public ResultCode Accept(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            (long bufferPos, long bufferSize) = context.Request.GetBufferType0x22();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.SUCCESS;

                Socket newSocket = null;

                try
                {
                    newSocket = socket.Handle.Accept();
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }

                if (newSocket == null && errno == LinuxError.SUCCESS)
                {
                    errno = LinuxError.EWOULDBLOCK;
                }
                else if (errno == LinuxError.SUCCESS)
                {
                    BsdSocket newBsdSocket = new BsdSocket
                    {
                        Family   = (int)newSocket.AddressFamily,
                        Type     = (int)newSocket.SocketType,
                        Protocol = (int)newSocket.ProtocolType,
                        Handle   = newSocket
                    };

                    _sockets.Add(newBsdSocket);

                    WriteSockAddr(context, bufferPos, newBsdSocket, true);

                    WriteBsdResult(context, _sockets.Count - 1, errno);

                    context.ResponseData.Write(0x10);

                    return ResultCode.Success;
                }
            }

            return WriteBsdResult(context, -1, errno);
        }

        [Command(13)]
        // Bind(u32 socket, buffer<nn::socket::sockaddr_in, 0x21, 0x10> addr) -> (i32 ret, u32 bsd_errno)
        public ResultCode Bind(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            (long bufferPos, long bufferSize) = context.Request.GetBufferType0x21();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.SUCCESS;

                try
                {
                    IPEndPoint endPoint = ParseSockAddr(context, bufferPos, bufferSize);

                    socket.Handle.Bind(endPoint);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(14)]
        // Connect(u32 socket, buffer<nn::socket::sockaddr_in, 0x21, 0x10>) -> (i32 ret, u32 bsd_errno)
        public ResultCode Connect(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            (long bufferPos, long bufferSize) = context.Request.GetBufferType0x21();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.SUCCESS;
                try
                {
                    IPEndPoint endPoint = ParseSockAddr(context, bufferPos, bufferSize);

                    socket.Handle.Connect(endPoint);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(15)]
        // GetPeerName(u32 socket) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<nn::socket::sockaddr_in, 0x22, 0x10> addr)
        public ResultCode GetPeerName(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            (long bufferPos, long bufferSize) = context.Request.GetBufferType0x22();

            LinuxError  errno  = LinuxError.EBADF;
            BsdSocket socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.SUCCESS;

                WriteSockAddr(context, bufferPos, socket, true);
                WriteBsdResult(context, 0, errno);
                context.ResponseData.Write(0x10);
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(16)]
        // GetSockName(u32 socket) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<nn::socket::sockaddr_in, 0x22, 0x10> addr)
        public ResultCode GetSockName(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            (long bufferPos, long bufferSize) = context.Request.GetBufferType0x22();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.SUCCESS;

                WriteSockAddr(context, bufferPos, socket, false);
                WriteBsdResult(context, 0, errno);
                context.ResponseData.Write(0x10);
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(17)]
        // GetSockOpt(u32 socket, u32 level, u32 option_name) -> (i32 ret, u32 bsd_errno, u32, buffer<unknown, 0x22, 0>)
        public ResultCode GetSockOpt(ServiceCtx context)
        {
            int socketFd   = context.RequestData.ReadInt32();
            int level      = context.RequestData.ReadInt32();
            int optionName = context.RequestData.ReadInt32();

            (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x22();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.ENOPROTOOPT;

                if (level == 0xFFFF)
                {
                    errno = HandleGetSocketOption(context, socket, (SocketOptionName)optionName, bufferPosition, bufferSize);
                }
                else
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported GetSockOpt Level: {(SocketOptionLevel)level}");
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(18)]
        // Listen(u32 socket, u32 backlog) -> (i32 ret, u32 bsd_errno)
        public ResultCode Listen(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();
            int backlog  = context.RequestData.ReadInt32();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.SUCCESS;

                try
                {
                    socket.Handle.Listen(backlog);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(19)]
        // Ioctl(u32 fd, u32 request, u32 bufcount, buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>) -> (i32 ret, u32 bsd_errno, buffer<unknown, 0x22, 0>, buffer<unknown, 0x22, 0>, buffer<unknown, 0x22, 0>, buffer<unknown, 0x22, 0>)
        public ResultCode Ioctl(ServiceCtx context)
        {
            int      socketFd    = context.RequestData.ReadInt32();
            BsdIoctl cmd         = (BsdIoctl)context.RequestData.ReadInt32();
            int      bufferCount = context.RequestData.ReadInt32();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                switch (cmd)
                {
                    case BsdIoctl.AtMark:
                        errno = LinuxError.SUCCESS;

                        (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x22();

                        // FIXME: OOB not implemented.
                        context.Memory.Write((ulong)bufferPosition, 0);
                        break;

                    default:
                        errno = LinuxError.EOPNOTSUPP;

                        Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Ioctl Cmd: {cmd}");
                        break;
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(20)]
        // Fcntl(u32 socket, u32 cmd, u32 arg) -> (i32 ret, u32 bsd_errno)
        public ResultCode Fcntl(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();
            int cmd      = context.RequestData.ReadInt32();
            int arg      = context.RequestData.ReadInt32();

            int        result = 0;
            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.SUCCESS;

                if (cmd == 0x3)
                {
                    result = !socket.Handle.Blocking ? 0x800 : 0;
                }
                else if (cmd == 0x4 && arg == 0x800)
                {
                    socket.Handle.Blocking = false;
                    result = 0;
                }
                else
                {
                    errno = LinuxError.EOPNOTSUPP;
                }
            }

            return WriteBsdResult(context, result, errno);
        }

        private LinuxError HandleGetSocketOption(ServiceCtx context, BsdSocket socket, SocketOptionName optionName, long optionValuePosition, long optionValueSize)
        {
            try
            {
                byte[] optionValue = new byte[optionValueSize];

                switch (optionName)
                {
                    case SocketOptionName.Broadcast:
                    case SocketOptionName.DontLinger:
                    case SocketOptionName.Debug:
                    case SocketOptionName.Error:
                    case SocketOptionName.KeepAlive:
                    case SocketOptionName.OutOfBandInline:
                    case SocketOptionName.ReceiveBuffer:
                    case SocketOptionName.ReceiveTimeout:
                    case SocketOptionName.SendBuffer:
                    case SocketOptionName.SendTimeout:
                    case SocketOptionName.Type:
                    case SocketOptionName.Linger:
                        socket.Handle.GetSocketOption(SocketOptionLevel.Socket, optionName, optionValue);
                        context.Memory.Write((ulong)optionValuePosition, optionValue);

                        return LinuxError.SUCCESS;

                    case (SocketOptionName)0x200:
                        socket.Handle.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue);
                        context.Memory.Write((ulong)optionValuePosition, optionValue);

                        return LinuxError.SUCCESS;

                    default:
                        Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported SetSockOpt OptionName: {optionName}");

                        return LinuxError.EOPNOTSUPP;
                }
            }
            catch (SocketException exception)
            {
                return ConvertError((WsaError)exception.ErrorCode);
            }
        }

        private LinuxError HandleSetSocketOption(ServiceCtx context, BsdSocket socket, SocketOptionName optionName, long optionValuePosition, long optionValueSize)
        {
            try
            {
                switch (optionName)
                {
                    case SocketOptionName.Broadcast:
                    case SocketOptionName.DontLinger:
                    case SocketOptionName.Debug:
                    case SocketOptionName.Error:
                    case SocketOptionName.KeepAlive:
                    case SocketOptionName.OutOfBandInline:
                    case SocketOptionName.ReceiveBuffer:
                    case SocketOptionName.ReceiveTimeout:
                    case SocketOptionName.SendBuffer:
                    case SocketOptionName.SendTimeout:
                    case SocketOptionName.Type:
                    case SocketOptionName.ReuseAddress:
                        socket.Handle.SetSocketOption(SocketOptionLevel.Socket, optionName, context.Memory.Read<int>((ulong)optionValuePosition));

                        return LinuxError.SUCCESS;

                    case (SocketOptionName)0x200:
                        socket.Handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, context.Memory.Read<int>((ulong)optionValuePosition));

                        return LinuxError.SUCCESS;

                    case SocketOptionName.Linger:
                        socket.Handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger,
                            new LingerOption(context.Memory.Read<int>((ulong)optionValuePosition) != 0, context.Memory.Read<int>((ulong)optionValuePosition + 4)));

                        return LinuxError.SUCCESS;

                    default:
                        Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported SetSockOpt OptionName: {optionName}");

                        return LinuxError.EOPNOTSUPP;
                }
            }
            catch (SocketException exception)
            {
                return ConvertError((WsaError)exception.ErrorCode);
            }
        }

        [Command(21)]
        // SetSockOpt(u32 socket, u32 level, u32 option_name, buffer<unknown, 0x21, 0> option_value) -> (i32 ret, u32 bsd_errno)
        public ResultCode SetSockOpt(ServiceCtx context)
        {
            int socketFd   = context.RequestData.ReadInt32();
            int level      = context.RequestData.ReadInt32();
            int optionName = context.RequestData.ReadInt32();

            (long bufferPos, long bufferSize) = context.Request.GetBufferType0x21();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.ENOPROTOOPT;

                if (level == 0xFFFF)
                {
                    errno = HandleSetSocketOption(context, socket, (SocketOptionName)optionName, bufferPos, bufferSize);
                }
                else
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported SetSockOpt Level: {(SocketOptionLevel)level}");
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(22)]
        // Shutdown(u32 socket, u32 how) -> (i32 ret, u32 bsd_errno)
        public ResultCode Shutdown(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();
            int how      = context.RequestData.ReadInt32();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                errno = LinuxError.EINVAL;

                if (how >= 0 && how <= 2)
                {
                    errno = LinuxError.SUCCESS;

                    try
                    {
                        socket.Handle.Shutdown((SocketShutdown)how);
                    }
                    catch (SocketException exception)
                    {
                        errno = ConvertError((WsaError)exception.ErrorCode);
                    }
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(23)]
        // ShutdownAllSockets(u32 how) -> (i32 ret, u32 bsd_errno)
        public ResultCode ShutdownAllSockets(ServiceCtx context)
        {
            int how = context.RequestData.ReadInt32();

            LinuxError errno = LinuxError.EINVAL;

            if (how >= 0 && how <= 2)
            {
                errno = LinuxError.SUCCESS;

                foreach (BsdSocket socket in _sockets)
                {
                    if (socket != null)
                    {
                        try
                        {
                            socket.Handle.Shutdown((SocketShutdown)how);
                        }
                        catch (SocketException exception)
                        {
                            errno = ConvertError((WsaError)exception.ErrorCode);
                            break;
                        }
                    }
                }
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(24)]
        // Write(u32 socket, buffer<i8, 0x21, 0> message) -> (i32 ret, u32 bsd_errno)
        public ResultCode Write(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            (long sendPosition, long sendSize) = context.Request.GetBufferType0x21();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);
            int        result = -1;

            if (socket != null)
            {
                byte[] sendBuffer = new byte[sendSize];

                context.Memory.Read((ulong)sendPosition, sendBuffer);

                try
                {
                    result = socket.Handle.Send(sendBuffer);
                    errno  = SetResultErrno(socket.Handle, result);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }
            }

            return WriteBsdResult(context, result, errno);
        }

        [Command(25)]
        // Read(u32 socket) -> (i32 ret, u32 bsd_errno, buffer<i8, 0x22, 0> message)
        public ResultCode Read(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            (long receivePosition, long receiveLength) = context.Request.GetBufferType0x22();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);
            int        result = -1;

            if (socket != null)
            {
                byte[] receivedBuffer = new byte[receiveLength];

                try
                {
                    result = socket.Handle.Receive(receivedBuffer);
                    errno  = SetResultErrno(socket.Handle, result);
                    context.Memory.Write((ulong)receivePosition, receivedBuffer);
                }
                catch (SocketException exception)
                {
                    errno = ConvertError((WsaError)exception.ErrorCode);
                }
            }

            return WriteBsdResult(context, result, errno);
        }

        [Command(26)]
        // Close(u32 socket) -> (i32 ret, u32 bsd_errno)
        public ResultCode Close(ServiceCtx context)
        {
            int socketFd = context.RequestData.ReadInt32();

            LinuxError errno  = LinuxError.EBADF;
            BsdSocket  socket = RetrieveSocket(socketFd);

            if (socket != null)
            {
                socket.Handle.Close();

                _sockets[socketFd] = null;

                errno = LinuxError.SUCCESS;
            }

            return WriteBsdResult(context, 0, errno);
        }

        [Command(27)]
        // DuplicateSocket(u32 socket, u64 reserved) -> (i32 ret, u32 bsd_errno)
        public ResultCode DuplicateSocket(ServiceCtx context)
        {
            int   socketFd = context.RequestData.ReadInt32();
            ulong reserved = context.RequestData.ReadUInt64();

            LinuxError errno     = LinuxError.ENOENT;
            int        newSockFd = -1;

            if (_isPrivileged)
            {
                errno = LinuxError.EBADF;

                BsdSocket oldSocket = RetrieveSocket(socketFd);

                if (oldSocket != null)
                {
                    _sockets.Add(oldSocket);
                    newSockFd = _sockets.Count - 1;
                }
            }

            return WriteBsdResult(context, newSockFd, errno);
        }
    }
}