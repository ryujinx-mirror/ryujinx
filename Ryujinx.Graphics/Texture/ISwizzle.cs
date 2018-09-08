namespace Ryujinx.Graphics.Texture
{
    interface ISwizzle
    {
        int GetSwizzleOffset(int X, int Y);
    }
}