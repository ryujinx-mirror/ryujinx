using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.Common.SystemInterop
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public partial class UnixStream : Stream, IDisposable
    {
        private const int InvalidFd = -1;

        private int _fd;

        [LibraryImport("libc", SetLastError = true)]
        private static partial long read(int fd, IntPtr buf, ulong count);

        [LibraryImport("libc", SetLastError = true)]
        private static partial long write(int fd, IntPtr buf, ulong count);

        [LibraryImport("libc", SetLastError = true)]
        private static partial int close(int fd);

        public UnixStream(int fd)
        {
            if (InvalidFd == fd)
            {
                throw new ArgumentException("Invalid file descriptor");
            }

            _fd = fd;
            
            CanRead = read(fd, IntPtr.Zero, 0) != -1;
            CanWrite = write(fd, IntPtr.Zero, 0) != -1;  
        }

        ~UnixStream()
        {
            Close();
        }

        public override bool CanRead { get; }
        public override bool CanWrite { get; }
        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override unsafe int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if (offset < 0 || offset > (buffer.Length - count) || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (buffer.Length == 0)
            {
                return 0;
            }

            long r = 0;
            fixed (byte* buf = &buffer[offset])
            {
                do
                {
                    r = read(_fd, (IntPtr)buf, (ulong)count);
                } while (ShouldRetry(r));
            }

            return (int)r;
        }
        
        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            if (offset < 0 || offset > (buffer.Length - count) || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (buffer.Length == 0)
            {
                return;
            }

            fixed (byte* buf = &buffer[offset])
            {
                long r = 0;
                do {
                    r = write(_fd, (IntPtr)buf, (ulong)count);
                } while (ShouldRetry(r));
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            if (_fd == InvalidFd)
            {
                return;
            }

            Flush();

            int r;
            do {
                r = close(_fd);
            } while (ShouldRetry(r));

            _fd = InvalidFd;
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        private bool ShouldRetry(long r)
        {
            if (r == -1)
            {
                const int eintr = 4;

                int errno = Marshal.GetLastPInvokeError();

                if (errno == eintr)
                {
                    return true;
                }

                throw new SystemException($"Operation failed with error 0x{errno:X}");
            }

            return false;
        }
    }
}
