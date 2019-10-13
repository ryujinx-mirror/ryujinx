namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeImm : IOpCode
    {
        int Immediate { get; }
    }
}