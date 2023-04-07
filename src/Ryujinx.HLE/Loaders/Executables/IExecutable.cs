using System;

namespace Ryujinx.HLE.Loaders.Executables
{
    interface IExecutable
    {
        byte[] Program { get; }
        Span<byte> Text { get; }
        Span<byte> Ro   { get; }
        Span<byte> Data { get; }

        uint TextOffset { get; }
        uint RoOffset   { get; }
        uint DataOffset { get; }
        uint BssOffset  { get; }
        uint BssSize    { get; }
    }
}