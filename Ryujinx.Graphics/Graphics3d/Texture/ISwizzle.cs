namespace Ryujinx.Graphics.Texture
{
    interface ISwizzle
    {
        int GetSwizzleOffset(int x, int y, int z);

        void SetMipLevel(int level);

        int GetMipOffset(int level);

        int GetImageSize(int mipsCount);
    }
}