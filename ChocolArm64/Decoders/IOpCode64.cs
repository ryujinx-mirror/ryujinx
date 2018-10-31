using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    interface IOpCode64
    {
        long Position { get; }

        InstEmitter  Emitter      { get; }
        RegisterSize RegisterSize { get; }
    }
}