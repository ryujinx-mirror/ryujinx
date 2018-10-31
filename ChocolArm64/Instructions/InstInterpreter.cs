using ChocolArm64.Decoders;
using ChocolArm64.Memory;
using ChocolArm64.State;

namespace ChocolArm64.Instructions
{
    delegate void InstInterpreter(CpuThreadState state, MemoryManager memory, OpCode64 opCode);
}