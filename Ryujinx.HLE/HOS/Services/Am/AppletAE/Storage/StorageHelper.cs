using System.IO;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.Storage
{
    class StorageHelper
    {
        private const uint LaunchParamsMagic = 0xc79497ca;

        public static byte[] MakeLaunchParams()
        {
            // Size needs to be at least 0x88 bytes otherwise application errors.
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                ms.SetLength(0x88);

                writer.Write(LaunchParamsMagic);
                writer.Write(1);  // IsAccountSelected? Only lower 8 bits actually used.
                writer.Write(1L); // User Id Low (note: User Id needs to be != 0)
                writer.Write(0L); // User Id High

                return ms.ToArray();
            }
        }
    }
}
