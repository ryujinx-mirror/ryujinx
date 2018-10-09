using System.IO;

namespace Ryujinx.HLE.Loaders.Executables
{
    class Nro : IExecutable
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

        public long SourceAddress { get; private set; }
        public long BssAddress    { get; private set; }

        public Nro(Stream Input, string FilePath, long SourceAddress = 0, long BssAddress = 0)
        {
            this.FilePath      = FilePath;
            this.SourceAddress = SourceAddress;
            this.BssAddress    = BssAddress;

            BinaryReader Reader = new BinaryReader(Input);

            Input.Seek(4, SeekOrigin.Begin);

            int Mod0Offset = Reader.ReadInt32();
            int Padding8   = Reader.ReadInt32();
            int Paddingc   = Reader.ReadInt32();
            int NroMagic   = Reader.ReadInt32();
            int Unknown14  = Reader.ReadInt32();
            int FileSize   = Reader.ReadInt32();
            int Unknown1c  = Reader.ReadInt32();
            int TextOffset = Reader.ReadInt32();
            int TextSize   = Reader.ReadInt32();
            int ROOffset   = Reader.ReadInt32();
            int ROSize     = Reader.ReadInt32();
            int DataOffset = Reader.ReadInt32();
            int DataSize   = Reader.ReadInt32();
            int BssSize    = Reader.ReadInt32();

            this.Mod0Offset = Mod0Offset;
            this.TextOffset = TextOffset;
            this.ROOffset   = ROOffset;
            this.DataOffset = DataOffset;
            this.BssSize    = BssSize;

            byte[] Read(long Position, int Size)
            {
                Input.Seek(Position, SeekOrigin.Begin);

                return Reader.ReadBytes(Size);
            }

            Text = Read(TextOffset, TextSize);
            RO   = Read(ROOffset,   ROSize);
            Data = Read(DataOffset, DataSize);
        }
    }
}