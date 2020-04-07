using LibHac;
using LibHac.Fs;
using System.IO;

namespace Ryujinx.HLE.Loaders.Executables
{
    class KipExecutable : Kip, IExecutable
    {
        public byte[] Text { get; }
        public byte[] Ro { get; }
        public byte[] Data { get; }

        public int TextOffset => Header.Sections[0].OutOffset;
        public int RoOffset => Header.Sections[1].OutOffset;
        public int DataOffset => Header.Sections[2].OutOffset;
        public int BssOffset => Header.Sections[3].OutOffset;
        public int BssSize => Header.Sections[3].DecompressedSize;

        public int[] Capabilities { get; }

        public KipExecutable(IStorage inStorage) : base(inStorage)
        {
            Capabilities = new int[32];

            for (int index = 0; index < Capabilities.Length; index++)
            {
                Capabilities[index] = System.BitConverter.ToInt32(Header.Capabilities, index * 4);
            }

            Text = DecompressSection(0);
            Ro = DecompressSection(1);
            Data = DecompressSection(2);
        }
    }
}