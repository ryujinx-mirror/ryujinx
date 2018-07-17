using Ryujinx.HLE.Loaders.Compression;
using System;
using System.IO;

namespace Ryujinx.HLE.Loaders.Executables
{
    class Nso : IExecutable
    {
        public string FilePath { get; private set; }

        public byte[] Text { get; private set; }
        public byte[] RO   { get; private set; }
        public byte[] Data { get; private set; }

        public int Mod0Offset { get; private set; }
        public int TextOffset { get; private set; }
        public int ROOffset   { get; private set; }
        public int DataOffset { get; private set; }
        public int BssSize    { get; private set; }

        [Flags]
        private enum NsoFlags
        {
            IsTextCompressed = 1 << 0,
            IsROCompressed   = 1 << 1,
            IsDataCompressed = 1 << 2,
            HasTextHash      = 1 << 3,
            HasROHash        = 1 << 4,
            HasDataHash      = 1 << 5
        }

        public Nso(Stream Input, string FilePath)
        {
            this.FilePath = FilePath;

            BinaryReader Reader = new BinaryReader(Input);

            Input.Seek(0, SeekOrigin.Begin);

            int NsoMagic      = Reader.ReadInt32();
            int Version       = Reader.ReadInt32();
            int Reserved      = Reader.ReadInt32();
            int FlagsMsk      = Reader.ReadInt32();
            int TextOffset    = Reader.ReadInt32();
            int TextMemOffset = Reader.ReadInt32();
            int TextDecSize   = Reader.ReadInt32();
            int ModNameOffset = Reader.ReadInt32();
            int ROOffset      = Reader.ReadInt32();
            int ROMemOffset   = Reader.ReadInt32();
            int RODecSize     = Reader.ReadInt32();
            int ModNameSize   = Reader.ReadInt32();
            int DataOffset    = Reader.ReadInt32();
            int DataMemOffset = Reader.ReadInt32();
            int DataDecSize   = Reader.ReadInt32();
            int BssSize       = Reader.ReadInt32();

            byte[] BuildId = Reader.ReadBytes(0x20);

            int TextSize = Reader.ReadInt32();
            int ROSize   = Reader.ReadInt32();
            int DataSize = Reader.ReadInt32();

            Input.Seek(0x24, SeekOrigin.Current);

            int DynStrOffset = Reader.ReadInt32();
            int DynStrSize   = Reader.ReadInt32();
            int DynSymOffset = Reader.ReadInt32();
            int DynSymSize   = Reader.ReadInt32();

            byte[] TextHash = Reader.ReadBytes(0x20);
            byte[] ROHash   = Reader.ReadBytes(0x20);
            byte[] DataHash = Reader.ReadBytes(0x20);

            NsoFlags Flags = (NsoFlags)FlagsMsk;

            this.TextOffset = TextMemOffset;
            this.ROOffset   = ROMemOffset;
            this.DataOffset = DataMemOffset;
            this.BssSize    = BssSize;

            //Text segment
            Input.Seek(TextOffset, SeekOrigin.Begin);

            Text = Reader.ReadBytes(TextSize);

            if (Flags.HasFlag(NsoFlags.IsTextCompressed) || true)
            {
                Text = Lz4.Decompress(Text, TextDecSize);
            }

            //Read-only data segment
            Input.Seek(ROOffset, SeekOrigin.Begin);

            RO = Reader.ReadBytes(ROSize);

            if (Flags.HasFlag(NsoFlags.IsROCompressed) || true)
            {
                RO = Lz4.Decompress(RO, RODecSize);
            }

            //Data segment
            Input.Seek(DataOffset, SeekOrigin.Begin);

            Data = Reader.ReadBytes(DataSize);

            if (Flags.HasFlag(NsoFlags.IsDataCompressed) || true)
            {
                Data = Lz4.Decompress(Data, DataDecSize);
            }

            using (MemoryStream TextMS = new MemoryStream(Text))
            {
                BinaryReader TextReader = new BinaryReader(TextMS);

                TextMS.Seek(4, SeekOrigin.Begin);

                Mod0Offset = TextReader.ReadInt32();
            }
        }
    }
}