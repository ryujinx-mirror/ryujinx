using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class KernelAccessControl
    {
        public int[] Capabilities { get; private set; }

        public KernelAccessControl(Stream stream, int offset, int size)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            Capabilities = new int[size / 4];

            BinaryReader reader = new(stream);

            for (int index = 0; index < Capabilities.Length; index++)
            {
                Capabilities[index] = reader.ReadInt32();
            }
        }
    }
}
