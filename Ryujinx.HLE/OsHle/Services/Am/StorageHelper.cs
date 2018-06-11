using System.IO;

namespace Ryujinx.HLE.OsHle.Services.Am
{
    class StorageHelper
    {
        private const uint LaunchParamsMagic = 0xc79497ca;

        public static byte[] MakeLaunchParams()
        {
            //Size needs to be at least 0x88 bytes otherwise application errors.
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                MS.SetLength(0x88);

                Writer.Write(LaunchParamsMagic);
                Writer.Write(1);  //IsAccountSelected? Only lower 8 bits actually used.
                Writer.Write(1L); //User Id Low (note: User Id needs to be != 0)
                Writer.Write(0L); //User Id High

                return MS.ToArray();
            }
        }
    }
}
