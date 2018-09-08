namespace Ryujinx.Graphics.Texture
{
    static class ImageConverter
    {
        public static byte[] G8R8ToR8G8(
            byte[] Data,
            int    Width,
            int    Height,
            int    Depth)
        {
            int Texels = Width * Height * Depth;

            byte[] Output = new byte[Texels * 2];

            for (int Texel = 0; Texel < Texels; Texel++)
            {
                Output[Texel * 2 + 0] = Data[Texel * 2 + 1];
                Output[Texel * 2 + 1] = Data[Texel * 2 + 0];
            }

            return Output;
        }
    }
}