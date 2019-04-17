namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeFArith : IOpCodeAlu
    {
        RoundingMode RoundingMode { get; }

        FmulScale Scale { get; }

        bool FlushToZero { get; }
        bool AbsoluteA   { get; }
    }
}