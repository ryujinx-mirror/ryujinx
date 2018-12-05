using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Bsd
{
    class IClient : IpcService
    {

        private static Dictionary<WSAError, LinuxError> ErrorMap = new Dictionary<WSAError, LinuxError>
        {
            // WSAEINTR
            {WSAError.WSAEINTR,           LinuxError.EINTR},
            // WSAEWOULDBLOCK
            {WSAError.WSAEWOULDBLOCK,     LinuxError.EWOULDBLOCK},
            // WSAEINPROGRESS
            {WSAError.WSAEINPROGRESS,     LinuxError.EINPROGRESS},
            // WSAEALREADY
            {WSAError.WSAEALREADY,        LinuxError.EALREADY},
            // WSAENOTSOCK
            {WSAError.WSAENOTSOCK,        LinuxError.ENOTSOCK},
            // WSAEDESTADDRREQ
            {WSAError.WSAEDESTADDRREQ,    LinuxError.EDESTADDRREQ},
            // WSAEMSGSIZE
            {WSAError.WSAEMSGSIZE,        LinuxError.EMSGSIZE},
            // WSAEPROTOTYPE
            {WSAError.WSAEPROTOTYPE,      LinuxError.EPROTOTYPE},
            // WSAENOPROTOOPT
            {WSAError.WSAENOPROTOOPT,     LinuxError.ENOPROTOOPT},
            // WSAEPROTONOSUPPORT
            {WSAError.WSAEPROTONOSUPPORT, LinuxError.EPROTONOSUPPORT},
            // WSAESOCKTNOSUPPORT
            {WSAError.WSAESOCKTNOSUPPORT, LinuxError.ESOCKTNOSUPPORT},
            // WSAEOPNOTSUPP
            {WSAError.WSAEOPNOTSUPP,      LinuxError.EOPNOTSUPP},
            // WSAEPFNOSUPPORT
            {WSAError.WSAEPFNOSUPPORT,    LinuxError.EPFNOSUPPORT},
            // WSAEAFNOSUPPORT
            {WSAError.WSAEAFNOSUPPORT,    LinuxError.EAFNOSUPPORT},
            // WSAEADDRINUSE
            {WSAError.WSAEADDRINUSE,      LinuxError.EADDRINUSE},
            // WSAEADDRNOTAVAIL
            {WSAError.WSAEADDRNOTAVAIL,   LinuxError.EADDRNOTAVAIL},
            // WSAENETDOWN
            {WSAError.WSAENETDOWN,        LinuxError.ENETDOWN},
            // WSAENETUNREACH
            {WSAError.WSAENETUNREACH,     LinuxError.ENETUNREACH},
            // WSAENETRESET
            {WSAError.WSAENETRESET,       LinuxError.ENETRESET},
            // WSAECONNABORTED
            {WSAError.WSAECONNABORTED,    LinuxError.ECONNABORTED},
            // WSAECONNRESET
            {WSAError.WSAECONNRESET,      LinuxError.ECONNRESET},
            // WSAENOBUFS
            {WSAError.WSAENOBUFS,         LinuxError.ENOBUFS},
            // WSAEISCONN
            {WSAError.WSAEISCONN,         LinuxError.EISCONN},
            // WSAENOTCONN
            {WSAError.WSAENOTCONN,        LinuxError.ENOTCONN},
            // WSAESHUTDOWN
            {WSAError.WSAESHUTDOWN,       LinuxError.ESHUTDOWN},
            // WSAETOOMANYREFS
            {WSAError.WSAETOOMANYREFS,    LinuxError.ETOOMANYREFS},
            // WSAETIMEDOUT
            {WSAError.WSAETIMEDOUT,       LinuxError.ETIMEDOUT},
            // WSAECONNREFUSED
            {WSAError.WSAECONNREFUSED,    LinuxError.ECONNREFUSED},
            // WSAELOOP
            {WSAError.WSAELOOP,           LinuxError.ELOOP},
            // WSAENAMETOOLONG
            {WSAError.WSAENAMETOOLONG,    LinuxError.ENAMETOOLONG},
            // WSAEHOSTDOWN
            {WSAError.WSAEHOSTDOWN,       LinuxError.EHOSTDOWN},
            // WSAEHOSTUNREACH
            {WSAError.WSAEHOSTUNREACH,    LinuxError.EHOSTUNREACH},
            // WSAENOTEMPTY
            {WSAError.WSAENOTEMPTY,       LinuxError.ENOTEMPTY},
            // WSAEUSERS
            {WSAError.WSAEUSERS,          LinuxError.EUSERS},
            // WSAEDQUOT
            {WSAError.WSAEDQUOT,          LinuxError.EDQUOT},
            // WSAESTALE
            {WSAError.WSAESTALE,          LinuxError.ESTALE},
            // WSAEREMOTE
            {WSAError.WSAEREMOTE,         LinuxError.EREMOTE},
            // WSAEINVAL
            {WSAError.WSAEINVAL,          LinuxError.EINVAL},
            // WSAEFAULT
            {WSAError.WSAEFAULT,          LinuxError.EFAULT},
            // NOERROR
            {0, 0}
        };

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private bool IsPrivileged;

        private List<BsdSocket> Sockets = new List<BsdSocket>();

        public IClient(bool IsPrivileged)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  RegisterClient     },
                { 1,  StartMonitoring    },
                { 2,  Socket             },
                { 3,  SocketExempt       },
                { 4,  Open               },
                { 5,  Select             },
                { 6,  Poll               },
                { 7,  Sysctl             },
                { 8,  Recv               },
                { 9,  RecvFrom           },
                { 10, Send               },
                { 11, SendTo             },
                { 12, Accept             },
                { 13, Bind               },
                { 14, Connect            },
                { 15, GetPeerName        },
                { 16, GetSockName        },
                { 17, GetSockOpt         },
                { 18, Listen             },
                { 19, Ioctl              },
                { 20, Fcntl              },
                { 21, SetSockOpt         },
                { 22, Shutdown           },
                { 23, ShutdownAllSockets },
                { 24, Write              },
                { 25, Read               },
                { 26, Close              },
                { 27, DuplicateSocket    },
            };

            this.IsPrivileged = IsPrivileged;
        }

        private LinuxError ConvertError(WSAError ErrorCode)
        {
            LinuxError Errno;

            if (!ErrorMap.TryGetValue(ErrorCode, out Errno))
            {
                Errno = (LinuxError)ErrorCode;
            }

            return Errno;
        }

        private long WriteWinSock2Error(ServiceCtx Context, WSAError ErrorCode)
        {
            return WriteBsdResult(Context, -1, ConvertError(ErrorCode));
        }

        private long WriteBsdResult(ServiceCtx Context, int Result, LinuxError ErrorCode = 0)
        {
            if (ErrorCode != LinuxError.SUCCESS)
            {
                Result = -1;
            }

            Context.ResponseData.Write(Result);
            Context.ResponseData.Write((int)ErrorCode);

            return 0;
        }

        private BsdSocket RetrieveSocket(int SocketFd)
        {
            if (SocketFd >= 0 && Sockets.Count > SocketFd)
            {
                return Sockets[SocketFd];
            }

            return null;
        }

        private LinuxError SetResultErrno(Socket Socket, int Result)
        {
            return Result == 0 && !Socket.Blocking ? LinuxError.EWOULDBLOCK : LinuxError.SUCCESS;
        }

        private AddressFamily ConvertFromBsd(int Domain)
        {
            if (Domain == 2)
            {
                return AddressFamily.InterNetwork;
            }

            // FIXME: AF_ROUTE ignored, is that really needed?
            return AddressFamily.Unknown;
        }

        private long SocketInternal(ServiceCtx Context, bool Exempt)
        {
            AddressFamily Domain   = (AddressFamily)Context.RequestData.ReadInt32();
            SocketType    Type     = (SocketType)Context.RequestData.ReadInt32();
            ProtocolType  Protocol = (ProtocolType)Context.RequestData.ReadInt32();

            if (Domain == AddressFamily.Unknown)
            {
                return WriteBsdResult(Context, -1, LinuxError.EPROTONOSUPPORT);
            }
            else if ((Type == SocketType.Seqpacket || Type == SocketType.Raw) && !IsPrivileged)
            {
                if (Domain != AddressFamily.InterNetwork || Type != SocketType.Raw || Protocol != ProtocolType.Icmp)
                {
                    return WriteBsdResult(Context, -1, LinuxError.ENOENT);
                }
            }

            BsdSocket NewBsdSocket = new BsdSocket
            {
                Family   = (int)Domain,
                Type     = (int)Type,
                Protocol = (int)Protocol,
                Handle   = new Socket(Domain, Type, Protocol)
            };

            Sockets.Add(NewBsdSocket);

            if (Exempt)
            {
                NewBsdSocket.Handle.Disconnect(true);
            }

            return WriteBsdResult(Context, Sockets.Count - 1);
        }

        private IPEndPoint ParseSockAddr(ServiceCtx Context, long BufferPosition, long BufferSize)
        {
            int Size   = Context.Memory.ReadByte(BufferPosition);
            int Family = Context.Memory.ReadByte(BufferPosition + 1);
            int Port   = EndianSwap.Swap16(Context.Memory.ReadUInt16(BufferPosition + 2));

            byte[] RawIp = Context.Memory.ReadBytes(BufferPosition + 4, 4);

            return new IPEndPoint(new IPAddress(RawIp), Port);
        }

        private void WriteSockAddr(ServiceCtx Context, long BufferPosition, IPEndPoint EndPoint)
        {
            Context.Memory.WriteByte(BufferPosition, 0);
            Context.Memory.WriteByte(BufferPosition + 1, (byte)EndPoint.AddressFamily);
            Context.Memory.WriteUInt16(BufferPosition + 2, EndianSwap.Swap16((ushort)EndPoint.Port));
            Context.Memory.WriteBytes(BufferPosition + 4, EndPoint.Address.GetAddressBytes());
        }

        private void WriteSockAddr(ServiceCtx Context, long BufferPosition, BsdSocket Socket, bool IsRemote)
        {
            IPEndPoint EndPoint = (IsRemote ? Socket.Handle.RemoteEndPoint : Socket.Handle.LocalEndPoint) as IPEndPoint;

            WriteSockAddr(Context, BufferPosition, EndPoint);
        }

        // Initialize(nn::socket::BsdBufferConfig config, u64 pid, u64 transferMemorySize, KObject<copy, transfer_memory>, pid) -> u32 bsd_errno
        public long RegisterClient(ServiceCtx Context)
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
            Context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceBsd, "Stubbed.");

            return 0;
        }

        // StartMonitoring(u64, pid)
        public long StartMonitoring(ServiceCtx Context)
        {
            ulong Unknown0 = Context.RequestData.ReadUInt64();

            Logger.PrintStub(LogClass.ServiceBsd, $"Stubbed. Unknown0: {Unknown0}");

            return 0;
        }

        // Socket(u32 domain, u32 type, u32 protocol) -> (i32 ret, u32 bsd_errno)
        public long Socket(ServiceCtx Context)
        {
            return SocketInternal(Context, false);
        }

        // SocketExempt(u32 domain, u32 type, u32 protocol) -> (i32 ret, u32 bsd_errno)
        public long SocketExempt(ServiceCtx Context)
        {
            return SocketInternal(Context, true);
        }

        // Open(u32 flags, array<unknown, 0x21> path) -> (i32 ret, u32 bsd_errno)
        public long Open(ServiceCtx Context)
        {
            (long BufferPosition, long BufferSize) = Context.Request.GetBufferType0x21();

            int Flags = Context.RequestData.ReadInt32();

            byte[] RawPath = Context.Memory.ReadBytes(BufferPosition, BufferSize);
            string Path    = Encoding.ASCII.GetString(RawPath);

            WriteBsdResult(Context, -1, LinuxError.EOPNOTSUPP);

            Logger.PrintStub(LogClass.ServiceBsd, $"Stubbed. Path: {Path} - " +
                                                  $"Flags: {Flags}");

            return 0;
        }

        // Select(u32 nfds, nn::socket::timeout timeout, buffer<nn::socket::fd_set, 0x21, 0> readfds_in, buffer<nn::socket::fd_set, 0x21, 0> writefds_in, buffer<nn::socket::fd_set, 0x21, 0> errorfds_in) -> (i32 ret, u32 bsd_errno, buffer<nn::socket::fd_set, 0x22, 0> readfds_out, buffer<nn::socket::fd_set, 0x22, 0> writefds_out, buffer<nn::socket::fd_set, 0x22, 0> errorfds_out)
        public long Select(ServiceCtx Context)
        {
            WriteBsdResult(Context, -1, LinuxError.EOPNOTSUPP);

            Logger.PrintStub(LogClass.ServiceBsd, $"Stubbed.");

            return 0;
        }

        // Poll(u32 nfds, u32 timeout, buffer<unknown, 0x21, 0> fds) -> (i32 ret, u32 bsd_errno, buffer<unknown, 0x22, 0>)
        public long Poll(ServiceCtx Context)
        {
            int FdsCount = Context.RequestData.ReadInt32();
            int Timeout  = Context.RequestData.ReadInt32();

            (long BufferPosition, long BufferSize) = Context.Request.GetBufferType0x21();


            if (Timeout < -1 || FdsCount < 0 || (FdsCount * 8) > BufferSize)
            {
                return WriteBsdResult(Context, -1, LinuxError.EINVAL);
            }

            PollEvent[] Events = new PollEvent[FdsCount];

            for (int i = 0; i < FdsCount; i++)
            {
                int SocketFd = Context.Memory.ReadInt32(BufferPosition + i * 8);

                BsdSocket Socket = RetrieveSocket(SocketFd);

                if (Socket == null)
                {
                    return WriteBsdResult(Context, -1, LinuxError.EBADF);
                }

                PollEvent.EventTypeMask InputEvents  = (PollEvent.EventTypeMask)Context.Memory.ReadInt16(BufferPosition + i * 8 + 4);
                PollEvent.EventTypeMask OutputEvents = (PollEvent.EventTypeMask)Context.Memory.ReadInt16(BufferPosition + i * 8 + 6);

                Events[i] = new PollEvent(SocketFd, Socket, InputEvents, OutputEvents);
            }

            List<Socket> ReadEvents  = new List<Socket>();
            List<Socket> WriteEvents = new List<Socket>();
            List<Socket> ErrorEvents = new List<Socket>();

            foreach (PollEvent Event in Events)
            {
                bool IsValidEvent = false;

                if ((Event.InputEvents & PollEvent.EventTypeMask.Input) != 0)
                {
                    ReadEvents.Add(Event.Socket.Handle);
                    ErrorEvents.Add(Event.Socket.Handle);

                    IsValidEvent = true;
                }

                if ((Event.InputEvents & PollEvent.EventTypeMask.UrgentInput) != 0)
                {
                    ReadEvents.Add(Event.Socket.Handle);
                    ErrorEvents.Add(Event.Socket.Handle);

                    IsValidEvent = true;
                }

                if ((Event.InputEvents & PollEvent.EventTypeMask.Output) != 0)
                {
                    WriteEvents.Add(Event.Socket.Handle);
                    ErrorEvents.Add(Event.Socket.Handle);

                    IsValidEvent = true;
                }

                if ((Event.InputEvents & PollEvent.EventTypeMask.Error) != 0)
                {
                    ErrorEvents.Add(Event.Socket.Handle);
                    IsValidEvent = true;
                }

                if (!IsValidEvent)
                {
                    Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported Poll input event type: {Event.InputEvents}");
                    return WriteBsdResult(Context, -1, LinuxError.EINVAL);
                }
            }

            try
            {
                System.Net.Sockets.Socket.Select(ReadEvents, WriteEvents, ErrorEvents, Timeout);
            }
            catch (SocketException Exception)
            {
                return WriteWinSock2Error(Context, (WSAError)Exception.ErrorCode);
            }

            for (int i = 0; i < FdsCount; i++)
            {
                PollEvent Event = Events[i];
                Context.Memory.WriteInt32(BufferPosition + i * 8, Event.SocketFd);
                Context.Memory.WriteInt16(BufferPosition + i * 8 + 4, (short)Event.InputEvents);

                PollEvent.EventTypeMask OutputEvents = 0;

                Socket Socket = Event.Socket.Handle;

                if (ErrorEvents.Contains(Socket))
                {
                    OutputEvents |= PollEvent.EventTypeMask.Error;

                    if (!Socket.Connected || !Socket.IsBound)
                    {
                        OutputEvents |= PollEvent.EventTypeMask.Disconnected;
                    }
                }

                if (ReadEvents.Contains(Socket))
                {
                    if ((Event.InputEvents & PollEvent.EventTypeMask.Input) != 0)
                    {
                        OutputEvents |= PollEvent.EventTypeMask.Input;
                    }
                }

                if (WriteEvents.Contains(Socket))
                {
                    OutputEvents |= PollEvent.EventTypeMask.Output;
                }

                Context.Memory.WriteInt16(BufferPosition + i * 8 + 6, (short)OutputEvents);
            }

            return WriteBsdResult(Context, ReadEvents.Count + WriteEvents.Count + ErrorEvents.Count, LinuxError.SUCCESS);
        }

        // Sysctl(buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>) -> (i32 ret, u32 bsd_errno, u32, buffer<unknown, 0x22, 0>)
        public long Sysctl(ServiceCtx Context)
        {
            WriteBsdResult(Context, -1, LinuxError.EOPNOTSUPP);

            Logger.PrintStub(LogClass.ServiceBsd, $"Stubbed.");

            return 0;
        }

        // Recv(u32 socket, u32 flags) -> (i32 ret, u32 bsd_errno, array<i8, 0x22> message)
        public long Recv(ServiceCtx Context)
        {
            int         SocketFd    = Context.RequestData.ReadInt32();
            SocketFlags SocketFlags = (SocketFlags)Context.RequestData.ReadInt32();

            (long ReceivePosition, long ReceiveLength) = Context.Request.GetBufferType0x22();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);
            int        Result = -1;

            if (Socket != null)
            {
                if (SocketFlags != SocketFlags.None && (SocketFlags & SocketFlags.OutOfBand) == 0
                    && (SocketFlags & SocketFlags.Peek) == 0)
                {
                    Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported Recv flags: {SocketFlags}");
                    return WriteBsdResult(Context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[] ReceivedBuffer = new byte[ReceiveLength];

                try
                {
                    Result = Socket.Handle.Receive(ReceivedBuffer, SocketFlags);
                    Errno  = SetResultErrno(Socket.Handle, Result);

                    Context.Memory.WriteBytes(ReceivePosition, ReceivedBuffer);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }
            }

            return WriteBsdResult(Context, Result, Errno);
        }

        // RecvFrom(u32 sock, u32 flags) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<i8, 0x22, 0> message, buffer<nn::socket::sockaddr_in, 0x22, 0x10>)
        public long RecvFrom(ServiceCtx Context)
        {
            int         SocketFd    = Context.RequestData.ReadInt32();
            SocketFlags SocketFlags = (SocketFlags)Context.RequestData.ReadInt32();

            (long ReceivePosition,     long ReceiveLength)   = Context.Request.GetBufferType0x22();
            (long SockAddrInPosition,  long SockAddrInSize)  = Context.Request.GetBufferType0x21();
            (long SockAddrOutPosition, long SockAddrOutSize) = Context.Request.GetBufferType0x22(1);

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);
            int        Result = -1;

            if (Socket != null)
            {
                if (SocketFlags != SocketFlags.None && (SocketFlags & SocketFlags.OutOfBand) == 0
                    && (SocketFlags & SocketFlags.Peek) == 0)
                {
                    Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported Recv flags: {SocketFlags}");

                    return WriteBsdResult(Context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[]   ReceivedBuffer = new byte[ReceiveLength];
                EndPoint EndPoint       = ParseSockAddr(Context, SockAddrInPosition, SockAddrInSize);

                try
                {
                    Result = Socket.Handle.ReceiveFrom(ReceivedBuffer, ReceivedBuffer.Length, SocketFlags, ref EndPoint);
                    Errno  = SetResultErrno(Socket.Handle, Result);

                    Context.Memory.WriteBytes(ReceivePosition, ReceivedBuffer);
                    WriteSockAddr(Context, SockAddrOutPosition, (IPEndPoint)EndPoint);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }
            }

            return WriteBsdResult(Context, Result, Errno);
        }

        // Send(u32 socket, u32 flags, buffer<i8, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long Send(ServiceCtx Context)
        {
            int         SocketFd    = Context.RequestData.ReadInt32();
            SocketFlags SocketFlags = (SocketFlags)Context.RequestData.ReadInt32();

            (long SendPosition, long SendSize) = Context.Request.GetBufferType0x21();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);
            int        Result = -1;

            if (Socket != null)
            {
                if (SocketFlags != SocketFlags.None && SocketFlags != SocketFlags.OutOfBand
                    && SocketFlags != SocketFlags.Peek && SocketFlags != SocketFlags.DontRoute)
                {
                    Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported Send flags: {SocketFlags}");

                    return WriteBsdResult(Context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[] SendBuffer = Context.Memory.ReadBytes(SendPosition, SendSize);

                try
                {
                    Result = Socket.Handle.Send(SendBuffer, SocketFlags);
                    Errno  = SetResultErrno(Socket.Handle, Result);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }

            }

            return WriteBsdResult(Context, Result, Errno);
        }

        // SendTo(u32 socket, u32 flags, buffer<i8, 0x21, 0>, buffer<nn::socket::sockaddr_in, 0x21, 0x10>) -> (i32 ret, u32 bsd_errno)
        public long SendTo(ServiceCtx Context)
        {
            int         SocketFd    = Context.RequestData.ReadInt32();
            SocketFlags SocketFlags = (SocketFlags)Context.RequestData.ReadInt32();

            (long SendPosition,   long SendSize)   = Context.Request.GetBufferType0x21();
            (long BufferPosition, long BufferSize) = Context.Request.GetBufferType0x21(1);

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);
            int        Result = -1;

            if (Socket != null)
            {
                if (SocketFlags != SocketFlags.None && SocketFlags != SocketFlags.OutOfBand
                    && SocketFlags != SocketFlags.Peek && SocketFlags != SocketFlags.DontRoute)
                {
                    Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported Send flags: {SocketFlags}");

                    return WriteBsdResult(Context, -1, LinuxError.EOPNOTSUPP);
                }

                byte[]   SendBuffer = Context.Memory.ReadBytes(SendPosition, SendSize);
                EndPoint EndPoint   = ParseSockAddr(Context, BufferPosition, BufferSize);

                try
                {
                    Result = Socket.Handle.SendTo(SendBuffer, SendBuffer.Length, SocketFlags, EndPoint);
                    Errno  = SetResultErrno(Socket.Handle, Result);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }

            }

            return WriteBsdResult(Context, Result, Errno);
        }

        // Accept(u32 socket) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<nn::socket::sockaddr_in, 0x22, 0x10> addr)
        public long Accept(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            (long BufferPos, long BufferSize) = Context.Request.GetBufferType0x22();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.SUCCESS;

                Socket NewSocket = null;

                try
                {
                    NewSocket = Socket.Handle.Accept();
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }

                if (NewSocket == null && Errno == LinuxError.SUCCESS)
                {
                    Errno = LinuxError.EWOULDBLOCK;
                }
                else if (Errno == LinuxError.SUCCESS)
                {
                    BsdSocket NewBsdSocket = new BsdSocket
                    {
                        Family   = (int)NewSocket.AddressFamily,
                        Type     = (int)NewSocket.SocketType,
                        Protocol = (int)NewSocket.ProtocolType,
                        Handle   = NewSocket,
                    };

                    Sockets.Add(NewBsdSocket);

                    WriteSockAddr(Context, BufferPos, NewBsdSocket, true);

                    WriteBsdResult(Context, Sockets.Count - 1, Errno);

                    Context.ResponseData.Write(0x10);

                    return 0;
                }
            }

            return WriteBsdResult(Context, -1, Errno);
        }

        // Bind(u32 socket, buffer<nn::socket::sockaddr_in, 0x21, 0x10> addr) -> (i32 ret, u32 bsd_errno)
        public long Bind(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            (long BufferPos, long BufferSize) = Context.Request.GetBufferType0x21();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.SUCCESS;

                try
                {
                    IPEndPoint EndPoint = ParseSockAddr(Context, BufferPos, BufferSize);

                    Socket.Handle.Bind(EndPoint);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // Connect(u32 socket, buffer<nn::socket::sockaddr_in, 0x21, 0x10>) -> (i32 ret, u32 bsd_errno)
        public long Connect(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            (long BufferPos, long BufferSize) = Context.Request.GetBufferType0x21();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.SUCCESS;
                try
                {
                    IPEndPoint EndPoint = ParseSockAddr(Context, BufferPos, BufferSize);

                    Socket.Handle.Connect(EndPoint);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // GetPeerName(u32 socket) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<nn::socket::sockaddr_in, 0x22, 0x10> addr)
        public long GetPeerName(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            (long BufferPos, long BufferSize) = Context.Request.GetBufferType0x22();

            LinuxError  Errno  = LinuxError.EBADF;
            BsdSocket Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.SUCCESS;

                WriteSockAddr(Context, BufferPos, Socket, true);
                WriteBsdResult(Context, 0, Errno);
                Context.ResponseData.Write(0x10);
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // GetSockName(u32 socket) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<nn::socket::sockaddr_in, 0x22, 0x10> addr)
        public long GetSockName(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            (long BufferPos, long BufferSize) = Context.Request.GetBufferType0x22();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.SUCCESS;

                WriteSockAddr(Context, BufferPos, Socket, false);
                WriteBsdResult(Context, 0, Errno);
                Context.ResponseData.Write(0x10);
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // GetSockOpt(u32 socket, u32 level, u32 option_name) -> (i32 ret, u32 bsd_errno, u32, buffer<unknown, 0x22, 0>)
        public long GetSockOpt(ServiceCtx Context)
        {
            int SocketFd   = Context.RequestData.ReadInt32();
            int Level      = Context.RequestData.ReadInt32();
            int OptionName = Context.RequestData.ReadInt32();

            (long BufferPosition, long BufferSize) = Context.Request.GetBufferType0x22();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.ENOPROTOOPT;

                if (Level == 0xFFFF)
                {
                    Errno = HandleGetSocketOption(Context, Socket, (SocketOptionName)OptionName, BufferPosition, BufferSize);
                }
                else
                {
                    Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported GetSockOpt Level: {(SocketOptionLevel)Level}");
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // Listen(u32 socket, u32 backlog) -> (i32 ret, u32 bsd_errno)
        public long Listen(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();
            int Backlog  = Context.RequestData.ReadInt32();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.SUCCESS;

                try
                {
                    Socket.Handle.Listen(Backlog);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // Ioctl(u32 fd, u32 request, u32 bufcount, buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>, buffer<unknown, 0x21, 0>) -> (i32 ret, u32 bsd_errno, buffer<unknown, 0x22, 0>, buffer<unknown, 0x22, 0>, buffer<unknown, 0x22, 0>, buffer<unknown, 0x22, 0>)
        public long Ioctl(ServiceCtx Context)
        {
            int      SocketFd    = Context.RequestData.ReadInt32();
            BsdIoctl Cmd         = (BsdIoctl)Context.RequestData.ReadInt32();
            int      BufferCount = Context.RequestData.ReadInt32();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                switch (Cmd)
                {
                    case BsdIoctl.AtMark:
                        Errno = LinuxError.SUCCESS;

                        (long BufferPosition, long BufferSize) = Context.Request.GetBufferType0x22();

                        // FIXME: OOB not implemented.
                        Context.Memory.WriteInt32(BufferPosition, 0);
                        break;

                    default:
                        Errno = LinuxError.EOPNOTSUPP;

                        Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported Ioctl Cmd: {Cmd}");
                        break;
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // Fcntl(u32 socket, u32 cmd, u32 arg) -> (i32 ret, u32 bsd_errno)
        public long Fcntl(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();
            int Cmd      = Context.RequestData.ReadInt32();
            int Arg      = Context.RequestData.ReadInt32();

            int        Result = 0;
            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.SUCCESS;

                if (Cmd == 0x3)
                {
                    Result = !Socket.Handle.Blocking ? 0x800 : 0;
                }
                else if (Cmd == 0x4 && Arg == 0x800)
                {
                    Socket.Handle.Blocking = false;
                    Result = 0;
                }
                else
                {
                    Errno = LinuxError.EOPNOTSUPP;
                }
            }

            return WriteBsdResult(Context, Result, Errno);
        }

        private LinuxError HandleGetSocketOption(ServiceCtx Context, BsdSocket Socket, SocketOptionName OptionName, long OptionValuePosition, long OptionValueSize)
        {
            try
            {
                byte[] OptionValue = new byte[OptionValueSize];

                switch (OptionName)
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
                        Socket.Handle.GetSocketOption(SocketOptionLevel.Socket, OptionName, OptionValue);
                        Context.Memory.WriteBytes(OptionValuePosition, OptionValue);

                        return LinuxError.SUCCESS;

                    case (SocketOptionName)0x200:
                        Socket.Handle.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, OptionValue);
                        Context.Memory.WriteBytes(OptionValuePosition, OptionValue);

                        return LinuxError.SUCCESS;

                    default:
                        Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported SetSockOpt OptionName: {OptionName}");

                        return LinuxError.EOPNOTSUPP;
                }
            }
            catch (SocketException Exception)
            {
                return ConvertError((WSAError)Exception.ErrorCode);
            }
        }

        private LinuxError HandleSetSocketOption(ServiceCtx Context, BsdSocket Socket, SocketOptionName OptionName, long OptionValuePosition, long OptionValueSize)
        {
            try
            {
                switch (OptionName)
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
                        Socket.Handle.SetSocketOption(SocketOptionLevel.Socket, OptionName, Context.Memory.ReadInt32(OptionValuePosition));

                        return LinuxError.SUCCESS;

                    case (SocketOptionName)0x200:
                        Socket.Handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Context.Memory.ReadInt32(OptionValuePosition));

                        return LinuxError.SUCCESS;

                    case SocketOptionName.Linger:
                        Socket.Handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger,
                            new LingerOption(Context.Memory.ReadInt32(OptionValuePosition) != 0, Context.Memory.ReadInt32(OptionValuePosition + 4)));

                        return LinuxError.SUCCESS;

                    default:
                        Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported SetSockOpt OptionName: {OptionName}");

                        return LinuxError.EOPNOTSUPP;
                }
            }
            catch (SocketException Exception)
            {
                return ConvertError((WSAError)Exception.ErrorCode);
            }
        }

        // SetSockOpt(u32 socket, u32 level, u32 option_name, buffer<unknown, 0x21, 0> option_value) -> (i32 ret, u32 bsd_errno)
        public long SetSockOpt(ServiceCtx Context)
        {
            int SocketFd   = Context.RequestData.ReadInt32();
            int Level      = Context.RequestData.ReadInt32();
            int OptionName = Context.RequestData.ReadInt32();

            (long BufferPos, long BufferSize) = Context.Request.GetBufferType0x21();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.ENOPROTOOPT;

                if (Level == 0xFFFF)
                {
                    Errno = HandleSetSocketOption(Context, Socket, (SocketOptionName)OptionName, BufferPos, BufferSize);
                }
                else
                {
                    Logger.PrintWarning(LogClass.ServiceBsd, $"Unsupported SetSockOpt Level: {(SocketOptionLevel)Level}");
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // Shutdown(u32 socket, u32 how) -> (i32 ret, u32 bsd_errno)
        public long Shutdown(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();
            int How      = Context.RequestData.ReadInt32();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Errno = LinuxError.EINVAL;

                if (How >= 0 && How <= 2)
                {
                    Errno = LinuxError.SUCCESS;

                    try
                    {
                        Socket.Handle.Shutdown((SocketShutdown)How);
                    }
                    catch (SocketException Exception)
                    {
                        Errno = ConvertError((WSAError)Exception.ErrorCode);
                    }
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // ShutdownAllSockets(u32 how) -> (i32 ret, u32 bsd_errno)
        public long ShutdownAllSockets(ServiceCtx Context)
        {
            int How = Context.RequestData.ReadInt32();

            LinuxError Errno = LinuxError.EINVAL;

            if (How >= 0 && How <= 2)
            {
                Errno = LinuxError.SUCCESS;

                foreach (BsdSocket Socket in Sockets)
                {
                    if (Socket != null)
                    {
                        try
                        {
                            Socket.Handle.Shutdown((SocketShutdown)How);
                        }
                        catch (SocketException Exception)
                        {
                            Errno = ConvertError((WSAError)Exception.ErrorCode);
                            break;
                        }
                    }
                }
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // Write(u32 socket, buffer<i8, 0x21, 0> message) -> (i32 ret, u32 bsd_errno)
        public long Write(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            (long SendPosition, long SendSize) = Context.Request.GetBufferType0x21();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);
            int        Result = -1;

            if (Socket != null)
            {
                byte[] SendBuffer = Context.Memory.ReadBytes(SendPosition, SendSize);

                try
                {
                    Result = Socket.Handle.Send(SendBuffer);
                    Errno  = SetResultErrno(Socket.Handle, Result);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }
            }

            return WriteBsdResult(Context, Result, Errno);
        }

        // Read(u32 socket) -> (i32 ret, u32 bsd_errno, buffer<i8, 0x22, 0> message)
        public long Read(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            (long ReceivePosition, long ReceiveLength) = Context.Request.GetBufferType0x22();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);
            int        Result = -1;

            if (Socket != null)
            {
                byte[] ReceivedBuffer = new byte[ReceiveLength];

                try
                {
                    Result = Socket.Handle.Receive(ReceivedBuffer);
                    Errno  = SetResultErrno(Socket.Handle, Result);
                }
                catch (SocketException Exception)
                {
                    Errno = ConvertError((WSAError)Exception.ErrorCode);
                }
            }

            return WriteBsdResult(Context, Result, Errno);
        }

        // Close(u32 socket) -> (i32 ret, u32 bsd_errno)
        public long Close(ServiceCtx Context)
        {
            int SocketFd = Context.RequestData.ReadInt32();

            LinuxError Errno  = LinuxError.EBADF;
            BsdSocket  Socket = RetrieveSocket(SocketFd);

            if (Socket != null)
            {
                Socket.Handle.Close();

                Sockets[SocketFd] = null;

                Errno = LinuxError.SUCCESS;
            }

            return WriteBsdResult(Context, 0, Errno);
        }

        // DuplicateSocket(u32 socket, u64 reserved) -> (i32 ret, u32 bsd_errno)
        public long DuplicateSocket(ServiceCtx Context)
        {
            int   SocketFd = Context.RequestData.ReadInt32();
            ulong Reserved = Context.RequestData.ReadUInt64();

            LinuxError Errno     = LinuxError.ENOENT;
            int        NewSockFd = -1;

            if (IsPrivileged)
            {
                Errno = LinuxError.EBADF;

                BsdSocket OldSocket = RetrieveSocket(SocketFd);

                if (OldSocket != null)
                {
                    Sockets.Add(OldSocket);
                    NewSockFd = Sockets.Count - 1;
                }
            }

            return WriteBsdResult(Context, NewSockFd, Errno);
        }
    }
}
