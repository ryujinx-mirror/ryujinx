namespace Ryujinx.Graphics.Texture
{
    interface ISwizzle
    {
        int GetSwizzleOffset(int X, int Y, int Z);

        void SetMipLevel(int Level);

        int GetMipOffset(int Level);

        int GetImageSize(int MipsCount);
    }
}