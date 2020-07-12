namespace Ryujinx.Graphics.Device
{
    public enum AccessControl
    {
        None      = 0,
        ReadOnly  = 1 << 0,
        WriteOnly = 1 << 1,
        ReadWrite = ReadOnly | WriteOnly
    }
}
