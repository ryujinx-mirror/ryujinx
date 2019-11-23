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

        public static OpusPacketHeader FromStream(BinaryReader reader)
        {
            OpusPacketHeader header = reader.ReadStruct<OpusPacketHeader>();

            header.length     = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(header.length)     : header.length;
            header.finalRange = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(header.finalRange) : header.finalRange;

            return header;
        }
    }
}
