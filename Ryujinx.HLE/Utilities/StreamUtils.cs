using System.IO;

namespace Ryujinx.HLE.Utilities
{
    static class StreamUtils
    {
        public static byte[] StreamToBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
