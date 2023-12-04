using Ryujinx.HLE.HOS.Services.Sockets.Bsd;
using System;

namespace Ryujinx.HLE.HOS.Services.Ssl.SslService
{
    interface ISslConnectionBase : IDisposable
    {
        int SocketFd { get; }

        ISocket Socket { get; }

        ResultCode Handshake(string hostName);

        ResultCode GetServerCertificate(string hostname, Span<byte> certificates, out uint storageSize, out uint certificateCount);

        ResultCode Write(out int writtenCount, ReadOnlyMemory<byte> buffer);

        ResultCode Read(out int readCount, Memory<byte> buffer);

        ResultCode Peek(out int peekCount, Memory<byte> buffer);

        int Pending();
    }
}
