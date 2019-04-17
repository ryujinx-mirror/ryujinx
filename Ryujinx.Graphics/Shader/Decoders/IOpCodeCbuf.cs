namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeCbuf : IOpCode
    {
        int Offset { get; }
        int Slot   { get; }
    }
}