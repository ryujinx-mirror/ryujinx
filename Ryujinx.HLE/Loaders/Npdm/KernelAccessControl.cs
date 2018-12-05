using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class KernelAccessControl
    {
        public int[] Capabilities { get; private set; }

        public KernelAccessControl(Stream Stream, int Offset, int Size)
        {
            Stream.Seek(Offset, SeekOrigin.Begin);

            Capabilities = new int[Size / 4];

            BinaryReader Reader = new BinaryReader(Stream);

            for (int Index = 0; Index < Capabilities.Length; Index++)
            {
                Capabilities[Index] = Reader.ReadInt32();
            }
        }
    }
}
