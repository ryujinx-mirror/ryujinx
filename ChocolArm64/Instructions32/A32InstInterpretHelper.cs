using ChocolArm64.Decoders;
using ChocolArm64.State;
using System;

namespace ChocolArm64.Instructions32
{
    static class A32InstInterpretHelper
    {
        public static bool IsConditionTrue(CpuThreadState state, Cond cond)
        {
            switch (cond)
            {
                case Cond.Eq:   return  state.Zero;
                case Cond.Ne:   return !state.Zero;
                case Cond.GeUn: return  state.Carry;
                case Cond.LtUn: return !state.Carry;
                case Cond.Mi:   return  state.Negative;
                case Cond.Pl:   return !state.Negative;
                case Cond.Vs:   return  state.Overflow;
                case Cond.Vc:   return !state.Overflow;
                case Cond.GtUn: return  state.Carry    && !state.Zero;
                case Cond.LeUn: return !state.Carry    &&  state.Zero;
                case Cond.Ge:   return  state.Negative ==  state.Overflow;
                case Cond.Lt:   return  state.Negative !=  state.Overflow;
                case Cond.Gt:   return  state.Negative ==  state.Overflow && !state.Zero;
                case Cond.Le:   return  state.Negative !=  state.Overflow &&  state.Zero;
            }

            return true;
        }

        public unsafe static uint GetReg(CpuThreadState state, int reg)
        {
            if ((uint)reg > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(reg));
            }

            fixed (uint* ptr = &state.R0)
            {
                return *(ptr + reg);
            }
        }

        public unsafe static void SetReg(CpuThreadState state, int reg, uint value)
        {
            if ((uint)reg > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(reg));
            }

            fixed (uint* ptr = &state.R0)
            {
                *(ptr + reg) = value;
            }
        }

        public static uint GetPc(CpuThreadState state)
        {
            //Due to the old fetch-decode-execute pipeline of old ARM CPUs,
            //the PC is 4 or 8 bytes (2 instructions) ahead of the current instruction.
            return state.R15 + (state.Thumb ? 2U : 4U);
        }
    }
}