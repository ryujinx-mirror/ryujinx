using Ryujinx.Graphics.Gal;
using System.IO;

namespace Ryushader
{
    class Memory : IGalMemory
    {
        private Stream BaseStream;

        private BinaryReader Reader;

        public Memory(Stream BaseStream)
        {
            this.BaseStream = BaseStream;

            Reader = new BinaryReader(BaseStream);
        }

        public int ReadInt32(long Position)
        {
            BaseStream.Seek(Position, SeekOrigin.Begin);

            return Reader.ReadInt32();
        }
    }
}
