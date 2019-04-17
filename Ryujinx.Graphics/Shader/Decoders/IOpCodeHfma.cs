namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeHfma : IOpCode
    {
        bool NegateB  { get; }
        bool NegateC  { get; }
        bool Saturate { get; }

        FPHalfSwizzle SwizzleA { get; }
        FPHalfSwizzle SwizzleB { get; }
        FPHalfSwizzle SwizzleC { get; }
    }
}