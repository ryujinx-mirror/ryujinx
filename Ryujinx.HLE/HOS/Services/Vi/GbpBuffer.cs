using System.IO;

namespace Ryujinx.HLE.HOS.Services.Android
{
    struct GbpBuffer
    {
        public int Magic  { get; }
        public int Width  { get; }
        public int Height { get; }
        public int Stride { get; }
        public int Format { get; }
        public int Usage  { get; }

        public int Pid      { get; }
        public int RefCount { get; }

        public int FdsCount  { get; }
        public int IntsCount { get; }

        public byte[] RawData { get; }

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