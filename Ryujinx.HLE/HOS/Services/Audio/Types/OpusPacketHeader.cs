using Ryujinx.Common;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct OpusPacketHeader
    {
        public uint length;
        public uint finalRange;

        public static OpusPacketHeader FromSpan(ReadOnlySpan<byte> data)
        {
            OpusPacketHeader header = MemoryMarshal.Cast<byte, OpusPacketHeader>(data)[0];

            header.length     = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(header.length)     : header.length;
            header.finalRange = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(header.finalRange) : header.finalRange;

            return header;
        }
    }
}
