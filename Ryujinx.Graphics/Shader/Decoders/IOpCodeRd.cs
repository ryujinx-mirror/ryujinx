namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeRd : IOpCode
    {
        Register Rd { get; }
    }
}