namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeRegCbuf : IOpCodeRc
    {
        int Offset { get; }
        int Slot   { get; }
    }
}