using ChocolArm64.Decoder;
using ChocolArm64.Memory;
using ChocolArm64.State;

namespace ChocolArm64.Instruction
{
    delegate void AInstInterpreter(AThreadState State, AMemory Memory, AOpCode OpCode);
}