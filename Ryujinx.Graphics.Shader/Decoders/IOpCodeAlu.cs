namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeAlu : IOpCodeRd, IOpCodeRa, IOpCodePredicate39
    {
        bool Extended    { get; }
        bool SetCondCode { get; }
        bool Saturate    { get; }
    }
}