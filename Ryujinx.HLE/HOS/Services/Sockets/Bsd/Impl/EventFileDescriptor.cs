using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    class EventFileDescriptor : IFileDescriptor
    {
        private ulong _value;
        private readonly EventFdFlags _flags;

        private object _lock = new object();

        public bool Blocking { get => !_flags.HasFlag(EventFdFlags.NonBlocking); set => throw new NotSupportedException(); }

        public ManualResetEvent WriteEvent { get; }
        public ManualResetEvent ReadEvent { get; }

        public EventFileDescriptor(ulong value, EventFdFlags flags)
        {
            _value = value;
            _flags = flags;

            WriteEvent = new ManualResetEvent(true);
            ReadEvent = new ManualResetEvent(true);
        }

        public int Refcount { get; set; }

        public void Dispose()
        {
            WriteEvent.Dispose();
            ReadEvent.Dispose();
        }

        public LinuxError Read(out int readSize, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong))
            {
                readSize = 0;

                return LinuxError.EINVAL;
            }

            ReadEvent.Reset();

            lock (_lock)
            {
                ref ulong count = ref MemoryMarshal.Cast<byte, ulong>(buffer)[0];

                if (_value == 0)
                {
                    if (Blocking)
                    {
                        while (_value == 0)
                        {
                            Monitor.Wait(_lock);
                        }
                    }
                    else
                    {
                        readSize = 0;

                        return LinuxError.EAGAIN;
                    }
                }

                readSize = sizeof(ulong);

                if (_flags.HasFlag(EventFdFlags.Semaphore))
                {
                    --_value;

                    count = 1;
                }
                else
                {
                    count = _value;

                    _value = 0;
                }

                ReadEvent.Set();

                return LinuxError.SUCCESS;
            }
        }

        public LinuxError Write(out int writeSize, ReadOnlySpan<byte> buffer)
        {
            if (!MemoryMarshal.TryRead(buffer, out ulong count) || count == ulong.MaxValue)
            {
                writeSize = 0;

                return LinuxError.EINVAL;
            }

            WriteEvent.Reset();

            lock (_lock)
            {
                if (_value > _value + count)
                {
                    if (Blocking)
                    {
                        Monitor.Wait(_lock);
                    }
                    else
                    {
                        writeSize = 0;

                        return LinuxError.EAGAIN;
                    }
                }

                writeSize = sizeof(ulong);

                _value += count;
                Monitor.Pulse(_lock);

                WriteEvent.Set();

                return LinuxError.SUCCESS;
            }
        }
    }
}
