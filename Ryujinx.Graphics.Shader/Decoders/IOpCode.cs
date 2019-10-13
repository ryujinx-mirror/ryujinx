using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCode
    {
        InstEmitter Emitter { get; }

        ulong Address   { get; }
        long  RawOpCode { get; }

        Register Predicate { get; }

        bool InvertPredicate { get; }
    }
}