namespace Ryujinx.HLE.Loaders.Executables
{
    public interface IExecutable
    {
        string FilePath { get; }

        byte[] Text { get; }
        byte[] RO   { get; }
        byte[] Data { get; }

        long SourceAddress { get; }
        long BssAddress    { get; }

        int Mod0Offset { get; }
        int TextOffset { get; }
        int ROOffset   { get; }
        int DataOffset { get; }
        int BssSize    { get; }
    }
}