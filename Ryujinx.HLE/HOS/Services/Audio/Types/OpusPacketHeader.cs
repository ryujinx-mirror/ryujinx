using Ryujinx.Common;
using System;
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

            header.length     = EndianSwap.FromBigEndianToPlatformEndian(header.length);
            header.finalRange = EndianSwap.FromBigEndianToPlatformEndian(header.finalRange);

            return header;
        }
    }
}
