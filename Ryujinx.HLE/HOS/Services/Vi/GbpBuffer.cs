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

        public GbpBuffer(BinaryReader reader)
        {
            Magic  = reader.ReadInt32();
            Width  = reader.ReadInt32();
            Height = reader.ReadInt32();
            Stride = reader.ReadInt32();
            Format = reader.ReadInt32();
            Usage  = reader.ReadInt32();

            Pid      = reader.ReadInt32();
            RefCount = reader.ReadInt32();

            FdsCount  = reader.ReadInt32();
            IntsCount = reader.ReadInt32();

            RawData = reader.ReadBytes((FdsCount + IntsCount) * 4);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Stride);
            writer.Write(Format);
            writer.Write(Usage);

            writer.Write(Pid);
            writer.Write(RefCount);

            writer.Write(FdsCount);
            writer.Write(IntsCount);

            writer.Write(RawData);
        }
    }
}