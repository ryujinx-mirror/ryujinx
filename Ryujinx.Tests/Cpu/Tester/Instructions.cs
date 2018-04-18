// https://github.com/LDj3SNuD/ARM_v8-A_AArch64_Instructions_Tester/blob/master/Tester/Instructions.cs

// https://meriac.github.io/archex/A64_v83A_ISA/index.xml
/* https://meriac.github.io/archex/A64_v83A_ISA/fpsimdindex.xml */

using System.Numerics;

namespace Ryujinx.Tests.Cpu.Tester
{
    using Types;

    using static AArch64;
    using static Shared;

    internal static class Base
    {
#region "Alu"
        // https://meriac.github.io/archex/A64_v83A_ISA/cls_int.xml
        public static void Cls(bool sf, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits operand1 = X(datasize, n);

            BigInteger result = (BigInteger)CountLeadingSignBits(operand1);

            X(d, result.SubBigInteger(datasize - 1, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/clz_int.xml
        public static void Clz(bool sf, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits operand1 = X(datasize, n);

            BigInteger result = (BigInteger)CountLeadingZeroBits(operand1);

            X(d, result.SubBigInteger(datasize - 1, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/rbit_int.xml
        public static void Rbit(bool sf, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result = new Bits(datasize);
            Bits operand = X(datasize, n);

            for (int i = 0; i <= datasize - 1; i++)
            {
                result[datasize - 1 - i] = operand[i];
            }

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/rev16_int.xml
        public static void Rev16(bool sf, Bits Rn, Bits Rd)
        {
            /* Bits opc = "01"; */

            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            int container_size = 16;

            /* Operation */
            Bits result = new Bits(datasize);
            Bits operand = X(datasize, n);

            int containers = datasize / container_size;
            int elements_per_container = container_size / 8;
            int index = 0;
            int rev_index;

            for (int c = 0; c <= containers - 1; c++)
            {
                rev_index = index + ((elements_per_container - 1) * 8);

                for (int e = 0; e <= elements_per_container - 1; e++)
                {
                    result[rev_index + 7, rev_index] = operand[index + 7, index];

                    index = index + 8;
                    rev_index = rev_index - 8;
                }
            }

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/rev32_int.xml
        // (https://meriac.github.io/archex/A64_v83A_ISA/rev.xml)
        public static void Rev32(bool sf, Bits Rn, Bits Rd)
        {
            /* Bits opc = "10"; */

            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            int container_size = 32;

            /* Operation */
            Bits result = new Bits(datasize);
            Bits operand = X(datasize, n);

            int containers = datasize / container_size;
            int elements_per_container = container_size / 8;
            int index = 0;
            int rev_index;

            for (int c = 0; c <= containers - 1; c++)
            {
                rev_index = index + ((elements_per_container - 1) * 8);

                for (int e = 0; e <= elements_per_container - 1; e++)
                {
                    result[rev_index + 7, rev_index] = operand[index + 7, index];

                    index = index + 8;
                    rev_index = rev_index - 8;
                }
            }

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/rev64_rev.xml
        // (https://meriac.github.io/archex/A64_v83A_ISA/rev.xml)
        public static void Rev64(Bits Rn, Bits Rd)
        {
            /* Bits opc = "11"; */

            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);

            int container_size = 64;

            /* Operation */
            Bits result = new Bits(64);
            Bits operand = X(64, n);

            int containers = 64 / container_size;
            int elements_per_container = container_size / 8;
            int index = 0;
            int rev_index;

            for (int c = 0; c <= containers - 1; c++)
            {
                rev_index = index + ((elements_per_container - 1) * 8);

                for (int e = 0; e <= elements_per_container - 1; e++)
                {
                    result[rev_index + 7, rev_index] = operand[index + 7, index];

                    index = index + 8;
                    rev_index = rev_index - 8;
                }
            }

            X(d, result);
        }
#endregion

#region "AluImm"
        // https://meriac.github.io/archex/A64_v83A_ISA/add_addsub_imm.xml
        public static void Add_Imm(bool sf, Bits shift, Bits imm12, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            switch (shift)
            {
                default:
                case Bits bits when bits == "00":
                    imm = ZeroExtend(imm12, datasize);
                    break;
                case Bits bits when bits == "01":
                    imm = ZeroExtend(Bits.Concat(imm12, Zeros(12)), datasize);
                    break;
                /* when '1x' ReservedValue(); */
            }

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));

            (result, _) = AddWithCarry(datasize, operand1, imm, false);

            if (d == 31)
            {
                SP(result);
            }
            else
            {
                X(d, result);
            }
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/adds_addsub_imm.xml
        public static void Adds_Imm(bool sf, Bits shift, Bits imm12, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            switch (shift)
            {
                default:
                case Bits bits when bits == "00":
                    imm = ZeroExtend(imm12, datasize);
                    break;
                case Bits bits when bits == "01":
                    imm = ZeroExtend(Bits.Concat(imm12, Zeros(12)), datasize);
                    break;
                /* when '1x' ReservedValue(); */
            }

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));
            Bits nzcv;

            (result, nzcv) = AddWithCarry(datasize, operand1, imm, false);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/and_log_imm.xml
        public static void And_Imm(bool sf, bool N, Bits immr, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            /* if sf == '0' && N != '0' then ReservedValue(); */

            (imm, _) = DecodeBitMasks(datasize, N, imms, immr, true);

            /* Operation */
            Bits operand1 = X(datasize, n);

            Bits result = AND(operand1, imm);

            if (d == 31)
            {
                SP(result);
            }
            else
            {
                X(d, result);
            }
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/ands_log_imm.xml
        public static void Ands_Imm(bool sf, bool N, Bits immr, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            /* if sf == '0' && N != '0' then ReservedValue(); */

            (imm, _) = DecodeBitMasks(datasize, N, imms, immr, true);

            /* Operation */
            Bits operand1 = X(datasize, n);

            Bits result = AND(operand1, imm);

            PSTATE.NZCV(result[datasize - 1], IsZeroBit(result), false, false);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/eor_log_imm.xml
        public static void Eor_Imm(bool sf, bool N, Bits immr, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            /* if sf == '0' && N != '0' then ReservedValue(); */

            (imm, _) = DecodeBitMasks(datasize, N, imms, immr, true);

            /* Operation */
            Bits operand1 = X(datasize, n);

            Bits result = EOR(operand1, imm);

            if (d == 31)
            {
                SP(result);
            }
            else
            {
                X(d, result);
            }
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/orr_log_imm.xml
        public static void Orr_Imm(bool sf, bool N, Bits immr, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            /* if sf == '0' && N != '0' then ReservedValue(); */

            (imm, _) = DecodeBitMasks(datasize, N, imms, immr, true);

            /* Operation */
            Bits operand1 = X(datasize, n);

            Bits result = OR(operand1, imm);

            if (d == 31)
            {
                SP(result);
            }
            else
            {
                X(d, result);
            }
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/sub_addsub_imm.xml
        public static void Sub_Imm(bool sf, Bits shift, Bits imm12, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            switch (shift)
            {
                default:
                case Bits bits when bits == "00":
                    imm = ZeroExtend(imm12, datasize);
                    break;
                case Bits bits when bits == "01":
                    imm = ZeroExtend(Bits.Concat(imm12, Zeros(12)), datasize);
                    break;
                /* when '1x' ReservedValue(); */
            }

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));
            Bits operand2 = NOT(imm);

            (result, _) = AddWithCarry(datasize, operand1, operand2, true);

            if (d == 31)
            {
                SP(result);
            }
            else
            {
                X(d, result);
            }
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/subs_addsub_imm.xml
        public static void Subs_Imm(bool sf, Bits shift, Bits imm12, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits imm;

            switch (shift)
            {
                default:
                case Bits bits when bits == "00":
                    imm = ZeroExtend(imm12, datasize);
                    break;
                case Bits bits when bits == "01":
                    imm = ZeroExtend(Bits.Concat(imm12, Zeros(12)), datasize);
                    break;
                /* when '1x' ReservedValue(); */
            }

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));
            Bits operand2 = NOT(imm);
            Bits nzcv;

            (result, nzcv) = AddWithCarry(datasize, operand1, operand2, true);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }
#endregion

#region "AluRs"
        // https://meriac.github.io/archex/A64_v83A_ISA/adc.xml
        public static void Adc(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            (result, _) = AddWithCarry(datasize, operand1, operand2, PSTATE.C);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/adcs.xml
        public static void Adcs(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);
            Bits nzcv;

            (result, nzcv) = AddWithCarry(datasize, operand1, operand2, PSTATE.C);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/add_addsub_shift.xml
        public static void Add_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if shift == '11' then ReservedValue(); */
            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            (result, _) = AddWithCarry(datasize, operand1, operand2, false);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/adds_addsub_shift.xml
        public static void Adds_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if shift == '11' then ReservedValue(); */
            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);
            Bits nzcv;

            (result, nzcv) = AddWithCarry(datasize, operand1, operand2, false);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/and_log_shift.xml
        public static void And_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            Bits result = AND(operand1, operand2);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/ands_log_shift.xml
        public static void Ands_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            Bits result = AND(operand1, operand2);

            PSTATE.NZCV(result[datasize - 1], IsZeroBit(result), false, false);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/asrv.xml
        public static void Asrv(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            Bits op2 = "10";

            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ShiftType shift_type = DecodeShift(op2);

            /* Operation */
            Bits operand2 = X(datasize, m);

            Bits result = ShiftReg(datasize, n, shift_type, (int)(UInt(operand2) % datasize)); // BigInteger.Modulus Operator (BigInteger, BigInteger)

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/bic_log_shift.xml
        public static void Bic(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            operand2 = NOT(operand2);

            Bits result = AND(operand1, operand2);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/bics.xml
        public static void Bics(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            operand2 = NOT(operand2);

            Bits result = AND(operand1, operand2);

            PSTATE.NZCV(result[datasize - 1], IsZeroBit(result), false, false);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/crc32.xml
        public static void Crc32(bool sf, Bits Rm, Bits sz, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);

            /* if sf == '1' && sz != '11' then UnallocatedEncoding(); */
            /* if sf == '0' && sz == '11' then UnallocatedEncoding(); */

            int size = 8 << (int)UInt(sz);

            /* Operation */
            /* if !HaveCRCExt() then UnallocatedEncoding(); */

            Bits acc = X(32, n); // accumulator
            Bits val = X(size, m); // input value
            Bits poly = new Bits(0x04C11DB7u);

            Bits tempacc = Bits.Concat(BitReverse(acc), Zeros(size));
            Bits tempval = Bits.Concat(BitReverse(val), Zeros(32));

            // Poly32Mod2 on a bitstring does a polynomial Modulus over {0,1} operation
            X(d, BitReverse(Poly32Mod2(EOR(tempacc, tempval), poly)));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/crc32c.xml
        public static void Crc32c(bool sf, Bits Rm, Bits sz, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);

            /* if sf == '1' && sz != '11' then UnallocatedEncoding(); */
            /* if sf == '0' && sz == '11' then UnallocatedEncoding(); */

            int size = 8 << (int)UInt(sz);

            /* Operation */
            /* if !HaveCRCExt() then UnallocatedEncoding(); */

            Bits acc = X(32, n); // accumulator
            Bits val = X(size, m); // input value
            Bits poly = new Bits(0x1EDC6F41u);

            Bits tempacc = Bits.Concat(BitReverse(acc), Zeros(size));
            Bits tempval = Bits.Concat(BitReverse(val), Zeros(32));

            // Poly32Mod2 on a bitstring does a polynomial Modulus over {0,1} operation
            X(d, BitReverse(Poly32Mod2(EOR(tempacc, tempval), poly)));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/eon.xml
        public static void Eon(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            operand2 = NOT(operand2);

            Bits result = EOR(operand1, operand2);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/eor_log_shift.xml
        public static void Eor_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            Bits result = EOR(operand1, operand2);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/extr.xml
        public static void Extr(bool sf, bool N, Bits Rm, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if N != sf then UnallocatedEncoding(); */
            /* if sf == '0' && imms<5> == '1' then ReservedValue(); */

            int lsb = (int)UInt(imms);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);
            Bits concat = Bits.Concat(operand1, operand2);

            Bits result = concat[lsb + datasize - 1, lsb];

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/lslv.xml
        public static void Lslv(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            Bits op2 = "00";

            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ShiftType shift_type = DecodeShift(op2);

            /* Operation */
            Bits operand2 = X(datasize, m);

            Bits result = ShiftReg(datasize, n, shift_type, (int)(UInt(operand2) % datasize)); // BigInteger.Modulus Operator (BigInteger, BigInteger)

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/lsrv.xml
        public static void Lsrv(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            Bits op2 = "01";

            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ShiftType shift_type = DecodeShift(op2);

            /* Operation */
            Bits operand2 = X(datasize, m);

            Bits result = ShiftReg(datasize, n, shift_type, (int)(UInt(operand2) % datasize)); // BigInteger.Modulus Operator (BigInteger, BigInteger)

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/orn_log_shift.xml
        public static void Orn(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            operand2 = NOT(operand2);

            Bits result = OR(operand1, operand2);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/orr_log_shift.xml
        public static void Orr_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            Bits result = OR(operand1, operand2);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/rorv.xml
        public static void Rorv(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            Bits op2 = "11";

            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ShiftType shift_type = DecodeShift(op2);

            /* Operation */
            Bits operand2 = X(datasize, m);

            Bits result = ShiftReg(datasize, n, shift_type, (int)(UInt(operand2) % datasize)); // BigInteger.Modulus Operator (BigInteger, BigInteger)

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/sbc.xml
        public static void Sbc(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            operand2 = NOT(operand2);

            (result, _) = AddWithCarry(datasize, operand1, operand2, PSTATE.C);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/sbcs.xml
        public static void Sbcs(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);
            Bits nzcv;

            operand2 = NOT(operand2);

            (result, nzcv) = AddWithCarry(datasize, operand1, operand2, PSTATE.C);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/sdiv.xml
        public static void Sdiv(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            BigInteger result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (IsZero(operand2))
            {
                result = (BigInteger)0m;
            }
            else
            {
                result = RoundTowardsZero(Real(Int(operand1, false)) / Real(Int(operand2, false)));
            }

            X(d, result.SubBigInteger(datasize - 1, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/sub_addsub_shift.xml
        public static void Sub_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if shift == '11' then ReservedValue(); */
            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);

            operand2 = NOT(operand2);

            (result, _) = AddWithCarry(datasize, operand1, operand2, true);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/subs_addsub_shift.xml
        public static void Subs_Rs(bool sf, Bits shift, Bits Rm, Bits imm6, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* if shift == '11' then ReservedValue(); */
            /* if sf == '0' && imm6<5> == '1' then ReservedValue(); */

            ShiftType shift_type = DecodeShift(shift);
            int shift_amount = (int)UInt(imm6);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = ShiftReg(datasize, m, shift_type, shift_amount);
            Bits nzcv;

            operand2 = NOT(operand2);

            (result, nzcv) = AddWithCarry(datasize, operand1, operand2, true);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/udiv.xml
        public static void Udiv(bool sf, Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            BigInteger result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (IsZero(operand2))
            {
                result = (BigInteger)0m;
            }
            else
            {
                result = RoundTowardsZero(Real(Int(operand1, true)) / Real(Int(operand2, true)));
            }

            X(d, result.SubBigInteger(datasize - 1, 0));
        }
#endregion

#region "AluRx"
        // https://meriac.github.io/archex/A64_v83A_ISA/add_addsub_ext.xml
        public static void Add_Rx(bool sf, Bits Rm, Bits option, Bits imm3, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ExtendType extend_type = DecodeRegExtend(option);
            int shift = (int)UInt(imm3);

            /* if shift > 4 then ReservedValue(); */

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));
            Bits operand2 = ExtendReg(datasize, m, extend_type, shift);

            (result, _) = AddWithCarry(datasize, operand1, operand2, false);

            if (d == 31)
            {
                SP(result);
            }
            else
            {
                X(d, result);
            }
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/adds_addsub_ext.xml
        public static void Adds_Rx(bool sf, Bits Rm, Bits option, Bits imm3, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ExtendType extend_type = DecodeRegExtend(option);
            int shift = (int)UInt(imm3);

            /* if shift > 4 then ReservedValue(); */

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));
            Bits operand2 = ExtendReg(datasize, m, extend_type, shift);
            Bits nzcv;

            (result, nzcv) = AddWithCarry(datasize, operand1, operand2, false);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/sub_addsub_ext.xml
        public static void Sub_Rx(bool sf, Bits Rm, Bits option, Bits imm3, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ExtendType extend_type = DecodeRegExtend(option);
            int shift = (int)UInt(imm3);

            /* if shift > 4 then ReservedValue(); */

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));
            Bits operand2 = ExtendReg(datasize, m, extend_type, shift);

            operand2 = NOT(operand2);

            (result, _) = AddWithCarry(datasize, operand1, operand2, true);

            if (d == 31)
            {
                SP(result);
            }
            else
            {
                X(d, result);
            }
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/subs_addsub_ext.xml
        public static void Subs_Rx(bool sf, Bits Rm, Bits option, Bits imm3, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            ExtendType extend_type = DecodeRegExtend(option);
            int shift = (int)UInt(imm3);

            /* if shift > 4 then ReservedValue(); */

            /* Operation */
            Bits result;
            Bits operand1 = (n == 31 ? SP(datasize) : X(datasize, n));
            Bits operand2 = ExtendReg(datasize, m, extend_type, shift);
            Bits nzcv;

            operand2 = NOT(operand2);

            (result, nzcv) = AddWithCarry(datasize, operand1, operand2, true);

            PSTATE.NZCV(nzcv);

            X(d, result);
        }
#endregion

#region "Bfm"
        // https://meriac.github.io/archex/A64_v83A_ISA/bfm.xml
        public static void Bfm(bool sf, bool N, Bits immr, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            int R;
            Bits wmask;
            Bits tmask;

            /* if sf == '1' && N != '1' then ReservedValue(); */
            /* if sf == '0' && (N != '0' || immr<5> != '0' || imms<5> != '0') then ReservedValue(); */

            R = (int)UInt(immr);
            (wmask, tmask) = DecodeBitMasks(datasize, N, imms, immr, false);

            /* Operation */
            Bits dst = X(datasize, d);
            Bits src = X(datasize, n);

            // perform bitfield move on low bits
            Bits bot = OR(AND(dst, NOT(wmask)), AND(ROR(src, R), wmask));

            // combine extension bits and result bits
            X(d, OR(AND(dst, NOT(tmask)), AND(bot, tmask)));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/sbfm.xml
        public static void Sbfm(bool sf, bool N, Bits immr, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            int R;
            int S;
            Bits wmask;
            Bits tmask;

            /* if sf == '1' && N != '1' then ReservedValue(); */
            /* if sf == '0' && (N != '0' || immr<5> != '0' || imms<5> != '0') then ReservedValue(); */

            R = (int)UInt(immr);
            S = (int)UInt(imms);
            (wmask, tmask) = DecodeBitMasks(datasize, N, imms, immr, false);

            /* Operation */
            Bits src = X(datasize, n);

            // perform bitfield move on low bits
            Bits bot = AND(ROR(src, R), wmask);

            // determine extension bits (sign, zero or dest register)
            Bits top = Replicate(datasize, src[S]);

            // combine extension bits and result bits
            X(d, OR(AND(top, NOT(tmask)), AND(bot, tmask)));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/ubfm.xml
        public static void Ubfm(bool sf, bool N, Bits immr, Bits imms, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            int R;
            Bits wmask;
            Bits tmask;

            /* if sf == '1' && N != '1' then ReservedValue(); */
            /* if sf == '0' && (N != '0' || immr<5> != '0' || imms<5> != '0') then ReservedValue(); */

            R = (int)UInt(immr);
            (wmask, tmask) = DecodeBitMasks(datasize, N, imms, immr, false);

            /* Operation */
            Bits src = X(datasize, n);

            // perform bitfield move on low bits
            Bits bot = AND(ROR(src, R), wmask);

            // combine extension bits and result bits
            X(d, AND(bot, tmask));
        }
#endregion

#region "CcmpImm"
        // https://meriac.github.io/archex/A64_v83A_ISA/ccmn_imm.xml
        public static void Ccmn_Imm(bool sf, Bits imm5, Bits cond, Bits Rn, Bits nzcv)
        {
            /* Decode */
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits flags = nzcv;
            Bits imm = ZeroExtend(imm5, datasize);

            /* Operation */
            Bits operand1 = X(datasize, n);

            if (ConditionHolds(cond))
            {
                (_, flags) = AddWithCarry(datasize, operand1, imm, false);
            }

            PSTATE.NZCV(flags);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/ccmp_imm.xml
        public static void Ccmp_Imm(bool sf, Bits imm5, Bits cond, Bits Rn, Bits nzcv)
        {
            /* Decode */
            int n = (int)UInt(Rn);
            int datasize = (sf ? 64 : 32);

            Bits flags = nzcv;
            Bits imm = ZeroExtend(imm5, datasize);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2;

            if (ConditionHolds(cond))
            {
                operand2 = NOT(imm);
                (_, flags) = AddWithCarry(datasize, operand1, operand2, true);
            }

            PSTATE.NZCV(flags);
        }
#endregion

#region "CcmpReg"
        // https://meriac.github.io/archex/A64_v83A_ISA/ccmn_reg.xml
        public static void Ccmn_Reg(bool sf, Bits Rm, Bits cond, Bits Rn, Bits nzcv)
        {
            /* Decode */
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            Bits flags = nzcv;

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (ConditionHolds(cond))
            {
                (_, flags) = AddWithCarry(datasize, operand1, operand2, false);
            }

            PSTATE.NZCV(flags);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/ccmp_reg.xml
        public static void Ccmp_Reg(bool sf, Bits Rm, Bits cond, Bits Rn, Bits nzcv)
        {
            /* Decode */
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            Bits flags = nzcv;

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (ConditionHolds(cond))
            {
                operand2 = NOT(operand2);
                (_, flags) = AddWithCarry(datasize, operand1, operand2, true);
            }

            PSTATE.NZCV(flags);
        }
#endregion

#region "Csel"
        // https://meriac.github.io/archex/A64_v83A_ISA/csel.xml
        public static void Csel(bool sf, Bits Rm, Bits cond, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (ConditionHolds(cond))
            {
                result = operand1;
            }
            else
            {
                result = operand2;
            }

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/csinc.xml
        public static void Csinc(bool sf, Bits Rm, Bits cond, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (ConditionHolds(cond))
            {
                result = operand1;
            }
            else
            {
                result = operand2 + 1;
            }

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/csinv.xml
        public static void Csinv(bool sf, Bits Rm, Bits cond, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (ConditionHolds(cond))
            {
                result = operand1;
            }
            else
            {
                result = NOT(operand2);
            }

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/csneg.xml
        public static void Csneg(bool sf, Bits Rm, Bits cond, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits result;
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);

            if (ConditionHolds(cond))
            {
                result = operand1;
            }
            else
            {
                result = NOT(operand2);
                result = result + 1;
            }

            X(d, result);
        }
#endregion

#region "Mov"
        // https://meriac.github.io/archex/A64_v83A_ISA/movk.xml
        public static void Movk(bool sf, Bits hw, Bits imm16, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && hw<1> == '1' then UnallocatedEncoding(); */

            int pos = (int)UInt(Bits.Concat(hw, "0000"));

            /* Operation */
            Bits result = X(datasize, d);

            result[pos + 15, pos] = imm16;

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/movn.xml
        public static void Movn(bool sf, Bits hw, Bits imm16, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && hw<1> == '1' then UnallocatedEncoding(); */

            int pos = (int)UInt(Bits.Concat(hw, "0000"));

            /* Operation */
            Bits result = Zeros(datasize);

            result[pos + 15, pos] = imm16;
            result = NOT(result);

            X(d, result);
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/movz.xml
        public static void Movz(bool sf, Bits hw, Bits imm16, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int datasize = (sf ? 64 : 32);

            /* if sf == '0' && hw<1> == '1' then UnallocatedEncoding(); */

            int pos = (int)UInt(Bits.Concat(hw, "0000"));

            /* Operation */
            Bits result = Zeros(datasize);

            result[pos + 15, pos] = imm16;

            X(d, result);
        }
#endregion

#region "Mul"
        // https://meriac.github.io/archex/A64_v83A_ISA/madd.xml
        public static void Madd(bool sf, Bits Rm, Bits Ra, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int a = (int)UInt(Ra);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);
            Bits operand3 = X(datasize, a);

            BigInteger result = UInt(operand3) + (UInt(operand1) * UInt(operand2));

            X(d, result.SubBigInteger(datasize - 1, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/msub.xml
        public static void Msub(bool sf, Bits Rm, Bits Ra, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int a = (int)UInt(Ra);
            int datasize = (sf ? 64 : 32);

            /* Operation */
            Bits operand1 = X(datasize, n);
            Bits operand2 = X(datasize, m);
            Bits operand3 = X(datasize, a);

            BigInteger result = UInt(operand3) - (UInt(operand1) * UInt(operand2));

            X(d, result.SubBigInteger(datasize - 1, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/smaddl.xml
        public static void Smaddl(Bits Rm, Bits Ra, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int a = (int)UInt(Ra);

            /* Operation */
            Bits operand1 = X(32, n);
            Bits operand2 = X(32, m);
            Bits operand3 = X(64, a);

            BigInteger result = Int(operand3, false) + (Int(operand1, false) * Int(operand2, false));

            X(d, result.SubBigInteger(63, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/umaddl.xml
        public static void Umaddl(Bits Rm, Bits Ra, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int a = (int)UInt(Ra);

            /* Operation */
            Bits operand1 = X(32, n);
            Bits operand2 = X(32, m);
            Bits operand3 = X(64, a);

            BigInteger result = Int(operand3, true) + (Int(operand1, true) * Int(operand2, true));

            X(d, result.SubBigInteger(63, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/smsubl.xml
        public static void Smsubl(Bits Rm, Bits Ra, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int a = (int)UInt(Ra);

            /* Operation */
            Bits operand1 = X(32, n);
            Bits operand2 = X(32, m);
            Bits operand3 = X(64, a);

            BigInteger result = Int(operand3, false) - (Int(operand1, false) * Int(operand2, false));

            X(d, result.SubBigInteger(63, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/umsubl.xml
        public static void Umsubl(Bits Rm, Bits Ra, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);
            int a = (int)UInt(Ra);

            /* Operation */
            Bits operand1 = X(32, n);
            Bits operand2 = X(32, m);
            Bits operand3 = X(64, a);

            BigInteger result = Int(operand3, true) - (Int(operand1, true) * Int(operand2, true));

            X(d, result.SubBigInteger(63, 0));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/smulh.xml
        public static void Smulh(Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);

            /* Operation */
            Bits operand1 = X(64, n);
            Bits operand2 = X(64, m);

            BigInteger result = Int(operand1, false) * Int(operand2, false);

            X(d, result.SubBigInteger(127, 64));
        }

        // https://meriac.github.io/archex/A64_v83A_ISA/umulh.xml
        public static void Umulh(Bits Rm, Bits Rn, Bits Rd)
        {
            /* Decode */
            int d = (int)UInt(Rd);
            int n = (int)UInt(Rn);
            int m = (int)UInt(Rm);

            /* Operation */
            Bits operand1 = X(64, n);
            Bits operand2 = X(64, m);

            BigInteger result = Int(operand1, true) * Int(operand2, true);

            X(d, result.SubBigInteger(127, 64));
        }
#endregion
    }

    /*
    internal static class Advanced
    {
    }
    */
}
