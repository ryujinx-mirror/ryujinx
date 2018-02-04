using System;

namespace ChocolArm64.Decoder
{
    static class ADecoderHelper
    {
        public struct BitMask
        {
            public long WMask;
            public long TMask;
            public int  Pos;
            public int  Shift;
            public bool IsUndefined;

            public static BitMask Invalid => new BitMask { IsUndefined = true };
        }

        public static BitMask DecodeBitMask(int OpCode, bool Immediate)
        {
            int ImmS = (OpCode >> 10) & 0x3f;
            int ImmR = (OpCode >> 16) & 0x3f;

            int N  = (OpCode >> 22) & 1;
            int SF = (OpCode >> 31) & 1;

            int Length = ABitUtils.HighestBitSet32((~ImmS & 0x3f) | (N << 6));

            if (Length < 1 || (SF == 0 && N != 0))
            {
                return BitMask.Invalid;
            }

            int Size = 1 << Length;

            int Levels = Size - 1;

            int S = ImmS & Levels;
            int R = ImmR & Levels;

            if (Immediate && S == Levels)
            {
                return BitMask.Invalid;
            }

            long WMask = ABitUtils.FillWithOnes(S + 1);
            long TMask = ABitUtils.FillWithOnes(((S - R) & Levels) + 1);

            if (R > 0)
            {
                WMask  = ABitUtils.RotateRight(WMask, R, Size);
                WMask &= ABitUtils.FillWithOnes(Size);
            }

            return new BitMask()
            {
                WMask = ABitUtils.Replicate(WMask, Size),
                TMask = ABitUtils.Replicate(TMask, Size),

                Pos   = ImmS,
                Shift = ImmR
            };
        }

        public static long DecodeImm8Float(long Imm, int Size)
        {
            int E = 0, F = 0;

            switch (Size)
            {
                case 0: E =  8; F = 23; break;
                case 1: E = 11; F = 52; break;

                default: throw new ArgumentOutOfRangeException(nameof(Size));
            }

            long Value = (Imm & 0x3f) << F - 4;

            long EBit = (Imm >> 6) & 1;
            long SBit = (Imm >> 7) & 1;

            if (EBit != 0)
            {
                Value |= (1L << E - 3) - 1 << F + 2;
            }

            Value |= (EBit ^ 1) << F + E - 1;
            Value |=  SBit      << F + E;

            return Value;
        }

        public static long DecodeImm26_2(int OpCode)
        {
            return ((long)OpCode << 38) >> 36;
        }

        public static long DecodeImmS19_2(int OpCode)
        {
            return (((long)OpCode << 40) >> 43) & ~3;
        }

        public static long DecodeImmS14_2(int OpCode)
        {
            return (((long)OpCode << 45) >> 48) & ~3;
        }
    }
}