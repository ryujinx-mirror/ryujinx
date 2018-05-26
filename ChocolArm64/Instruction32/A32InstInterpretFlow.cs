using ChocolArm64.Decoder;
using ChocolArm64.Decoder32;
using ChocolArm64.Memory;
using ChocolArm64.State;

using static ChocolArm64.Instruction32.A32InstInterpretHelper;

namespace ChocolArm64.Instruction32
{
    static partial class A32InstInterpret
    {
        public static void B(AThreadState State, AMemory Memory, AOpCode OpCode)
        {
            A32OpCodeBImmAl Op = (A32OpCodeBImmAl)OpCode;

            if (IsConditionTrue(State, Op.Cond))
            {
                BranchWritePc(State, GetPc(State) + (uint)Op.Imm);
            }
        }

        public static void Bl(AThreadState State, AMemory Memory, AOpCode OpCode)
        {
            Blx(State, Memory, OpCode, false);
        }

        public static void Blx(AThreadState State, AMemory Memory, AOpCode OpCode)
        {
            Blx(State, Memory, OpCode, true);
        }

        public static void Blx(AThreadState State, AMemory Memory, AOpCode OpCode, bool X)
        {
            A32OpCodeBImmAl Op = (A32OpCodeBImmAl)OpCode;

            if (IsConditionTrue(State, Op.Cond))
            {
                uint Pc = GetPc(State);

                if (State.Thumb)
                {
                    State.R14 = Pc | 1;
                }
                else
                {
                    State.R14 = Pc - 4U;
                }

                if (X)
                {
                    State.Thumb = !State.Thumb;
                }

                if (!State.Thumb)
                {
                    Pc &= ~3U;
                }

                BranchWritePc(State, Pc + (uint)Op.Imm);
            }
        }

        private static void BranchWritePc(AThreadState State, uint Pc)
        {
            State.R15 = State.Thumb
                ? Pc & ~1U
                : Pc & ~3U;
        }
    }
}