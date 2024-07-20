using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class KernelAccessControl
    {
        public int[] Capabilities { get; private set; }

        /// <exception cref="System.ArgumentException">The stream does not support reading, is <see langword="null"/>, or is already closed.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
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
