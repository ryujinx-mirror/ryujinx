using ChocolArm64.Decoders;
using ChocolArm64.Decoders32;
using ChocolArm64.Memory;
using ChocolArm64.State;

using static ChocolArm64.Instructions32.A32InstInterpretHelper;

namespace ChocolArm64.Instructions32
{
    static partial class A32InstInterpret
    {
        public static void B(CpuThreadState state, MemoryManager memory, OpCode64 opCode)
        {
            A32OpCodeBImmAl op = (A32OpCodeBImmAl)opCode;

            if (IsConditionTrue(state, op.Cond))
            {
                BranchWritePc(state, GetPc(state) + (uint)op.Imm);
            }
        }

        public static void Bl(CpuThreadState state, MemoryManager memory, OpCode64 opCode)
        {
            Blx(state, memory, opCode, false);
        }

        public static void Blx(CpuThreadState state, MemoryManager memory, OpCode64 opCode)
        {
            Blx(state, memory, opCode, true);
        }

        public static void Blx(CpuThreadState state, MemoryManager memory, OpCode64 opCode, bool x)
        {
            A32OpCodeBImmAl op = (A32OpCodeBImmAl)opCode;

            if (IsConditionTrue(state, op.Cond))
            {
                uint pc = GetPc(state);

                if (state.Thumb)
                {
                    state.R14 = pc | 1;
                }
                else
                {
                    state.R14 = pc - 4U;
                }

                if (x)
                {
                    state.Thumb = !state.Thumb;
                }

                if (!state.Thumb)
                {
                    pc &= ~3U;
                }

                BranchWritePc(state, pc + (uint)op.Imm);
            }
        }

        private static void BranchWritePc(CpuThreadState state, uint pc)
        {
            state.R15 = state.Thumb
                ? pc & ~1U
                : pc & ~3U;
        }
    }
}