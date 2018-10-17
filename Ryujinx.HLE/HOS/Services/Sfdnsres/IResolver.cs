using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Sfdnsres
{
    class IResolver : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IResolver()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  SetDnsAddressesPrivate },
                { 1,  GetDnsAddressesPrivate },
                { 2,  GetHostByName          },
                { 3,  GetHostByAddress       },
                { 4,  GetHostStringError     },
                { 5,  GetGaiStringError      },
                { 8,  RequestCancelHandle    },
                { 9,  CancelSocketCall       },
                { 11, ClearDnsAddresses      },
            };
        }

        private long SerializeHostEnt(ServiceCtx Context, IPHostEntry HostEntry, List<IPAddress> Addresses = null)
        {
            long OriginalBufferPosition = Context.Request.ReceiveBuff[0].Position;
            long BufferPosition         = OriginalBufferPosition;
            long BufferSize             = Context.Request.ReceiveBuff[0].Size;

            string HostName = HostEntry.HostName + '\0';

            // h_name
            Context.Memory.WriteBytes(BufferPosition, Encoding.ASCII.GetBytes(HostName));
            BufferPosition += HostName.Length;

            // h_aliases list size
            Context.Memory.WriteInt32(BufferPosition, IPAddress.HostToNetworkOrder(HostEntry.Aliases.Length));
            BufferPosition += 4;

            // Actual aliases
            foreach (string Alias in HostEntry.Aliases)
            {
                Context.Memory.WriteBytes(BufferPosition, Encoding.ASCII.GetBytes(Alias + '\0'));
                BufferPosition += Alias.Length + 1;
            }

            // h_addrtype but it's a short (also only support IPv4)
            Context.Memory.WriteInt16(BufferPosition, IPAddress.HostToNetworkOrder((short)2));
            BufferPosition += 2;

            // h_length but it's a short
            Context.Memory.WriteInt16(BufferPosition, IPAddress.HostToNetworkOrder((short)4));
            BufferPosition += 2;

            // Ip address count, we can only support ipv4 (blame Nintendo)
            Context.Memory.WriteInt32(BufferPosition, Addresses != null ? IPAddress.HostToNetworkOrder(Addresses.Count) : 0);
            BufferPosition += 4;

            if (Addresses != null)
            {
                foreach (IPAddress Ip in Addresses)
                {
                    Context.Memory.WriteInt32(BufferPosition, IPAddress.HostToNetworkOrder(BitConverter.ToInt32(Ip.GetAddressBytes(), 0)));
                    BufferPosition += 4;
                }
            }

            return BufferPosition - OriginalBufferPosition;
        }

        private string GetGaiStringErrorFromErrorCode(GaiError ErrorCode)
        {
            if (ErrorCode > GaiError.Max)
            {
                ErrorCode = GaiError.Max;
            }

            switch (ErrorCode)
            {
                case GaiError.AddressFamily:
                    return "Address family for hostname not supported";
                case GaiError.Again:
                    return "Temporary failure in name resolution";
                case GaiError.BadFlags:
                    return "Invalid value for ai_flags";
                case GaiError.Fail:
                    return "Non-recoverable failure in name resolution";
                case GaiError.Family:
                    return "ai_family not supported";
                case GaiError.Memory:
                    return "Memory allocation failure";
                case GaiError.NoData:
                    return "No address associated with hostname";
                case GaiError.NoName:
                    return "hostname nor servname provided, or not known";
                case GaiError.Service:
                    return "servname not supported for ai_socktype";
                case GaiError.SocketType:
                    return "ai_socktype not supported";
                case GaiError.System:
                    return "System error returned in errno";
                case GaiError.BadHints:
                    return "Invalid value for hints";
                case GaiError.Protocol:
                    return "Resolved protocol is unknown";
                case GaiError.Overflow:
                    return "Argument buffer overflow";
                case GaiError.Max:
                    return "Unknown error";
                default:
                    return "Success";
            }
        }

        private string GetHostStringErrorFromErrorCode(NetDBError ErrorCode)
        {
            if (ErrorCode <= NetDBError.Internal)
            {
                return "Resolver internal error";
            }

            switch (ErrorCode)
            {
                case NetDBError.Success:
                    return "Resolver Error 0 (no error)";
                case NetDBError.HostNotFound:
                    return "Unknown host";
                case NetDBError.TryAgain:
                    return "Host name lookup failure";
                case NetDBError.NoRecovery:
                    return "Unknown server error";
                case NetDBError.NoData:
                    return "No address associated with name";
                default:
                    return "Unknown resolver error";
            }
        }

        private List<IPAddress> GetIPV4Addresses(IPHostEntry HostEntry)
        {
            List<IPAddress> Result = new List<IPAddress>();
            foreach (IPAddress Ip in HostEntry.AddressList)
            {
                if (Ip.AddressFamily == AddressFamily.InterNetwork)
                    Result.Add(Ip);
            }
            return Result;
        }

        // SetDnsAddressesPrivate(u32, buffer<unknown, 5, 0>)
        public long SetDnsAddressesPrivate(ServiceCtx Context)
        {
            uint Unknown0       = Context.RequestData.ReadUInt32();
            long BufferPosition = Context.Request.SendBuff[0].Position;
            long BufferSize     = Context.Request.SendBuff[0].Size;

            // TODO: This is stubbed in 2.0.0+, reverse 1.0.0 version for the sake completeness.
            Logger.PrintStub(LogClass.ServiceSfdnsres, $"Stubbed. Unknown0: {Unknown0}");

            return MakeError(ErrorModule.Os, 1023);
        }

        // GetDnsAddressPrivate(u32) -> buffer<unknown, 6, 0>
        public long GetDnsAddressesPrivate(ServiceCtx Context)
        {
            uint Unknown0 = Context.RequestData.ReadUInt32();

            // TODO: This is stubbed in 2.0.0+, reverse 1.0.0 version for the sake completeness.
            Logger.PrintStub(LogClass.ServiceSfdnsres, $"Stubbed. Unknown0: {Unknown0}");

            return MakeError(ErrorModule.Os, 1023);
        }

        // GetHostByName(u8, u32, u64, pid, buffer<unknown, 5, 0>) -> (u32, u32, u32, buffer<unknown, 6, 0>)
        public long GetHostByName(ServiceCtx Context)
        {
            byte[] RawName = Context.Memory.ReadBytes(Context.Request.SendBuff[0].Position, Context.Request.SendBuff[0].Size);
            string Name    = Encoding.ASCII.GetString(RawName).TrimEnd('\0');

            // TODO: use params
            bool  EnableNsdResolve = Context.RequestData.ReadInt32() == 1;
            int   TimeOut          = Context.RequestData.ReadInt32();
            ulong PidPlaceholder   = Context.RequestData.ReadUInt64();

            IPHostEntry HostEntry = null;

            NetDBError NetDBErrorCode = NetDBError.Success;
            GaiError   Errno          = GaiError.Overflow;
            long       SerializedSize = 0;

            if (Name.Length <= 255)
            {
                try
                {
                    HostEntry = Dns.GetHostEntry(Name);
                }
                catch (SocketException Exception)
                {
                    NetDBErrorCode = NetDBError.Internal;

                    if (Exception.ErrorCode == 11001)
                    {
                        NetDBErrorCode = NetDBError.HostNotFound;
                        Errno = GaiError.NoData;
                    }
                    else if (Exception.ErrorCode == 11002)
                    {
                        NetDBErrorCode = NetDBError.TryAgain;
                    }
                    else if (Exception.ErrorCode == 11003)
                    {
                        NetDBErrorCode = NetDBError.NoRecovery;
                    }
                    else if (Exception.ErrorCode == 11004)
                    {
                        NetDBErrorCode = NetDBError.NoData;
                    }
                    else if (Exception.ErrorCode == 10060)
                    {
                        Errno = GaiError.Again;
                    }
                }
            }
            else
            {
                NetDBErrorCode = NetDBError.HostNotFound;
            }

            if (HostEntry != null)
            {
                Errno = GaiError.Success;

                List<IPAddress> Addresses = GetIPV4Addresses(HostEntry);

                if (Addresses.Count == 0)
                {
                    Errno          = GaiError.NoData;
                    NetDBErrorCode = NetDBError.NoAddress;
                }
                else
                {
                    SerializedSize = SerializeHostEnt(Context, HostEntry, Addresses);
                }
            }

            Context.ResponseData.Write((int)NetDBErrorCode);
            Context.ResponseData.Write((int)Errno);
            Context.ResponseData.Write(SerializedSize);

            return 0;
        }

        // GetHostByAddr(u32, u32, u32, u64, pid, buffer<unknown, 5, 0>) -> (u32, u32, u32, buffer<unknown, 6, 0>)
        public long GetHostByAddress(ServiceCtx Context)
        {
            byte[] RawIp = Context.Memory.ReadBytes(Context.Request.SendBuff[0].Position, Context.Request.SendBuff[0].Size);

            // TODO: use params
            uint  SocketLength   = Context.RequestData.ReadUInt32();
            uint  Type           = Context.RequestData.ReadUInt32();
            int   TimeOut        = Context.RequestData.ReadInt32();
            ulong PidPlaceholder = Context.RequestData.ReadUInt64();

            IPHostEntry HostEntry = null;

            NetDBError NetDBErrorCode = NetDBError.Success;
            GaiError   Errno          = GaiError.AddressFamily;
            long       SerializedSize = 0;

            if (RawIp.Length == 4)
            {
                try
                {
                    IPAddress Address = new IPAddress(RawIp);

                    HostEntry = Dns.GetHostEntry(Address);
                }
                catch (SocketException Exception)
                {
                    NetDBErrorCode = NetDBError.Internal;
                    if (Exception.ErrorCode == 11001)
                    {
                        NetDBErrorCode = NetDBError.HostNotFound;
                        Errno = GaiError.NoData;
                    }
                    else if (Exception.ErrorCode == 11002)
                    {
                        NetDBErrorCode = NetDBError.TryAgain;
                    }
                    else if (Exception.ErrorCode == 11003)
                    {
                        NetDBErrorCode = NetDBError.NoRecovery;
                    }
                    else if (Exception.ErrorCode == 11004)
                    {
                        NetDBErrorCode = NetDBError.NoData;
                    }
                    else if (Exception.ErrorCode == 10060)
                    {
                        Errno = GaiError.Again;
                    }
                }
            }
            else
            {
                NetDBErrorCode = NetDBError.NoAddress;
            }

            if (HostEntry != null)
            {
                Errno = GaiError.Success;
                SerializedSize = SerializeHostEnt(Context, HostEntry, GetIPV4Addresses(HostEntry));
            }

            Context.ResponseData.Write((int)NetDBErrorCode);
            Context.ResponseData.Write((int)Errno);
            Context.ResponseData.Write(SerializedSize);

            return 0;
        }

        // GetHostStringError(u32) -> buffer<unknown, 6, 0>
        public long GetHostStringError(ServiceCtx Context)
        {
            long       ResultCode  = MakeError(ErrorModule.Os, 1023);
            NetDBError ErrorCode   = (NetDBError)Context.RequestData.ReadInt32();
            string     ErrorString = GetHostStringErrorFromErrorCode(ErrorCode);

            if (ErrorString.Length + 1 <= Context.Request.ReceiveBuff[0].Size)
            {
                ResultCode = 0;
                Context.Memory.WriteBytes(Context.Request.ReceiveBuff[0].Position, Encoding.ASCII.GetBytes(ErrorString + '\0'));
            }

            return ResultCode;
        }

        // GetGaiStringError(u32) -> buffer<unknown, 6, 0>
        public long GetGaiStringError(ServiceCtx Context)
        {
            long     ResultCode  = MakeError(ErrorModule.Os, 1023);
            GaiError ErrorCode   = (GaiError)Context.RequestData.ReadInt32();
            string   ErrorString = GetGaiStringErrorFromErrorCode(ErrorCode);

            if (ErrorString.Length + 1 <= Context.Request.ReceiveBuff[0].Size)
            {
                ResultCode = 0;
                Context.Memory.WriteBytes(Context.Request.ReceiveBuff[0].Position, Encoding.ASCII.GetBytes(ErrorString + '\0'));
            }

            return ResultCode;
        }

        // RequestCancelHandle(u64, pid) -> u32
        public long RequestCancelHandle(ServiceCtx Context)
        {
            ulong Unknown0 = Context.RequestData.ReadUInt64();

            Context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceSfdnsres, $"Stubbed. Unknown0: {Unknown0}");

            return 0;
        }

        // CancelSocketCall(u32, u64, pid)
        public long CancelSocketCall(ServiceCtx Context)
        {
            uint  Unknown0 = Context.RequestData.ReadUInt32();
            ulong Unknown1 = Context.RequestData.ReadUInt64();

            Logger.PrintStub(LogClass.ServiceSfdnsres, $"Stubbed. Unknown0: {Unknown0} - " +
                                                       $"Unknown1: {Unknown1}");

            return 0;
        }

        // ClearDnsAddresses(u32)
        public long ClearDnsAddresses(ServiceCtx Context)
        {
            uint Unknown0 = Context.RequestData.ReadUInt32();

            Logger.PrintStub(LogClass.ServiceSfdnsres, $"Stubbed. Unknown0: {Unknown0}");

            return 0;
        }
    }
}
