using System;

namespace Ryujinx.HLE.Loaders.Executables
{
    interface IExecutable
    {
        byte[] Program { get; }        
        Span<byte> Text { get; }
        Span<byte> Ro   { get; }
        Span<byte> Data { get; }

        int TextOffset { get; }
        int RoOffset   { get; }
        int DataOffset { get; }
        int BssOffset  { get; }
        int BssSize    { get; }
    }
}