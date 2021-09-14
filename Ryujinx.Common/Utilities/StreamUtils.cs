using System.IO;

namespace Ryujinx.Common.Utilities
{
    public static class StreamUtils
    {
        public static byte[] StreamToBytes(Stream input)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                input.CopyTo(stream);

                return stream.ToArray();
            }
        }
    }
}