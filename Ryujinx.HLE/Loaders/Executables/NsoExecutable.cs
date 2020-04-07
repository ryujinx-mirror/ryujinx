using LibHac;
using LibHac.Fs;
using System;
using System.IO;

namespace Ryujinx.HLE.Loaders.Executables
{
    class NsoExecutable : Nso, IExecutable
    {
        public byte[] Text { get; }
        public byte[] Ro { get; }
        public byte[] Data { get; }

        public int TextOffset => (int)Sections[0].MemoryOffset;
        public int RoOffset => (int)Sections[1].MemoryOffset;
        public int DataOffset => (int)Sections[2].MemoryOffset;
        public int BssOffset => DataOffset + Data.Length;

        public new int BssSize => (int)base.BssSize;

        public NsoExecutable(IStorage inStorage) : base(inStorage)
        {
            Text = Sections[0].DecompressSection();
            Ro = Sections[1].DecompressSection();
            Data =  Sections[2].DecompressSection();
        }
    }
}