namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeFArith : IOpCodeAlu
    {
        RoundingMode RoundingMode { get; }

        FPMultiplyScale Scale { get; }

        bool FlushToZero { get; }
        bool AbsoluteA   { get; }
    }
}