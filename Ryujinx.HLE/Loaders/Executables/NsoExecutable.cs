using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Loader;
using Ryujinx.Common.Logging;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ryujinx.HLE.Loaders.Executables
{
    class NsoExecutable : IExecutable
    {
        public byte[] Program { get; }
        public Span<byte> Text => Program.AsSpan().Slice((int)TextOffset, (int)TextSize);
        public Span<byte> Ro   => Program.AsSpan().Slice((int)RoOffset,   (int)RoSize);
        public Span<byte> Data => Program.AsSpan().Slice((int)DataOffset, (int)DataSize);

        public uint TextOffset { get; }
        public uint RoOffset { get; }
        public uint DataOffset { get; }
        public uint BssOffset => DataOffset + (uint)Data.Length;

        public uint TextSize { get; }
        public uint RoSize { get; }
        public uint DataSize { get; }
        public uint BssSize { get; }

        public string   Name;
        public Buffer32 BuildId;

        public NsoExecutable(IStorage inStorage, string name = null)
        {
            NsoReader reader = new NsoReader();

            reader.Initialize(inStorage.AsFile(OpenMode.Read)).ThrowIfFailure();

            TextOffset = reader.Header.Segments[0].MemoryOffset;
            RoOffset   = reader.Header.Segments[1].MemoryOffset;
            DataOffset = reader.Header.Segments[2].MemoryOffset;
            BssSize    = reader.Header.BssSize;

            reader.GetSegmentSize(NsoReader.SegmentType.Data, out uint uncompressedSize).ThrowIfFailure();

            Program = new byte[DataOffset + uncompressedSize];

            TextSize = DecompressSection(reader, NsoReader.SegmentType.Text, TextOffset);
            RoSize   = DecompressSection(reader, NsoReader.SegmentType.Ro,   RoOffset);
            DataSize = DecompressSection(reader, NsoReader.SegmentType.Data, DataOffset);

            Name    = name;
            BuildId = reader.Header.ModuleId;

            PrintRoSectionInfo();
        }

        private uint DecompressSection(NsoReader reader, NsoReader.SegmentType segmentType, uint offset)
        {
            reader.GetSegmentSize(segmentType, out uint uncompressedSize).ThrowIfFailure();

            var span = Program.AsSpan().Slice((int)offset, (int)uncompressedSize);

            reader.ReadSegment(segmentType, span).ThrowIfFailure();

            return uncompressedSize;
        }

        private void PrintRoSectionInfo()
        {
            byte[]        roBuffer      = Ro.ToArray();
            string        rawTextBuffer = Encoding.ASCII.GetString(roBuffer, 0, (int)RoSize);
            StringBuilder stringBuilder = new StringBuilder();

            int zero = BitConverter.ToInt32(roBuffer, 0);

            if (zero == 0)
            {
                int    length     = BitConverter.ToInt32(roBuffer, 4);
                string modulePath = Encoding.UTF8.GetString(roBuffer, 8, length);

                MatchCollection moduleMatches = Regex.Matches(rawTextBuffer, @"[a-z]:[\\/][ -~]{5,}\.nss", RegexOptions.IgnoreCase);
                if (moduleMatches.Count > 0)
                {
                    modulePath = moduleMatches.First().Value;
                }

                stringBuilder.AppendLine($"    Module: {modulePath}");
            }

            MatchCollection fsSdkMatches = Regex.Matches(rawTextBuffer, @"sdk_version: ([0-9.]*)");
            if (fsSdkMatches.Count != 0)
            {
                stringBuilder.AppendLine($"    FS SDK Version: {fsSdkMatches.First().Value.Replace("sdk_version: ", "")}");
            }

            MatchCollection sdkMwMatches = Regex.Matches(rawTextBuffer, @"SDK MW[ -~]*");
            if (sdkMwMatches.Count != 0)
            {
                string libHeader  = "    SDK Libraries: ";
                string libContent = string.Join($"\n{new string(' ', libHeader.Length)}", sdkMwMatches);

                stringBuilder.AppendLine($"{libHeader}{libContent}");
            }

            if (stringBuilder.Length > 0)
            {
                Logger.Info?.Print(LogClass.Loader, $"{Name}:\n{stringBuilder.ToString().TrimEnd('\r', '\n')}");
            }
        }
    }
}