using System.IO;

namespace Ryujinx.HLE.HOS.Services.Android
{
    struct GbpBuffer
    {
        public int Magic  { get; private set; }
        public int Width  { get; private set; }
        public int Height { get; private set; }
        public int Stride { get; private set; }
        public int Format { get; private set; }
        public int Usage  { get; private set; }

        public int Pid      { get; private set; }
        public int RefCount { get; private set; }

        public int FdsCount  { get; private set; }
        public int IntsCount { get; private set; }

        public byte[] RawData { get; private set; }

        public int Size => RawData.Length + 10 * 4;

        public GbpBuffer(BinaryReader Reader)
        {
            Magic  = Reader.ReadInt32();
            Width  = Reader.ReadInt32();
            Height = Reader.ReadInt32();
            Stride = Reader.ReadInt32();
            Format = Reader.ReadInt32();
            Usage  = Reader.ReadInt32();

            Pid      = Reader.ReadInt32();
            RefCount = Reader.ReadInt32();

            FdsCount  = Reader.ReadInt32();
            IntsCount = Reader.ReadInt32();

            RawData = Reader.ReadBytes((FdsCount + IntsCount) * 4);
        }

        public void Write(BinaryWriter Writer)
        {
            Writer.Write(Magic);
            Writer.Write(Width);
            Writer.Write(Height);
            Writer.Write(Stride);
            Writer.Write(Format);
            Writer.Write(Usage);

            Writer.Write(Pid);
            Writer.Write(RefCount);

            Writer.Write(FdsCount);
            Writer.Write(IntsCount);

            Writer.Write(RawData);
        }
    }
}