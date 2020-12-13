using LibHac.Fs;
using LibHac.Kernel;
using System;

namespace Ryujinx.HLE.Loaders.Executables
{
    class KipExecutable : IExecutable
    {
        public byte[] Program { get; }
        public Span<byte> Text => Program.AsSpan().Slice((int)TextOffset, (int)TextSize);
        public Span<byte> Ro   => Program.AsSpan().Slice((int)RoOffset,   (int)RoSize);
        public Span<byte> Data => Program.AsSpan().Slice((int)DataOffset, (int)DataSize);

        public uint TextOffset { get; }
        public uint RoOffset { get; }
        public uint DataOffset { get; }
        public uint BssOffset { get; }

        public uint TextSize { get; }
        public uint RoSize { get; }
        public uint DataSize { get; }
        public uint BssSize { get; }

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

            TextOffset = (uint)reader.Segments[0].MemoryOffset;
            RoOffset = (uint)reader.Segments[1].MemoryOffset;
            DataOffset = (uint)reader.Segments[2].MemoryOffset;
            BssOffset = (uint)reader.Segments[3].MemoryOffset;
            BssSize = (uint)reader.Segments[3].Size;

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

            reader.GetSegmentSize(KipReader.SegmentType.Data, out int uncompressedSize).ThrowIfFailure();
            Program = new byte[DataOffset + uncompressedSize];

            TextSize = DecompressSection(reader, KipReader.SegmentType.Text, TextOffset, Program);
            RoSize   = DecompressSection(reader, KipReader.SegmentType.Ro,   RoOffset,   Program);
            DataSize = DecompressSection(reader, KipReader.SegmentType.Data, DataOffset, Program);
        }

        private static uint DecompressSection(KipReader reader, KipReader.SegmentType segmentType, uint offset, byte[] program)
        {
            reader.GetSegmentSize(segmentType, out int uncompressedSize).ThrowIfFailure();

            var span = program.AsSpan().Slice((int)offset, uncompressedSize);

            reader.ReadSegment(segmentType, span).ThrowIfFailure();

            return (uint)uncompressedSize;
        }
    }
}