namespace Ryujinx.Core.Loaders.Executables
{
    public interface IExecutable
    {
        byte[] Text { get; }
        byte[] RO   { get; }
        byte[] Data { get; }

        int Mod0Offset { get; }
        int TextOffset { get; }
        int ROOffset   { get; }
        int DataOffset { get; }
        int BssSize    { get; }
    }
}