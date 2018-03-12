using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ryujinx.Core.OsHle.IpcServices.Bsd
{

    //bsd_errno == (SocketException.ErrorCode - 10000)
    //https://github.com/freebsd/freebsd/blob/master/sys/sys/errno.h
    public enum BsdError
    {
        ENOTSOCK        = 38, /* Socket operation on non-socket */
        EDESTADDRREQ    = 39, /* Destination address required */
        EMSGSIZE        = 40, /* Message too long */
        EPROTOTYPE      = 41, /* Protocol wrong type for socket */
        ENOPROTOOPT     = 42, /* Protocol not available */
        EPROTONOSUPPORT = 43, /* Protocol not supported */
        ESOCKTNOSUPPORT = 44, /* Socket type not supported */
        EOPNOTSUPP      = 45, /* Operation not supported */
        EPFNOSUPPORT    = 46, /* Protocol family not supported */
        EAFNOSUPPORT    = 47, /* Address family not supported by protocol family */
        EADDRINUSE      = 48, /* Address already in use */
        EADDRNOTAVAIL   = 49, /* Can't assign requested address */
        ENETDOWN        = 50, /* Network is down */
        ENETUNREACH     = 51, /* Network is unreachable */
        ENETRESET       = 52, /* Network dropped connection on reset */
        ECONNABORTED    = 53, /* Software caused connection abort */
        ECONNRESET      = 54, /* Connection reset by peer */
        ENOBUFS         = 55, /* No buffer space available */
        EISCONN         = 56, /* Socket is already connected */
        ENOTCONN        = 57, /* Socket is not connected */
        ESHUTDOWN       = 58, /* Can't send after socket shutdown */
        ETOOMANYREFS    = 59, /* Too many references: can't splice */
        ETIMEDOUT       = 60, /* Operation timed out */
        ECONNREFUSED    = 61  /* Connection refused */
    }

    class SocketBsd
    {
        public int Family;
        public int Type;
        public int Protocol;
        public IPAddress IpAddress;
        public IPEndPoint RemoteEP;
        public Socket Handle;
    }

    class ServiceBsd : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private List<SocketBsd> Sockets = new List<SocketBsd>();

        public ServiceBsd()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  0, Initialize      },
                {  1, StartMonitoring },
                {  2, Socket          },
                {  6, Poll            },
                {  8, Recv            },
                { 10, Send            },
                { 11, SendTo          },
                { 12, Accept          },
                { 13, Bind            },
                { 14, Connect         },
                { 18, Listen          },
                { 21, SetSockOpt      },
                { 26, Close           }
            };
        }

        //(u32, u32, u32, u32, u32, u32, u32, u32, u64 pid, u64 transferMemorySize, pid, KObject) -> u32 bsd_errno
        public long Initialize(ServiceCtx Context)
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

            long Pid = Context.RequestData.ReadInt64();
            long TransferMemorySize = Context.RequestData.ReadInt64();

            // Two other args are unknown!

            Context.ResponseData.Write(0);

            //Todo: Stub

            return 0;
        }

        //(u64, pid)
        public long StartMonitoring(ServiceCtx Context)
        {
            //Todo: Stub

            return 0;
        }

        //(u32 domain, u32 type, u32 protocol) -> (i32 ret, u32 bsd_errno)
        public long Socket(ServiceCtx Context)
        {
            SocketBsd NewBSDSocket = new SocketBsd
            {
                Family   = Context.RequestData.ReadInt32(),
                Type     = Context.RequestData.ReadInt32(),
                Protocol = Context.RequestData.ReadInt32()
            };

            Sockets.Add(NewBSDSocket);

            Sockets[Sockets.Count - 1].Handle = new Socket((AddressFamily)Sockets[Sockets.Count - 1].Family,
                                                           (SocketType)Sockets[Sockets.Count - 1].Type,
                                                           (ProtocolType)Sockets[Sockets.Count - 1].Protocol);

            Context.ResponseData.Write(Sockets.Count - 1);
            Context.ResponseData.Write(0);

            return 0;
        }

        //(u32, u32, buffer<unknown, 0x21, 0>) -> (i32 ret, u32 bsd_errno, buffer<unknown, 0x22, 0>)
        public long Poll(ServiceCtx Context)
        {
            int PollCount = Context.RequestData.ReadInt32();
            int TimeOut   = Context.RequestData.ReadInt32();

            //https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/poll.h
            //https://msdn.microsoft.com/fr-fr/library/system.net.sockets.socket.poll(v=vs.110).aspx
            //https://github.com/switchbrew/libnx/blob/e0457c4534b3c37426d83e1a620f82cb28c3b528/nx/source/services/bsd.c#L343
            //https://github.com/TuxSH/ftpd/blob/switch_pr/source/ftp.c#L1634
            //https://linux.die.net/man/2/poll

            byte[] SentBuffer     = AMemoryHelper.ReadBytes(Context.Memory, 
                                                            Context.Request.SendBuff[0].Position, 
                                                            (int)Context.Request.SendBuff[0].Size);
            int SocketId          = Get32(SentBuffer, 0);
            short RequestedEvents = (short)Get16(SentBuffer, 4);
            short ReturnedEvents  = (short)Get16(SentBuffer, 6);

            //Todo: Stub - Need to implemented the Type-22 buffer.

            Context.ResponseData.Write(1);
            Context.ResponseData.Write(0);

            return 0;
        }

        //(u32 socket, u32 flags) -> (i32 ret, u32 bsd_errno, buffer<i8, 0x22, 0> message)
        public long Recv(ServiceCtx Context)
        {
            try
            {
                int SocketId          = Context.RequestData.ReadInt32();
                int SocketFlags       = Context.RequestData.ReadInt32();
                byte[] ReceivedBuffer = new byte[Context.Request.ReceiveBuff[0].Size];
                int ReadedBytes       = Sockets[SocketId].Handle.Receive(ReceivedBuffer);

                //Logging.Debug("Received Buffer:" + Environment.NewLine + Logging.HexDump(ReceivedBuffer));

                AMemoryHelper.WriteBytes(Context.Memory, Context.Request.ReceiveBuff[0].Position, ReceivedBuffer);

                Context.ResponseData.Write(ReadedBytes);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        //(u32 socket, u32 flags, buffer<i8, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long Send(ServiceCtx Context)
        {
            int SocketId      = Context.RequestData.ReadInt32();
            int SocketFlags   = Context.RequestData.ReadInt32();
            byte[] SentBuffer = AMemoryHelper.ReadBytes(Context.Memory, 
                                                        Context.Request.SendBuff[0].Position, 
                                                        (int)Context.Request.SendBuff[0].Size);

            try
            {
                //Logging.Debug("Sended Buffer:" + Environment.NewLine + Logging.HexDump(SendedBuffer));

                int BytesSent = Sockets[SocketId].Handle.Send(SentBuffer);

                Context.ResponseData.Write(BytesSent);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        //(u32 socket, u32 flags, buffer<i8, 0x21, 0>, buffer<sockaddr, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long SendTo(ServiceCtx Context)
        {
            int SocketId         = Context.RequestData.ReadInt32();
            int SocketFlags      = Context.RequestData.ReadInt32();
            byte[] SentBuffer    = AMemoryHelper.ReadBytes(Context.Memory, 
                                                           Context.Request.SendBuff[0].Position, 
                                                           (int)Context.Request.SendBuff[0].Size);
            byte[] AddressBuffer = AMemoryHelper.ReadBytes(Context.Memory, 
                                                           Context.Request.SendBuff[1].Position, 
                                                           (int)Context.Request.SendBuff[1].Size);

            if (!Sockets[SocketId].Handle.Connected)
            {
                try
                {
                    ParseAddrBuffer(SocketId, AddressBuffer);

                    Sockets[SocketId].Handle.Connect(Sockets[SocketId].RemoteEP);
                }
                catch (SocketException Ex)
                {
                    Context.ResponseData.Write(-1);
                    Context.ResponseData.Write(Ex.ErrorCode - 10000);
                }
            }

            try
            {
                //Logging.Debug("Sended Buffer:" + Environment.NewLine + Logging.HexDump(SendedBuffer));

                int BytesSent = Sockets[SocketId].Handle.Send(SentBuffer);

                Context.ResponseData.Write(BytesSent);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        //(u32 socket) -> (i32 ret, u32 bsd_errno, u32 addrlen, buffer<sockaddr, 0x22, 0> addr)
        public long Accept(ServiceCtx Context)
        {
            int SocketId       = Context.RequestData.ReadInt32();
            long AddrBufferPtr = Context.Request.ReceiveBuff[0].Position;

            Socket HandleAccept = null;

            var TimeOut = Task.Factory.StartNew(() =>
            {
                try
                {
                    HandleAccept = Sockets[SocketId].Handle.Accept();
                }
                catch (SocketException Ex)
                {
                    Context.ResponseData.Write(-1);
                    Context.ResponseData.Write(Ex.ErrorCode - 10000);
                }
            });

            TimeOut.Wait(10000);

            if (HandleAccept != null)
            {
                SocketBsd NewBSDSocket = new SocketBsd
                {
                    IpAddress = ((IPEndPoint)Sockets[SocketId].Handle.LocalEndPoint).Address,
                    RemoteEP  = ((IPEndPoint)Sockets[SocketId].Handle.LocalEndPoint),
                    Handle    = HandleAccept
                };

                Sockets.Add(NewBSDSocket);

                using (MemoryStream MS = new MemoryStream())
                {
                    BinaryWriter Writer = new BinaryWriter(MS);

                    Writer.Write((byte)0);
                    Writer.Write((byte)Sockets[Sockets.Count - 1].Handle.AddressFamily);
                    Writer.Write((Int16)((IPEndPoint)Sockets[Sockets.Count - 1].Handle.LocalEndPoint).Port);

                    string[] IpAdress = Sockets[Sockets.Count - 1].IpAddress.ToString().Split('.');
                    Writer.Write(byte.Parse(IpAdress[0]));
                    Writer.Write(byte.Parse(IpAdress[1]));
                    Writer.Write(byte.Parse(IpAdress[2]));
                    Writer.Write(byte.Parse(IpAdress[3]));

                    AMemoryHelper.WriteBytes(Context.Memory, AddrBufferPtr, MS.ToArray());

                    Context.ResponseData.Write(Sockets.Count - 1);
                    Context.ResponseData.Write(0);
                    Context.ResponseData.Write(MS.Length);
                }
            }
            else
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write((int)BsdError.ETIMEDOUT);
            }

            return 0;
        }

        //(u32 socket, buffer<sockaddr, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long Bind(ServiceCtx Context)
        {
            int SocketId = Context.RequestData.ReadInt32();

            byte[] AddressBuffer = AMemoryHelper.ReadBytes(Context.Memory, 
                                                           Context.Request.SendBuff[0].Position, 
                                                           (int)Context.Request.SendBuff[0].Size);

            try
            {
                ParseAddrBuffer(SocketId, AddressBuffer);

                Context.ResponseData.Write(0);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        //(u32 socket, buffer<sockaddr, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long Connect(ServiceCtx Context)
        {
            int SocketId = Context.RequestData.ReadInt32();

            byte[] AddressBuffer = AMemoryHelper.ReadBytes(Context.Memory, 
                                                           Context.Request.SendBuff[0].Position, 
                                                           (int)Context.Request.SendBuff[0].Size);

            try
            {
                ParseAddrBuffer(SocketId, AddressBuffer);

                Sockets[SocketId].Handle.Connect(Sockets[SocketId].RemoteEP);

                Context.ResponseData.Write(0);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        //(u32 socket, u32 backlog) -> (i32 ret, u32 bsd_errno)
        public long Listen(ServiceCtx Context)
        {
            int SocketId = Context.RequestData.ReadInt32();
            int BackLog  = Context.RequestData.ReadInt32();

            try
            {
                Sockets[SocketId].Handle.Bind(Sockets[SocketId].RemoteEP);
                Sockets[SocketId].Handle.Listen(BackLog);

                Context.ResponseData.Write(0);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        //(u32 socket, u32 level, u32 option_name, buffer<unknown, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long SetSockOpt(ServiceCtx Context)
        {
            int SocketId         = Context.RequestData.ReadInt32();
            int SocketLevel      = Context.RequestData.ReadInt32();
            int SocketOptionName = Context.RequestData.ReadInt32();

            byte[] SocketOptionValue = AMemoryHelper.ReadBytes(Context.Memory, 
                                                               Context.Request.PtrBuff[0].Position, 
                                                               Context.Request.PtrBuff[0].Size);

            try
            {
                Sockets[SocketId].Handle.SetSocketOption((SocketOptionLevel)SocketLevel, 
                                                         (SocketOptionName)SocketOptionName,
                                                         Get32(SocketOptionValue, 0));

                Context.ResponseData.Write(0);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        //(u32 socket) -> (i32 ret, u32 bsd_errno)
        public long Close(ServiceCtx Context)
        {
            int SocketId = Context.RequestData.ReadInt32();

            try
            {
                Sockets[SocketId].Handle.Close();
                Sockets[SocketId] = null;

                Context.ResponseData.Write(0);
                Context.ResponseData.Write(0);
            }
            catch (SocketException Ex)
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write(Ex.ErrorCode - 10000);
            }

            return 0;
        }

        public void ParseAddrBuffer(int SocketId, byte[] AddrBuffer)
        {
            using (MemoryStream MS = new MemoryStream(AddrBuffer))
            {
                BinaryReader Reader = new BinaryReader(MS);

                int Size   = Reader.ReadByte();
                int Family = Reader.ReadByte();
                int Port   = EndianSwap.Swap16(Reader.ReadInt16());
                string IpAddress = Reader.ReadByte().ToString() +
                                   "." + Reader.ReadByte().ToString() +
                                   "." + Reader.ReadByte().ToString() +
                                   "." + Reader.ReadByte().ToString();

                Logging.Debug($"Try to connect to {IpAddress}:{Port}");

                Sockets[SocketId].IpAddress = IPAddress.Parse(IpAddress);
                Sockets[SocketId].RemoteEP = new IPEndPoint(Sockets[SocketId].IpAddress, Port);
            }
        }

        private int Get16(byte[] Data, int Address)
        {
            return
                Data[Address + 0] << 0 |
                Data[Address + 1] << 8;
        }

        private int Get32(byte[] Data, int Address)
        {
            return
                Data[Address + 0] << 0 |
                Data[Address + 1] << 8 |
                Data[Address + 2] << 16 |
                Data[Address + 3] << 24;
        }
    }
}