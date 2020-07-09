using LibHac;
using LibHac.Fs;
using System;

namespace Ryujinx.HLE.Loaders.Executables
{
    class NroExecutable : Nro, IExecutable
    {
        public byte[] Program { get; }
        public Span<byte> Text => Program.AsSpan().Slice(TextOffset, (int)Header.NroSegments[0].Size);
        public Span<byte> Ro   => Program.AsSpan().Slice(RoOffset,   (int)Header.NroSegments[1].Size);
        public Span<byte> Data => Program.AsSpan().Slice(DataOffset, (int)Header.NroSegments[2].Size);

        public int TextOffset => (int)Header.NroSegments[0].FileOffset;
        public int RoOffset   => (int)Header.NroSegments[1].FileOffset;
        public int DataOffset => (int)Header.NroSegments[2].FileOffset;
        public int BssOffset  => DataOffset + Data.Length;     
        public int BssSize    => (int)Header.BssSize;

        public int Mod0Offset => Start.Mod0Offset;
        public int FileSize   => (int)Header.Size;

        public ulong SourceAddress { get; private set; }
        public ulong BssAddress    { get; private set; }

        public NroExecutable(IStorage inStorage, ulong sourceAddress = 0, ulong bssAddress = 0) : base(inStorage)
        {
            Program = new byte[FileSize];

            SourceAddress = sourceAddress;
            BssAddress    = bssAddress;

            OpenNroSegment(NroSegmentType.Text, false).Read(0, Text);
            OpenNroSegment(NroSegmentType.Ro  , false).Read(0, Ro);
            OpenNroSegment(NroSegmentType.Data, false).Read(0, Data);
        }
    }
}