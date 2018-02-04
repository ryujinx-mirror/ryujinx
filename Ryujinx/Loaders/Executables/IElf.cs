using System.Collections.ObjectModel;

namespace Ryujinx.Loaders.Executables
{
    interface IElf
    {
        ReadOnlyCollection<byte> Text { get; }
        ReadOnlyCollection<byte> RO   { get; }
        ReadOnlyCollection<byte> Data { get; }

        int Mod0Offset { get; }
        int TextOffset { get; }
        int ROOffset   { get; }
        int DataOffset { get; }
        int BssSize    { get; }
    }
}