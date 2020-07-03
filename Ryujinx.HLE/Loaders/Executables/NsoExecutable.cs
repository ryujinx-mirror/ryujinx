using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Loader;

namespace Ryujinx.HLE.Loaders.Executables
{
    class NsoExecutable : IExecutable
    {
        public byte[] Text { get; }
        public byte[] Ro { get; }
        public byte[] Data { get; }

        public int TextOffset { get; }
        public int RoOffset { get; }
        public int DataOffset { get; }
        public int BssOffset => DataOffset + Data.Length;

        public int BssSize { get; }

        public NsoExecutable(IStorage inStorage)
        {
            NsoReader reader = new NsoReader();

            reader.Initialize(inStorage.AsFile(OpenMode.Read)).ThrowIfFailure();

            TextOffset = (int)reader.Header.Segments[0].MemoryOffset;
            RoOffset = (int)reader.Header.Segments[1].MemoryOffset;
            DataOffset = (int)reader.Header.Segments[2].MemoryOffset;
            BssSize = (int)reader.Header.BssSize;

            Text = DecompressSection(reader, NsoReader.SegmentType.Text);
            Ro = DecompressSection(reader, NsoReader.SegmentType.Ro);
            Data = DecompressSection(reader, NsoReader.SegmentType.Data);
        }

        private static byte[] DecompressSection(NsoReader reader, NsoReader.SegmentType segmentType)
        {
            reader.GetSegmentSize(segmentType, out uint uncompressedSize).ThrowIfFailure();

            byte[] result = new byte[uncompressedSize];

            reader.ReadSegment(segmentType, result).ThrowIfFailure();

            return result;
        }
    }
}