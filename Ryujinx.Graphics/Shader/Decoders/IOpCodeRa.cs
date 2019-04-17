namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeRa : IOpCode
    {
        Register Ra { get; }
    }
}