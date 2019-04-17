namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeReg : IOpCode
    {
        Register Rb { get; }
    }
}