namespace Ryujinx.Graphics.Gpu
{
    interface ISwizzle
    {
        int GetSwizzleOffset(int X, int Y);
    }
}