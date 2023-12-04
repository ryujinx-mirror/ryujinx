using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Types
{
    class BsdMsgHdr
    {
        public byte[] Name { get; }
        public byte[][] Iov { get; }
        public byte[] Control { get; }
        public BsdSocketFlags Flags { get; }
        public uint Length;

        private BsdMsgHdr(byte[] name, byte[][] iov, byte[] control, BsdSocketFlags flags, uint length)
        {
            Name = name;
            Iov = iov;
            Control = control;
            Flags = flags;
            Length = length;
        }

        public static LinuxError Serialize(ref Span<byte> rawData, BsdMsgHdr message)
        {
            int msgNameLength = message.Name == null ? 0 : message.Name.Length;
            int iovCount = message.Iov == null ? 0 : message.Iov.Length;
            int controlLength = message.Control == null ? 0 : message.Control.Length;
            BsdSocketFlags flags = message.Flags;

            if (!MemoryMarshal.TryWrite(rawData, in msgNameLength))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            if (msgNameLength > 0)
            {
                if (rawData.Length < msgNameLength)
                {
                    return LinuxError.EFAULT;
                }

                message.Name.CopyTo(rawData);
                rawData = rawData[msgNameLength..];
            }

            if (!MemoryMarshal.TryWrite(rawData, in iovCount))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            if (iovCount > 0)
            {
                for (int index = 0; index < iovCount; index++)
                {
                    ulong iovLength = (ulong)message.Iov[index].Length;

                    if (!MemoryMarshal.TryWrite(rawData, in iovLength))
                    {
                        return LinuxError.EFAULT;
                    }

                    rawData = rawData[sizeof(ulong)..];

                    if (iovLength > 0)
                    {
                        if ((ulong)rawData.Length < iovLength)
                        {
                            return LinuxError.EFAULT;
                        }

                        message.Iov[index].CopyTo(rawData);
                        rawData = rawData[(int)iovLength..];
                    }
                }
            }

            if (!MemoryMarshal.TryWrite(rawData, in controlLength))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            if (controlLength > 0)
            {
                if (rawData.Length < controlLength)
                {
                    return LinuxError.EFAULT;
                }

                message.Control.CopyTo(rawData);
                rawData = rawData[controlLength..];
            }

            if (!MemoryMarshal.TryWrite(rawData, in flags))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(BsdSocketFlags)..];

            if (!MemoryMarshal.TryWrite(rawData, in message.Length))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            return LinuxError.SUCCESS;
        }

        public static LinuxError Deserialize(out BsdMsgHdr message, ref ReadOnlySpan<byte> rawData)
        {
            byte[] name = null;
            byte[][] iov = null;
            byte[] control = null;

            message = null;

            if (!MemoryMarshal.TryRead(rawData, out uint msgNameLength))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            if (msgNameLength > 0)
            {
                if (rawData.Length < msgNameLength)
                {
                    return LinuxError.EFAULT;
                }

                name = rawData[..(int)msgNameLength].ToArray();
                rawData = rawData[(int)msgNameLength..];
            }

            if (!MemoryMarshal.TryRead(rawData, out uint iovCount))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            if (iovCount > 0)
            {
                iov = new byte[iovCount][];

                for (int index = 0; index < iov.Length; index++)
                {
                    if (!MemoryMarshal.TryRead(rawData, out ulong iovLength))
                    {
                        return LinuxError.EFAULT;
                    }

                    rawData = rawData[sizeof(ulong)..];

                    if (iovLength > 0)
                    {
                        if ((ulong)rawData.Length < iovLength)
                        {
                            return LinuxError.EFAULT;
                        }

                        iov[index] = rawData[..(int)iovLength].ToArray();
                        rawData = rawData[(int)iovLength..];
                    }
                }
            }

            if (!MemoryMarshal.TryRead(rawData, out uint controlLength))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            if (controlLength > 0)
            {
                if (rawData.Length < controlLength)
                {
                    return LinuxError.EFAULT;
                }

                control = rawData[..(int)controlLength].ToArray();
                rawData = rawData[(int)controlLength..];
            }

            if (!MemoryMarshal.TryRead(rawData, out BsdSocketFlags flags))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(BsdSocketFlags)..];

            if (!MemoryMarshal.TryRead(rawData, out uint length))
            {
                return LinuxError.EFAULT;
            }

            rawData = rawData[sizeof(uint)..];

            message = new BsdMsgHdr(name, iov, control, flags, length);

            return LinuxError.SUCCESS;
        }
    }
}
