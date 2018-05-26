using ChocolArm64.Decoder;
using ChocolArm64.State;
using System;

namespace ChocolArm64.Instruction32
{
    static class A32InstInterpretHelper
    {
        public static bool IsConditionTrue(AThreadState State, ACond Cond)
        {
            switch (Cond)
            {
                case ACond.Eq:    return  State.Zero;
                case ACond.Ne:    return !State.Zero;
                case ACond.Ge_Un: return  State.Carry;
                case ACond.Lt_Un: return !State.Carry;
                case ACond.Mi:    return  State.Negative;
                case ACond.Pl:    return !State.Negative;
                case ACond.Vs:    return  State.Overflow;
                case ACond.Vc:    return !State.Overflow;
                case ACond.Gt_Un: return  State.Carry    && !State.Zero;
                case ACond.Le_Un: return !State.Carry    &&  State.Zero;
                case ACond.Ge:    return  State.Negative ==  State.Overflow;
                case ACond.Lt:    return  State.Negative !=  State.Overflow;
                case ACond.Gt:    return  State.Negative ==  State.Overflow && !State.Zero;
                case ACond.Le:    return  State.Negative !=  State.Overflow &&  State.Zero;
            }

            return true;
        }

        public unsafe static uint GetReg(AThreadState State, int Reg)
        {
            if ((uint)Reg > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(Reg));
            }

            fixed (uint* Ptr = &State.R0)
            {
                return *(Ptr + Reg);
            }
        }

        public unsafe static void SetReg(AThreadState State, int Reg, uint Value)
        {
            if ((uint)Reg > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(Reg));
            }

            fixed (uint* Ptr = &State.R0)
            {
                *(Ptr + Reg) = Value;
            }
        }

        public static uint GetPc(AThreadState State)
        {
            //Due to the old fetch-decode-execute pipeline of old ARM CPUs,
            //the PC is 4 or 8 bytes (2 instructions) ahead of the current instruction.
            return State.R15 + (State.Thumb ? 2U : 4U);
        }
    }
}