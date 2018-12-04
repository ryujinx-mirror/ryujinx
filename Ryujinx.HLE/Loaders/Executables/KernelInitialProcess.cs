using Ryujinx.HLE.Loaders.Compression;
using System.IO;

namespace Ryujinx.HLE.Loaders.Executables
{
    class KernelInitialProcess : IExecutable
    {
        public string Name { get; }

        public long TitleId { get; }

        public int ProcessCategory { get; }

        public byte MainThreadPriority { get; }
        public byte DefaultProcessorId { get; }

        public bool Is64Bits   { get; }
        public bool Addr39Bits { get; }
        public bool IsService  { get; }

        public byte[] Text { get; }
        public byte[] Ro   { get; }
        public byte[] Data { get; }

        public int TextOffset { get; }
        public int RoOffset   { get; }
        public int DataOffset { get; }
        public int BssOffset  { get; }
        public int BssSize    { get; }

        public int MainThreadStackSize { get; }

        public int[] Capabilities { get; }

        private struct SegmentHeader
        {
            public int Offset           { get; }
            public int DecompressedSize { get; }
            public int CompressedSize   { get; }
            public int Attribute        { get; }

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

            Capabilities = new int[8];

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
            long end = input.Position + header.CompressedSize;

            input.Seek(end, SeekOrigin.Begin);

            byte[] data = BackwardsLz.Decompress(input, header.DecompressedSize);

            input.Seek(end, SeekOrigin.Begin);

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