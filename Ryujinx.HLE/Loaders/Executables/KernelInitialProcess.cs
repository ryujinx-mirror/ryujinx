using Ryujinx.HLE.Loaders.Compression;
using System.IO;

namespace Ryujinx.HLE.Loaders.Executables
{
    class KernelInitialProcess : IExecutable
    {
        public string Name { get; private set; }

        public long TitleId { get; private set; }

        public int ProcessCategory { get; private set; }

        public byte MainThreadPriority { get; private set; }
        public byte DefaultProcessorId { get; private set; }

        public bool Is64Bits   { get; private set; }
        public bool Addr39Bits { get; private set; }
        public bool IsService  { get; private set; }

        public byte[] Text { get; private set; }
        public byte[] Ro   { get; private set; }
        public byte[] Data { get; private set; }

        public int TextOffset { get; private set; }
        public int RoOffset   { get; private set; }
        public int DataOffset { get; private set; }
        public int BssOffset  { get; private set; }
        public int BssSize    { get; private set; }

        public int MainThreadStackSize { get; private set; }

        public int[] Capabilities { get; private set; }

        private struct SegmentHeader
        {
            public int Offset           { get; private set; }
            public int DecompressedSize { get; private set; }
            public int CompressedSize   { get; private set; }
            public int Attribute        { get; private set; }

            public SegmentHeader(
                int offset,
                int decompressedSize,
                int compressedSize,
                int attribute)
            {
                Offset           = offset;
                DecompressedSize = decompressedSize;
                CompressedSize   = compressedSize;
                Attribute        = attribute;
            }
        }

        public KernelInitialProcess(Stream input)
        {
            BinaryReader reader = new BinaryReader(input);

            string magic = ReadString(reader, 4);

            if (magic != "KIP1")
            {

            }

            Name = ReadString(reader, 12);

            TitleId = reader.ReadInt64();

            ProcessCategory = reader.ReadInt32();

            MainThreadPriority = reader.ReadByte();
            DefaultProcessorId = reader.ReadByte();

            byte reserved = reader.ReadByte();
            byte flags    = reader.ReadByte();

            Is64Bits   = (flags & 0x08) != 0;
            Addr39Bits = (flags & 0x10) != 0;
            IsService  = (flags & 0x20) != 0;

            SegmentHeader[] segments = new SegmentHeader[6];

            for (int index = 0; index < segments.Length; index++)
            {
                segments[index] = new SegmentHeader(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32());
            }

            TextOffset = segments[0].Offset;
            RoOffset   = segments[1].Offset;
            DataOffset = segments[2].Offset;
            BssOffset  = segments[3].Offset;
            BssSize    = segments[3].DecompressedSize;

            MainThreadStackSize = segments[1].Attribute;

            Capabilities = new int[32];

            for (int index = 0; index < Capabilities.Length; index++)
            {
                Capabilities[index] = reader.ReadInt32();
            }

            input.Seek(0x100, SeekOrigin.Begin);

            Text = ReadSegment(segments[0], input);
            Ro   = ReadSegment(segments[1], input);
            Data = ReadSegment(segments[2], input);
        }

        private byte[] ReadSegment(SegmentHeader header, Stream input)
        {
            byte[] data = new byte[header.DecompressedSize];

            input.Read(data, 0, header.CompressedSize);

            BackwardsLz.DecompressInPlace(data, header.CompressedSize);

            return data;
        }

        private static string ReadString(BinaryReader reader, int maxSize)
        {
            string value = string.Empty;

            for (int index = 0; index < maxSize; index++)
            {
                char chr = (char)reader.ReadByte();

                if (chr == '\0')
                {
                    reader.BaseStream.Seek(maxSize - index - 1, SeekOrigin.Current);

                    break;
                }

                value += chr;
            }

            return value;
        }
    }
}