namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeLop : IOpCodeAlu
    {
        LogicalOperation LogicalOp { get; }

        bool InvertA { get; }
        bool InvertB { get; }
    }
}