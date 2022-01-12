using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    class BsdContext
    {
        private static ConcurrentDictionary<long, BsdContext> _registry = new ConcurrentDictionary<long, BsdContext>();

        private readonly object _lock = new object();

        private List<IFileDescriptor> _fds;

        private BsdContext()
        {
            _fds = new List<IFileDescriptor>();
        }

        public ISocket RetrieveSocket(int socketFd)
        {
            IFileDescriptor file = RetrieveFileDescriptor(socketFd);

            if (file is ISocket socket)
            {
                return socket;
            }

            return null;
        }

        public IFileDescriptor RetrieveFileDescriptor(int fd)
        {
            lock (_lock)
            {
                if (fd >= 0 && _fds.Count > fd)
                {
                    return _fds[fd];
                }
            }

            return null;
        }

        public int RegisterFileDescriptor(IFileDescriptor file)
        {
            lock (_lock)
            {
                for (int fd = 0; fd < _fds.Count; fd++)
                {
                    if (_fds[fd] == null)
                    {
                        _fds[fd] = file;

                        return fd;
                    }
                }

                _fds.Add(file);

                return _fds.Count - 1;
            }
        }

        public int DuplicateFileDescriptor(int fd)
        {
            IFileDescriptor oldFile = RetrieveFileDescriptor(fd);

            if (oldFile != null)
            {
                lock (_lock)
                {
                    oldFile.Refcount++;

                    return RegisterFileDescriptor(oldFile);
                }
            }

            return -1;
        }

        public bool CloseFileDescriptor(int fd)
        {
            IFileDescriptor file = RetrieveFileDescriptor(fd);

            if (file != null)
            {
                file.Refcount--;

                if (file.Refcount <= 0)
                {
                    file.Dispose();
                }

                lock (_lock)
                {
                    _fds[fd] = null;
                }

                return true;
            }

            return false;
        }

        public LinuxError ShutdownAllSockets(BsdSocketShutdownFlags how)
        {
            lock (_lock)
            {
                foreach (IFileDescriptor file in _fds)
                {
                    if (file is ISocket socket)
                    {
                        LinuxError errno = socket.Shutdown(how);

                        if (errno != LinuxError.SUCCESS)
                        {
                            return errno;
                        }
                    }
                }
            }

            return LinuxError.SUCCESS;
        }

        public static BsdContext GetOrRegister(long processId)
        {
            BsdContext context = GetContext(processId);

            if (context == null)
            {
                context = new BsdContext();

                _registry.TryAdd(processId, context);
            }

            return context;
        }

        public static BsdContext GetContext(long processId)
        {
            if (!_registry.TryGetValue(processId, out BsdContext processContext))
            {
                return null;
            }

            return processContext;
        }
    }
}
