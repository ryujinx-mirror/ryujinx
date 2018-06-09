using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ryujinx.Core.OsHle.Services.Bsd
{
    class IClient : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private List<BsdSocket> Sockets = new List<BsdSocket>();

        public IClient()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  Initialize      },
                { 1,  StartMonitoring },
                { 2,  Socket          },
                { 6,  Poll            },
                { 8,  Recv            },
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
            BsdSocket NewBsdSocket = new BsdSocket
            {
                Family   = Context.RequestData.ReadInt32(),
                Type     = Context.RequestData.ReadInt32(),
                Protocol = Context.RequestData.ReadInt32()
            };

            Sockets.Add(NewBsdSocket);

            NewBsdSocket.Handle = new Socket((AddressFamily)NewBsdSocket.Family,
                                                (SocketType)NewBsdSocket.Type,
                                              (ProtocolType)NewBsdSocket.Protocol);

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

            byte[] SentBuffer = Context.Memory.ReadBytes(Context.Request.SendBuff[0].Position,
                                                         Context.Request.SendBuff[0].Size);

            int SocketId        = Get32(SentBuffer, 0);
            int RequestedEvents = Get16(SentBuffer, 4);
            int ReturnedEvents  = Get16(SentBuffer, 6);

            //Todo: Stub - Need to implemented the Type-22 buffer.

            Context.ResponseData.Write(1);
            Context.ResponseData.Write(0);

            return 0;
        }

        //(u32 socket, u32 flags) -> (i32 ret, u32 bsd_errno, buffer<i8, 0x22, 0> message)
        public long Recv(ServiceCtx Context)
        {
            int SocketId    = Context.RequestData.ReadInt32();
            int SocketFlags = Context.RequestData.ReadInt32();

            byte[] ReceivedBuffer = new byte[Context.Request.ReceiveBuff[0].Size];

            try
            {
                int BytesRead = Sockets[SocketId].Handle.Receive(ReceivedBuffer);

                //Logging.Debug("Received Buffer:" + Environment.NewLine + Logging.HexDump(ReceivedBuffer));

                AMemoryHelper.WriteBytes(Context.Memory, Context.Request.ReceiveBuff[0].Position, ReceivedBuffer);

                Context.ResponseData.Write(BytesRead);
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
            int SocketId    = Context.RequestData.ReadInt32();
            int SocketFlags = Context.RequestData.ReadInt32();

            byte[] SentBuffer = Context.Memory.ReadBytes(Context.Request.SendBuff[0].Position,
                                                         Context.Request.SendBuff[0].Size);

            try
            {
                //Logging.Debug("Sent Buffer:" + Environment.NewLine + Logging.HexDump(SentBuffer));

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
            int SocketId    = Context.RequestData.ReadInt32();
            int SocketFlags = Context.RequestData.ReadInt32();

            byte[] SentBuffer = Context.Memory.ReadBytes(Context.Request.SendBuff[0].Position,
                                                         Context.Request.SendBuff[0].Size);

            byte[] AddressBuffer = Context.Memory.ReadBytes(Context.Request.SendBuff[1].Position,
                                                            Context.Request.SendBuff[1].Size);

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
                //Logging.Debug("Sent Buffer:" + Environment.NewLine + Logging.HexDump(SentBuffer));

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
            int SocketId = Context.RequestData.ReadInt32();

            long AddrBufferPtr = Context.Request.ReceiveBuff[0].Position;

            Socket HandleAccept = null;

            Task TimeOut = Task.Factory.StartNew(() =>
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
                BsdSocket NewBsdSocket = new BsdSocket
                {
                    IpAddress = ((IPEndPoint)Sockets[SocketId].Handle.LocalEndPoint).Address,
                    RemoteEP  = ((IPEndPoint)Sockets[SocketId].Handle.LocalEndPoint),
                    Handle    = HandleAccept
                };

                Sockets.Add(NewBsdSocket);

                using (MemoryStream MS = new MemoryStream())
                {
                    BinaryWriter Writer = new BinaryWriter(MS);

                    Writer.Write((byte)0);

                    Writer.Write((byte)NewBsdSocket.Handle.AddressFamily);

                    Writer.Write((short)((IPEndPoint)NewBsdSocket.Handle.LocalEndPoint).Port);

                    byte[] IpAddress = NewBsdSocket.IpAddress.GetAddressBytes();

                    Writer.Write(IpAddress);

                    AMemoryHelper.WriteBytes(Context.Memory, AddrBufferPtr, MS.ToArray());

                    Context.ResponseData.Write(Sockets.Count - 1);
                    Context.ResponseData.Write(0);
                    Context.ResponseData.Write(MS.Length);
                }
            }
            else
            {
                Context.ResponseData.Write(-1);
                Context.ResponseData.Write((int)BsdError.Timeout);
            }

            return 0;
        }

        //(u32 socket, buffer<sockaddr, 0x21, 0>) -> (i32 ret, u32 bsd_errno)
        public long Bind(ServiceCtx Context)
        {
            int SocketId = Context.RequestData.ReadInt32();

            byte[] AddressBuffer = Context.Memory.ReadBytes(Context.Request.SendBuff[0].Position,
                                                            Context.Request.SendBuff[0].Size);

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

            byte[] AddressBuffer = Context.Memory.ReadBytes(Context.Request.SendBuff[0].Position,
                                                            Context.Request.SendBuff[0].Size);

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
            int SocketId = Context.RequestData.ReadInt32();

            SocketOptionLevel SocketLevel      = (SocketOptionLevel)Context.RequestData.ReadInt32();
            SocketOptionName  SocketOptionName =  (SocketOptionName)Context.RequestData.ReadInt32();

            byte[] SocketOptionValue = Context.Memory.ReadBytes(Context.Request.PtrBuff[0].Position,
                                                                Context.Request.PtrBuff[0].Size);

            int OptionValue = Get32(SocketOptionValue, 0);

            try
            {
                Sockets[SocketId].Handle.SetSocketOption(SocketLevel, SocketOptionName, OptionValue);

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

                string IpAddress = Reader.ReadByte().ToString() + "." +
                                   Reader.ReadByte().ToString() + "." +
                                   Reader.ReadByte().ToString() + "." +
                                   Reader.ReadByte().ToString();

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