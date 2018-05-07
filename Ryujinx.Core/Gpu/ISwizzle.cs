namespace Ryujinx.Core.Gpu
{
    interface ISwizzle
    {
        int GetSwizzleOffset(int X, int Y);
    }
}