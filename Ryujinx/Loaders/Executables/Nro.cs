using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Ryujinx.Loaders.Executables
{
    class Nro : IElf
    {
        private byte[] m_Text;
        private byte[] m_RO;
        private byte[] m_Data;

        public ReadOnlyCollection<byte> Text => Array.AsReadOnly(m_Text);
        public ReadOnlyCollection<byte> RO   => Array.AsReadOnly(m_RO);
        public ReadOnlyCollection<byte> Data => Array.AsReadOnly(m_Data);

        public int Mod0Offset { get; private set; }
        public int TextOffset { get; private set; }
        public int ROOffset   { get; private set; }
        public int DataOffset { get; private set; }
        public int BssSize    { get; private set; }

        public Nro(Stream Input)
        {
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

            m_Text = Read(TextOffset, TextSize);
            m_RO   = Read(ROOffset,   ROSize);
            m_Data = Read(DataOffset, DataSize);
        }
    }
}