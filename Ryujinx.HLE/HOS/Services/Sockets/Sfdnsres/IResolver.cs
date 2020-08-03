using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres
{
    [Service("sfdnsres")]
    class IResolver : IpcService
    {
        public IResolver(ServiceCtx context) { }

        private long SerializeHostEnt(ServiceCtx context, IPHostEntry hostEntry, List<IPAddress> addresses = null)
        {
            long originalBufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferPosition         = originalBufferPosition;
            long bufferSize             = context.Request.ReceiveBuff[0].Size;

            string hostName = hostEntry.HostName + '\0';

            // h_name
            context.Memory.Write((ulong)bufferPosition, Encoding.ASCII.GetBytes(hostName));
            bufferPosition += hostName.Length;

            // h_aliases list size
            context.Memory.Write((ulong)bufferPosition, IPAddress.HostToNetworkOrder(hostEntry.Aliases.Length));
            bufferPosition += 4;

            // Actual aliases
            foreach (string alias in hostEntry.Aliases)
            {
                context.Memory.Write((ulong)bufferPosition, Encoding.ASCII.GetBytes(alias + '\0'));
                bufferPosition += alias.Length + 1;
            }

            // h_addrtype but it's a short (also only support IPv4)
            context.Memory.Write((ulong)bufferPosition, IPAddress.HostToNetworkOrder((short)2));
            bufferPosition += 2;

            // h_length but it's a short
            context.Memory.Write((ulong)bufferPosition, IPAddress.HostToNetworkOrder((short)4));
            bufferPosition += 2;

            // Ip address count, we can only support ipv4 (blame Nintendo)
            context.Memory.Write((ulong)bufferPosition, addresses != null ? IPAddress.HostToNetworkOrder(addresses.Count) : 0);
            bufferPosition += 4;

            if (addresses != null)
            {
                foreach (IPAddress ip in addresses)
                {
                    context.Memory.Write((ulong)bufferPosition, IPAddress.HostToNetworkOrder(BitConverter.ToInt32(ip.GetAddressBytes(), 0)));
                    bufferPosition += 4;
                }
            }

            return bufferPosition - originalBufferPosition;
        }

        private string GetGaiStringErrorFromErrorCode(GaiError errorCode)
        {
            if (errorCode > GaiError.Max)
            {
                errorCode = GaiError.Max;
            }

            switch (errorCode)
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

        private string GetHostStringErrorFromErrorCode(NetDbError errorCode)
        {
            if (errorCode <= NetDbError.Internal)
            {
                return "Resolver internal error";
            }

            switch (errorCode)
            {
                case NetDbError.Success:
                    return "Resolver Error 0 (no error)";
                case NetDbError.HostNotFound:
                    return "Unknown host";
                case NetDbError.TryAgain:
                    return "Host name lookup failure";
                case NetDbError.NoRecovery:
                    return "Unknown server error";
                case NetDbError.NoData:
                    return "No address associated with name";
                default:
                    return "Unknown resolver error";
            }
        }

        private List<IPAddress> GetIpv4Addresses(IPHostEntry hostEntry)
        {
            List<IPAddress> result = new List<IPAddress>();
            foreach (IPAddress ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    result.Add(ip);
            }
            return result;
        }

        [Command(0)]
        // SetDnsAddressesPrivate(u32, buffer<unknown, 5, 0>)
        public ResultCode SetDnsAddressesPrivate(ServiceCtx context)
        {
            uint unknown0       = context.RequestData.ReadUInt32();
            long bufferPosition = context.Request.SendBuff[0].Position;
            long bufferSize     = context.Request.SendBuff[0].Size;

            // TODO: This is stubbed in 2.0.0+, reverse 1.0.0 version for the sake completeness.
            Logger.Stub?.PrintStub(LogClass.ServiceSfdnsres, new { unknown0 });

            return ResultCode.NotAllocated;
        }

        [Command(1)]
        // GetDnsAddressPrivate(u32) -> buffer<unknown, 6, 0>
        public ResultCode GetDnsAddressesPrivate(ServiceCtx context)
        {
            uint unknown0 = context.RequestData.ReadUInt32();

            // TODO: This is stubbed in 2.0.0+, reverse 1.0.0 version for the sake completeness.
            Logger.Stub?.PrintStub(LogClass.ServiceSfdnsres, new { unknown0 });

            return ResultCode.NotAllocated;
        }

        [Command(2)]
        // GetHostByName(u8, u32, u64, pid, buffer<unknown, 5, 0>) -> (u32, u32, u32, buffer<unknown, 6, 0>)
        public ResultCode GetHostByName(ServiceCtx context)
        {
            byte[] rawName = new byte[context.Request.SendBuff[0].Size];

            context.Memory.Read((ulong)context.Request.SendBuff[0].Position, rawName);

            string name = Encoding.ASCII.GetString(rawName).TrimEnd('\0');

            // TODO: use params
            bool  enableNsdResolve = context.RequestData.ReadInt32() == 1;
            int   timeOut          = context.RequestData.ReadInt32();
            ulong pidPlaceholder   = context.RequestData.ReadUInt64();

            IPHostEntry hostEntry = null;

            NetDbError netDbErrorCode = NetDbError.Success;
            GaiError   errno          = GaiError.Overflow;
            long       serializedSize = 0;

            if (name.Length <= 255)
            {
                try
                {
                    hostEntry = Dns.GetHostEntry(name);
                }
                catch (SocketException exception)
                {
                    netDbErrorCode = NetDbError.Internal;

                    if (exception.ErrorCode == 11001)
                    {
                        netDbErrorCode = NetDbError.HostNotFound;
                        errno = GaiError.NoData;
                    }
                    else if (exception.ErrorCode == 11002)
                    {
                        netDbErrorCode = NetDbError.TryAgain;
                    }
                    else if (exception.ErrorCode == 11003)
                    {
                        netDbErrorCode = NetDbError.NoRecovery;
                    }
                    else if (exception.ErrorCode == 11004)
                    {
                        netDbErrorCode = NetDbError.NoData;
                    }
                    else if (exception.ErrorCode == 10060)
                    {
                        errno = GaiError.Again;
                    }
                }
            }
            else
            {
                netDbErrorCode = NetDbError.HostNotFound;
            }

            if (hostEntry != null)
            {
                errno = GaiError.Success;

                List<IPAddress> addresses = GetIpv4Addresses(hostEntry);

                if (addresses.Count == 0)
                {
                    errno          = GaiError.NoData;
                    netDbErrorCode = NetDbError.NoAddress;
                }
                else
                {
                    serializedSize = SerializeHostEnt(context, hostEntry, addresses);
                }
            }

            context.ResponseData.Write((int)netDbErrorCode);
            context.ResponseData.Write((int)errno);
            context.ResponseData.Write(serializedSize);

            return ResultCode.Success;
        }

        [Command(3)]
        // GetHostByAddr(u32, u32, u32, u64, pid, buffer<unknown, 5, 0>) -> (u32, u32, u32, buffer<unknown, 6, 0>)
        public ResultCode GetHostByAddress(ServiceCtx context)
        {
            byte[] rawIp = new byte[context.Request.SendBuff[0].Size];

            context.Memory.Read((ulong)context.Request.SendBuff[0].Position, rawIp);

            // TODO: use params
            uint  socketLength   = context.RequestData.ReadUInt32();
            uint  type           = context.RequestData.ReadUInt32();
            int   timeOut        = context.RequestData.ReadInt32();
            ulong pidPlaceholder = context.RequestData.ReadUInt64();

            IPHostEntry hostEntry = null;

            NetDbError netDbErrorCode = NetDbError.Success;
            GaiError   errno          = GaiError.AddressFamily;
            long       serializedSize = 0;

            if (rawIp.Length == 4)
            {
                try
                {
                    IPAddress address = new IPAddress(rawIp);

                    hostEntry = Dns.GetHostEntry(address);
                }
                catch (SocketException exception)
                {
                    netDbErrorCode = NetDbError.Internal;
                    if (exception.ErrorCode == 11001)
                    {
                        netDbErrorCode = NetDbError.HostNotFound;
                        errno = GaiError.NoData;
                    }
                    else if (exception.ErrorCode == 11002)
                    {
                        netDbErrorCode = NetDbError.TryAgain;
                    }
                    else if (exception.ErrorCode == 11003)
                    {
                        netDbErrorCode = NetDbError.NoRecovery;
                    }
                    else if (exception.ErrorCode == 11004)
                    {
                        netDbErrorCode = NetDbError.NoData;
                    }
                    else if (exception.ErrorCode == 10060)
                    {
                        errno = GaiError.Again;
                    }
                }
            }
            else
            {
                netDbErrorCode = NetDbError.NoAddress;
            }

            if (hostEntry != null)
            {
                errno = GaiError.Success;
                serializedSize = SerializeHostEnt(context, hostEntry, GetIpv4Addresses(hostEntry));
            }

            context.ResponseData.Write((int)netDbErrorCode);
            context.ResponseData.Write((int)errno);
            context.ResponseData.Write(serializedSize);

            return ResultCode.Success;
        }

        [Command(4)]
        // GetHostStringError(u32) -> buffer<unknown, 6, 0>
        public ResultCode GetHostStringError(ServiceCtx context)
        {
            ResultCode resultCode  = ResultCode.NotAllocated;
            NetDbError errorCode   = (NetDbError)context.RequestData.ReadInt32();
            string     errorString = GetHostStringErrorFromErrorCode(errorCode);

            if (errorString.Length + 1 <= context.Request.ReceiveBuff[0].Size)
            {
                resultCode = 0;
                context.Memory.Write((ulong)context.Request.ReceiveBuff[0].Position, Encoding.ASCII.GetBytes(errorString + '\0'));
            }

            return resultCode;
        }

        [Command(5)]
        // GetGaiStringError(u32) -> buffer<unknown, 6, 0>
        public ResultCode GetGaiStringError(ServiceCtx context)
        {
            ResultCode resultCode  = ResultCode.NotAllocated;
            GaiError   errorCode   = (GaiError)context.RequestData.ReadInt32();
            string     errorString = GetGaiStringErrorFromErrorCode(errorCode);

            if (errorString.Length + 1 <= context.Request.ReceiveBuff[0].Size)
            {
                resultCode = 0;
                context.Memory.Write((ulong)context.Request.ReceiveBuff[0].Position, Encoding.ASCII.GetBytes(errorString + '\0'));
            }

            return resultCode;
        }

        [Command(8)]
        // RequestCancelHandle(u64, pid) -> u32
        public ResultCode RequestCancelHandle(ServiceCtx context)
        {
            ulong unknown0 = context.RequestData.ReadUInt64();

            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceSfdnsres, new { unknown0 });

            return ResultCode.Success;
        }

        [Command(9)]
        // CancelSocketCall(u32, u64, pid)
        public ResultCode CancelSocketCall(ServiceCtx context)
        {
            uint  unknown0 = context.RequestData.ReadUInt32();
            ulong unknown1 = context.RequestData.ReadUInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceSfdnsres, new { unknown0, unknown1 });

            return ResultCode.Success;
        }

        [Command(11)]
        // ClearDnsAddresses(u32)
        public ResultCode ClearDnsAddresses(ServiceCtx context)
        {
            uint unknown0 = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSfdnsres, new { unknown0 });

            return ResultCode.Success;
        }
    }
}