namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeImmF : IOpCode
    {
        float Immediate { get; }
    }
}