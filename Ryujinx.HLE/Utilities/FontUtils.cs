using System.IO;

namespace Ryujinx.HLE.Utilities
{
    public static class FontUtils
    {
        private static readonly uint FontKey = 0x06186249;

        public static byte[] DecryptFont(Stream BFTTFStream)
        {
            uint KXor(uint In) => In ^ 0x06186249;

            using (BinaryReader Reader = new BinaryReader(BFTTFStream))
            {
                using (MemoryStream TTFStream = new MemoryStream())
                {
                    using (BinaryWriter Output = new BinaryWriter(TTFStream))
                    {
                        if (KXor(Reader.ReadUInt32()) != 0x18029a7f)
                        {
                            throw new InvalidDataException("Error: Input file is not in BFTTF format!");
                        }

                        BFTTFStream.Position += 4;

                        for (int i = 0; i < (BFTTFStream.Length - 8) / 4; i++)
                        {
                            Output.Write(KXor(Reader.ReadUInt32()));
                        }

                        return TTFStream.ToArray();
                    }
                }
            }
        }
    }
}
