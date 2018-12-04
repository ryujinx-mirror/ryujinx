using System.IO;

namespace Ryujinx.HLE.Loaders.Executables
{
    class NxRelocatableObject : IExecutable
    {
        public byte[] Text { get; }
        public byte[] Ro   { get; }
        public byte[] Data { get; }

        public int Mod0Offset { get; }
        public int TextOffset { get; }
        public int RoOffset   { get; }
        public int DataOffset { get; }
        public int BssSize    { get; }

        public int BssOffset => DataOffset + Data.Length;

        public ulong SourceAddress { get; }
        public ulong BssAddress    { get; }

        public NxRelocatableObject(Stream input, ulong sourceAddress = 0, ulong bssAddress = 0)
        {
            SourceAddress = sourceAddress;
            BssAddress    = bssAddress;

            BinaryReader reader = new BinaryReader(input);

            input.Seek(4, SeekOrigin.Begin);

            int mod0Offset = reader.ReadInt32();
            int padding8   = reader.ReadInt32();
            int paddingC   = reader.ReadInt32();
            int nroMagic   = reader.ReadInt32();
            int unknown14  = reader.ReadInt32();
            int fileSize   = reader.ReadInt32();
            int unknown1C  = reader.ReadInt32();
            int textOffset = reader.ReadInt32();
            int textSize   = reader.ReadInt32();
            int roOffset   = reader.ReadInt32();
            int roSize     = reader.ReadInt32();
            int dataOffset = reader.ReadInt32();
            int dataSize   = reader.ReadInt32();
            int bssSize    = reader.ReadInt32();

            Mod0Offset = mod0Offset;
            TextOffset = textOffset;
            RoOffset   = roOffset;
            DataOffset = dataOffset;
            BssSize    = bssSize;

            byte[] Read(long position, int size)
            {
                input.Seek(position, SeekOrigin.Begin);

                return reader.ReadBytes(size);
            }

            Text = Read(textOffset, textSize);
            Ro   = Read(roOffset,   roSize);
            Data = Read(dataOffset, dataSize);
        }
    }
}