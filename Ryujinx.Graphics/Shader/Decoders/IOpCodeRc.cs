namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeRc : IOpCode
    {
        Register Rc { get; }
    }
}