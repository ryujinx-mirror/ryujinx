using Ryujinx.Common.Memory;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Common.Utilities
{
    public static class StreamUtils
    {
        public static byte[] StreamToBytes(Stream input)
        {
            using MemoryStream stream = MemoryStreamManager.Shared.GetStream();


            input.CopyTo(stream);

            return stream.ToArray();
        }

        public static async Task<byte[]> StreamToBytesAsync(Stream input, CancellationToken cancellationToken = default)
        {
            using MemoryStream stream = MemoryStreamManager.Shared.GetStream();

            await input.CopyToAsync(stream, cancellationToken);

            return stream.ToArray();
        }
    }
}
