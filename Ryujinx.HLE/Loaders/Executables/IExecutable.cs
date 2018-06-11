namespace Ryujinx.HLE.Loaders.Executables
{
    public interface IExecutable
    {
        string Name { get; }

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