namespace Ryujinx.HLE.HOS.Services.Android
{
    enum ColorDataType
    {
        Integer = 0x0 << ColorShift.DataType,
        Float   = 0x1 << ColorShift.DataType,
        Stencil = 0x2 << ColorShift.DataType
    }
}