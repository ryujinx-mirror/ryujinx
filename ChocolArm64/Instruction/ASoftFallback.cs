using ChocolArm64.Translation;
using System;

namespace ChocolArm64.Instruction
{
    static class ASoftFallback
    {
        public static void EmitCall(AILEmitterCtx Context, string MthdName)
        {
            Context.EmitCall(typeof(ASoftFallback), MthdName);
        }

        public static ulong CountLeadingSigns(ulong Value, int Size)
        {
            return CountLeadingZeros((Value >> 1) ^ Value, Size - 1);
        }

        public static ulong CountLeadingZeros(ulong Value, int Size)
        {
            int HighBit = Size - 1;

            for (int Bit = HighBit; Bit >= 0; Bit--)
            {
                if (((Value >> Bit) & 1) != 0)
                {
                    return (ulong)(HighBit - Bit);
                }
            }

            return (ulong)Size;
        }

        private const uint Crc32RevPoly  = 0xedb88320;
        private const uint Crc32cRevPoly = 0x82f63b78;

        public static uint Crc32b(uint Crc, byte   Val) => Crc32 (Crc, Crc32RevPoly, Val);
        public static uint Crc32h(uint Crc, ushort Val) => Crc32h(Crc, Crc32RevPoly, Val);
        public static uint Crc32w(uint Crc, uint   Val) => Crc32w(Crc, Crc32RevPoly, Val);
        public static uint Crc32x(uint Crc, ulong  Val) => Crc32x(Crc, Crc32RevPoly, Val);

        public static uint Crc32cb(uint Crc, byte   Val) => Crc32 (Crc, Crc32cRevPoly, Val);
        public static uint Crc32ch(uint Crc, ushort Val) => Crc32h(Crc, Crc32cRevPoly, Val);
        public static uint Crc32cw(uint Crc, uint   Val) => Crc32w(Crc, Crc32cRevPoly, Val);
        public static uint Crc32cx(uint Crc, ulong  Val) => Crc32x(Crc, Crc32cRevPoly, Val);

        private static uint Crc32h(uint Crc, uint Poly, ushort Val)
        {
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8));

            return Crc;
        }

        private static uint Crc32w(uint Crc, uint Poly, uint Val)
        {
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 16));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 24));

            return Crc;
        }

        private static uint Crc32x(uint Crc, uint Poly, ulong Val)
        {
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 16));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 24));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 32));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 40));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 48));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 56));

            return Crc;
        }

        private static uint Crc32(uint Crc, uint Poly, byte Val)
        {
            Crc ^= Val;

            for (int Bit = 7; Bit >= 0; Bit--)
            {
                uint Mask = (uint)(-(int)(Crc & 1));

                Crc = (Crc >> 1) ^ (Poly & Mask);
            }

            return Crc;
        }

        public static uint ReverseBits32(uint Value)
        {
            Value = ((Value & 0xaaaaaaaa) >> 1) | ((Value & 0x55555555) << 1);
            Value = ((Value & 0xcccccccc) >> 2) | ((Value & 0x33333333) << 2);
            Value = ((Value & 0xf0f0f0f0) >> 4) | ((Value & 0x0f0f0f0f) << 4);
            Value = ((Value & 0xff00ff00) >> 8) | ((Value & 0x00ff00ff) << 8);

            return (Value >> 16) | (Value << 16);
        }

        public static ulong ReverseBits64(ulong Value)
        {
            Value = ((Value & 0xaaaaaaaaaaaaaaaa) >>  1) | ((Value & 0x5555555555555555) <<  1);
            Value = ((Value & 0xcccccccccccccccc) >>  2) | ((Value & 0x3333333333333333) <<  2);
            Value = ((Value & 0xf0f0f0f0f0f0f0f0) >>  4) | ((Value & 0x0f0f0f0f0f0f0f0f) <<  4);
            Value = ((Value & 0xff00ff00ff00ff00) >>  8) | ((Value & 0x00ff00ff00ff00ff) <<  8);
            Value = ((Value & 0xffff0000ffff0000) >> 16) | ((Value & 0x0000ffff0000ffff) << 16);

            return (Value >> 32) | (Value << 32);
        }

        public static uint ReverseBytes16_32(uint Value) => (uint)ReverseBytes16_64(Value);
        public static uint ReverseBytes32_32(uint Value) => (uint)ReverseBytes32_64(Value);

        public static ulong ReverseBytes16_64(ulong Value) => ReverseBytes(Value, RevSize.Rev16);
        public static ulong ReverseBytes32_64(ulong Value) => ReverseBytes(Value, RevSize.Rev32);
        public static ulong ReverseBytes64(ulong Value)    => ReverseBytes(Value, RevSize.Rev64);

        private enum RevSize
        {
            Rev16,
            Rev32,
            Rev64
        }

        private static ulong ReverseBytes(ulong Value, RevSize Size)
        {
            Value = ((Value & 0xff00ff00ff00ff00) >> 8) | ((Value & 0x00ff00ff00ff00ff) << 8);

            if (Size == RevSize.Rev16)
            {
                return Value;
            }

            Value = ((Value & 0xffff0000ffff0000) >> 16) | ((Value & 0x0000ffff0000ffff) << 16);

            if (Size == RevSize.Rev32)
            {
                return Value;
            }

            Value = ((Value & 0xffffffff00000000) >> 32) | ((Value & 0x00000000ffffffff) << 32);

            if (Size == RevSize.Rev64)
            {
                return Value;
            }

            throw new ArgumentException(nameof(Size));
        }

        public static long SMulHi128(long LHS, long RHS)
        {
            long Result = (long)UMulHi128((ulong)(LHS), (ulong)(RHS));
            if (LHS < 0) Result -= RHS;
            if (RHS < 0) Result -= LHS;
            return Result;
        }

        public static ulong UMulHi128(ulong LHS, ulong RHS)
        {
            //long multiplication
            //multiply 32 bits at a time in 64 bit, the result is what's carried over 64 bits.
            ulong LHigh = LHS >> 32;
            ulong LLow = LHS & 0xFFFFFFFF;
            ulong RHigh = RHS >> 32;
            ulong RLow = RHS & 0xFFFFFFFF;
            ulong Z2 = LLow * RLow;
            ulong T = LHigh * RLow + (Z2 >> 32);
            ulong Z1 = T & 0xFFFFFFFF;
            ulong Z0 = T >> 32;
            Z1 += LLow * RHigh;
            return LHigh * RHigh + Z0 + (Z1 >> 32);
        }
    }
}
