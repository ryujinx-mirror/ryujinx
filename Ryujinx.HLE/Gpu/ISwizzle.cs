namespace Ryujinx.HLE.Gpu
{
    interface ISwizzle
    {
        int GetSwizzleOffset(int X, int Y);
    }
}