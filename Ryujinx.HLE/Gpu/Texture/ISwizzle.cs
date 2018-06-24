namespace Ryujinx.HLE.Gpu.Texture
{
    interface ISwizzle
    {
        int GetSwizzleOffset(int X, int Y);
    }
}