using LibHac.Fs;
using LibHac.Loader;

namespace Ryujinx.HLE.Loaders.Executables
{
    class KipExecutable : IExecutable
    {
        public byte[] Text { get; }
        public byte[] Ro { get; }
        public byte[] Data { get; }

        public int TextOffset { get; }
        public int RoOffset { get; }
        public int DataOffset { get; }
        public int BssOffset { get; }
        public int BssSize { get; }

        public int[] Capabilities { get; }
        public bool UsesSecureMemory { get; }
        public bool Is64BitAddressSpace { get; }
        public bool Is64Bit { get; }
        public ulong ProgramId { get; }
        public byte Priority { get; }
        public int StackSize { get; }
        public byte IdealCoreId { get; }
        public int Version { get; }
        public string Name { get; }

        public KipExecutable(IStorage inStorage)
        {
            KipReader reader = new KipReader();

            reader.Initialize(inStorage).ThrowIfFailure();

            TextOffset = reader.Segments[0].MemoryOffset;
            RoOffset = reader.Segments[1].MemoryOffset;
            DataOffset = reader.Segments[2].MemoryOffset;
            BssOffset = reader.Segments[3].MemoryOffset;
            BssSize = reader.Segments[3].Size;

            StackSize = reader.StackSize;

            UsesSecureMemory = reader.UsesSecureMemory;
            Is64BitAddressSpace = reader.Is64BitAddressSpace;
            Is64Bit = reader.Is64Bit;

            ProgramId = reader.ProgramId;
            Priority = reader.Priority;
            IdealCoreId = reader.IdealCoreId;
            Version = reader.Version;
            Name = reader.Name.ToString();

            Capabilities = new int[32];

            for (int index = 0; index < Capabilities.Length; index++)
            {
                Capabilities[index] = (int)reader.Capabilities[index];
            }

            Text = DecompressSection(reader, KipReader.SegmentType.Text);
            Ro = DecompressSection(reader, KipReader.SegmentType.Ro);
            Data = DecompressSection(reader, KipReader.SegmentType.Data);
        }

        private static byte[] DecompressSection(KipReader reader, KipReader.SegmentType segmentType)
        {
            reader.GetSegmentSize(segmentType, out int uncompressedSize).ThrowIfFailure();

            byte[] result = new byte[uncompressedSize];

            reader.ReadSegment(segmentType, result).ThrowIfFailure();

            return result;
        }
    }
}