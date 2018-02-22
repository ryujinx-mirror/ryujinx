using System.Collections.ObjectModel;

namespace Ryujinx.Core.Loaders.Executables
{
    public enum Extensions
    {
        NRO,
        NSO
    }

    public interface IExecutable
    {
        ReadOnlyCollection<byte> Text { get; }
        ReadOnlyCollection<byte> RO   { get; }
        ReadOnlyCollection<byte> Data { get; }

        int Mod0Offset { get; }
        int TextOffset { get; }
        int ROOffset   { get; }
        int DataOffset { get; }
        int BssSize    { get; }

        Extensions Extension  { get; }
    }
}