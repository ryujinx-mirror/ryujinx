using Ryujinx.Common.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Ryujinx.Common.SystemInterop
{
    public partial class StdErrAdapter : IDisposable
    {
        private bool _disposable = false;
        private UnixStream _pipeReader;
        private UnixStream _pipeWriter;
        private Thread _worker;

        public StdErrAdapter()
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                RegisterPosix();
            }
        }

        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        private void RegisterPosix()
        {
            const int stdErrFileno = 2;

            (int readFd, int writeFd) = MakePipe();
            dup2(writeFd, stdErrFileno);

            _pipeReader = new UnixStream(readFd);
            _pipeWriter = new UnixStream(writeFd);

            _worker = new Thread(EventWorker);
            _disposable = true;
            _worker.Start();
        }

        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        private void EventWorker()
        {
            TextReader reader = new StreamReader(_pipeReader);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Logger.Error?.PrintRawMsg(line);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposable)
            {
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    _pipeReader?.Close();
                    _pipeWriter?.Close();
                }

                _disposable = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        [LibraryImport("libc", SetLastError = true)]
        private static partial int dup2(int fd, int fd2);

        [LibraryImport("libc", SetLastError = true)]
        private static unsafe partial int pipe(int* pipefd);

        private static unsafe (int, int) MakePipe()
        {
            int *pipefd = stackalloc int[2];

            if (pipe(pipefd) == 0)
            {
                return (pipefd[0], pipefd[1]);
            }
            else
            {
                throw new();
            }
        }
    }
}
