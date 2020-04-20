using System.IO;

namespace Ryujinx.HLE.Utilities
{
    public static class FontUtils
    {
        private static readonly uint FontKey = 0x06186249;

        public static byte[] DecryptFont(Stream bfttfStream)
        {
            uint KXor(uint In) => In ^ FontKey;

            using (BinaryReader reader = new BinaryReader(bfttfStream))
            {
                using (MemoryStream ttfStream = new MemoryStream())
                {
                    using (BinaryWriter output = new BinaryWriter(ttfStream))
                    {
                        if (KXor(reader.ReadUInt32()) != 0x18029a7f)
                        {
                            throw new InvalidDataException("Error: Input file is not in BFTTF format!");
                        }

                        bfttfStream.Position += 4;

                        for (int i = 0; i < (bfttfStream.Length - 8) / 4; i++)
                        {
                            output.Write(KXor(reader.ReadUInt32()));
                        }

                        return ttfStream.ToArray();
                    }
                }
            }
        }
    }
}
