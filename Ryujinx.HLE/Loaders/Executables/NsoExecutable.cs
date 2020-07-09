using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Loader;
using System;

namespace Ryujinx.HLE.Loaders.Executables
{
    class NsoExecutable : IExecutable
    {
        public byte[] Program { get; }
        public Span<byte> Text => Program.AsSpan().Slice(TextOffset, TextSize);
        public Span<byte> Ro   => Program.AsSpan().Slice(RoOffset,   RoSize);
        public Span<byte> Data => Program.AsSpan().Slice(DataOffset, DataSize);

        public int TextOffset { get; }
        public int RoOffset { get; }
        public int DataOffset { get; }
        public int BssOffset => DataOffset + Data.Length;

        public int TextSize { get; }
        public int RoSize { get; }
        public int DataSize { get; }
        public int BssSize { get; }

        public string Name;
        public Buffer32 BuildId;

        public NsoExecutable(IStorage inStorage, string name = null)
        {
            NsoReader reader = new NsoReader();

            reader.Initialize(inStorage.AsFile(OpenMode.Read)).ThrowIfFailure();

            TextOffset = (int)reader.Header.Segments[0].MemoryOffset;
            RoOffset = (int)reader.Header.Segments[1].MemoryOffset;
            DataOffset = (int)reader.Header.Segments[2].MemoryOffset;
            BssSize = (int)reader.Header.BssSize;

            reader.GetSegmentSize(NsoReader.SegmentType.Data, out uint uncompressedSize).ThrowIfFailure();
            Program = new byte[DataOffset + uncompressedSize];

            TextSize = DecompressSection(reader, NsoReader.SegmentType.Text, TextOffset, Program);
            RoSize   = DecompressSection(reader, NsoReader.SegmentType.Ro,   RoOffset,   Program);
            DataSize = DecompressSection(reader, NsoReader.SegmentType.Data, DataOffset, Program);

            Name = name;
            BuildId = reader.Header.ModuleId;
        }

        private static int DecompressSection(NsoReader reader, NsoReader.SegmentType segmentType, int offset, byte[] Program)
        {
            reader.GetSegmentSize(segmentType, out uint uncompressedSize).ThrowIfFailure();

            var span = Program.AsSpan().Slice(offset, (int)uncompressedSize);

            reader.ReadSegment(segmentType, span).ThrowIfFailure();

            return (int)uncompressedSize;
        }
    }
}