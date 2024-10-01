using System.Numerics;

namespace Ryujinx.Cpu.LightningJit.CodeGen.Arm64
{
    readonly struct RegisterSaveRestore
    {
        private const int FpRegister = 29;
        private const int LrRegister = 30;

        public const int Encodable9BitsOffsetLimit = 0x100;

        private readonly uint _gprMask;
        private readonly uint _fpSimdMask;
        private readonly OperandType _fpSimdType;
        private readonly int _reservedStackSize;
        private readonly bool _hasCall;

        public RegisterSaveRestore(
            uint gprMask,
            uint fpSimdMask = 0,
            OperandType fpSimdType = OperandType.FP64,
            bool hasCall = false,
            int reservedStackSize = 0)
        {
            _gprMask = gprMask;
            _fpSimdMask = fpSimdMask;
            _fpSimdType = fpSimdType;
            _reservedStackSize = reservedStackSize;
            _hasCall = hasCall;
        }

        public int GetReservedStackOffset()
        {
            int gprCalleeSavedRegsCount = BitOperations.PopCount(_gprMask);
            int fpSimdCalleeSavedRegsCount = BitOperations.PopCount(_fpSimdMask);

            return (_hasCall ? 16 : 0) + Align16(gprCalleeSavedRegsCount * 8 + fpSimdCalleeSavedRegsCount * _fpSimdType.GetSizeInBytes());
        }

        public void WritePrologue(ref Assembler asm)
        {
            uint gprMask = _gprMask;
            uint fpSimdMask = _fpSimdMask;

            int gprCalleeSavedRegsCount = BitOperations.PopCount(gprMask);
            int fpSimdCalleeSavedRegsCount = BitOperations.PopCount(fpSimdMask);

            int reservedStackSize = Align16(_reservedStackSize);
            int calleeSaveRegionSize = Align16(gprCalleeSavedRegsCount * 8 + fpSimdCalleeSavedRegsCount * _fpSimdType.GetSizeInBytes()) + reservedStackSize;
            int offset = 0;

            WritePrologueCalleeSavesPreIndexed(ref asm, ref gprMask, ref offset, calleeSaveRegionSize, OperandType.I64);

            if (_fpSimdType == OperandType.V128 && (gprCalleeSavedRegsCount & 1) != 0)
            {
                offset += 8;
            }

            WritePrologueCalleeSavesPreIndexed(ref asm, ref fpSimdMask, ref offset, calleeSaveRegionSize, _fpSimdType);

            if (_hasCall)
            {
                Operand rsp = Register(Assembler.SpRegister);

                if (offset != 0 || calleeSaveRegionSize + 16 < Encodable9BitsOffsetLimit)
                {
                    asm.StpRiPre(Register(FpRegister), Register(LrRegister), rsp, offset == 0 ? -(calleeSaveRegionSize + 16) : -16);
                }
                else
                {
                    asm.Sub(rsp, rsp, new Operand(OperandKind.Constant, OperandType.I64, (ulong)calleeSaveRegionSize));
                    asm.StpRiPre(Register(FpRegister), Register(LrRegister), rsp, -16);
                }

                asm.MovSp(Register(FpRegister), rsp);
            }
        }

        private static void WritePrologueCalleeSavesPreIndexed(
            ref Assembler asm,
            ref uint mask,
            ref int offset,
            int calleeSaveRegionSize,
            OperandType type)
        {
            if ((BitOperations.PopCount(mask) & 1) != 0)
            {
                int reg = BitOperations.TrailingZeroCount(mask);

                mask &= ~(1u << reg);

                if (offset != 0)
                {
                    asm.StrRiUn(Register(reg, type), Register(Assembler.SpRegister), offset);
                }
                else if (calleeSaveRegionSize < Encodable9BitsOffsetLimit)
                {
                    asm.StrRiPre(Register(reg, type), Register(Assembler.SpRegister), -calleeSaveRegionSize);
                }
                else
                {
                    asm.Sub(Register(Assembler.SpRegister), Register(Assembler.SpRegister), new Operand(OperandType.I64, (ulong)calleeSaveRegionSize));
                    asm.StrRiUn(Register(reg, type), Register(Assembler.SpRegister), 0);
                }

                offset += type.GetSizeInBytes();
            }

            while (mask != 0)
            {
                int reg = BitOperations.TrailingZeroCount(mask);

                mask &= ~(1u << reg);

                int reg2 = BitOperations.TrailingZeroCount(mask);

                mask &= ~(1u << reg2);

                if (offset != 0)
                {
                    asm.StpRiUn(Register(reg, type), Register(reg2, type), Register(Assembler.SpRegister), offset);
                }
                else if (calleeSaveRegionSize < Encodable9BitsOffsetLimit)
                {
                    asm.StpRiPre(Register(reg, type), Register(reg2, type), Register(Assembler.SpRegister), -calleeSaveRegionSize);
                }
                else
                {
                    asm.Sub(Register(Assembler.SpRegister), Register(Assembler.SpRegister), new Operand(OperandType.I64, (ulong)calleeSaveRegionSize));
                    asm.StpRiUn(Register(reg, type), Register(reg2, type), Register(Assembler.SpRegister), 0);
                }

                offset += type.GetSizeInBytes() * 2;
            }
        }

        public void WriteEpilogue(ref Assembler asm)
        {
            uint gprMask = _gprMask;
            uint fpSimdMask = _fpSimdMask;

            int gprCalleeSavedRegsCount = BitOperations.PopCount(gprMask);
            int fpSimdCalleeSavedRegsCount = BitOperations.PopCount(fpSimdMask);

            bool misalignedVector = _fpSimdType == OperandType.V128 && (gprCalleeSavedRegsCount & 1) != 0;

            int offset = gprCalleeSavedRegsCount * 8 + fpSimdCalleeSavedRegsCount * _fpSimdType.GetSizeInBytes();

            if (misalignedVector)
            {
                offset += 8;
            }

            int calleeSaveRegionSize = Align16(offset) + Align16(_reservedStackSize);

            if (_hasCall)
            {
                Operand rsp = Register(Assembler.SpRegister);

                if (offset != 0 || calleeSaveRegionSize + 16 < Encodable9BitsOffsetLimit)
                {
                    asm.LdpRiPost(Register(FpRegister), Register(LrRegister), rsp, offset == 0 ? calleeSaveRegionSize + 16 : 16);
                }
                else
                {
                    asm.LdpRiPost(Register(FpRegister), Register(LrRegister), rsp, 16);
                    asm.Add(rsp, rsp, new Operand(OperandKind.Constant, OperandType.I64, (ulong)calleeSaveRegionSize));
                }
            }

            WriteEpilogueCalleeSavesPostIndexed(ref asm, ref fpSimdMask, ref offset, calleeSaveRegionSize, _fpSimdType);

            if (misalignedVector)
            {
                offset -= 8;
            }

            WriteEpilogueCalleeSavesPostIndexed(ref asm, ref gprMask, ref offset, calleeSaveRegionSize, OperandType.I64);
        }

        private static void WriteEpilogueCalleeSavesPostIndexed(
            ref Assembler asm,
            ref uint mask,
            ref int offset,
            int calleeSaveRegionSize,
            OperandType type)
        {
            while (mask != 0)
            {
                int reg = HighestBitSet(mask);

                mask &= ~(1u << reg);

                if (mask != 0)
                {
                    int reg2 = HighestBitSet(mask);

                    mask &= ~(1u << reg2);

                    offset -= type.GetSizeInBytes() * 2;

                    if (offset != 0)
                    {
                        asm.LdpRiUn(Register(reg2, type), Register(reg, type), Register(Assembler.SpRegister), offset);
                    }
                    else if (calleeSaveRegionSize < Encodable9BitsOffsetLimit)
                    {
                        asm.LdpRiPost(Register(reg2, type), Register(reg, type), Register(Assembler.SpRegister), calleeSaveRegionSize);
                    }
                    else
                    {
                        asm.LdpRiUn(Register(reg2, type), Register(reg, type), Register(Assembler.SpRegister), 0);
                        asm.Add(Register(Assembler.SpRegister), Register(Assembler.SpRegister), new Operand(OperandType.I64, (ulong)calleeSaveRegionSize));
                    }
                }
                else
                {
                    offset -= type.GetSizeInBytes();

                    if (offset != 0)
                    {
                        asm.LdrRiUn(Register(reg, type), Register(Assembler.SpRegister), offset);
                    }
                    else if (calleeSaveRegionSize < Encodable9BitsOffsetLimit)
                    {
                        asm.LdrRiPost(Register(reg, type), Register(Assembler.SpRegister), calleeSaveRegionSize);
                    }
                    else
                    {
                        asm.LdrRiUn(Register(reg, type), Register(Assembler.SpRegister), 0);
                        asm.Add(Register(Assembler.SpRegister), Register(Assembler.SpRegister), new Operand(OperandType.I64, (ulong)calleeSaveRegionSize));
                    }
                }
            }
        }

        private static int HighestBitSet(uint value)
        {
            return 31 - BitOperations.LeadingZeroCount(value);
        }

        private static Operand Register(int register, OperandType type = OperandType.I64)
        {
            return new Operand(register, RegisterType.Integer, type);
        }

        private static int Align16(int value)
        {
            return (value + 0xf) & ~0xf;
        }
    }
}
