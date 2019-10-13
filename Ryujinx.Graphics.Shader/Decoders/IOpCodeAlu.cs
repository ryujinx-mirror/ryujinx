namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeAlu : IOpCodeRd, IOpCodeRa
    {
        Register Predicate39 { get; }

        bool InvertP     { get; }
        bool Extended    { get; }
        bool SetCondCode { get; }
        bool Saturate    { get; }
    }
}