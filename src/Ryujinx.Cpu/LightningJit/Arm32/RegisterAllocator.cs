using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;
using System.Numerics;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    class RegisterAllocator
    {
        public const int MaxTemps = 1;

        private uint _gprMask;
        private uint _fpSimdMask;

        public int FixedContextRegister { get; }
        public int FixedPageTableRegister { get; }

        public uint UsedGprsMask { get; private set; }
        public uint UsedFpSimdMask { get; private set; }

        public RegisterAllocator()
        {
            _gprMask = ushort.MaxValue;
            _fpSimdMask = ushort.MaxValue;

            FixedContextRegister = AllocateTempRegisterWithPreferencing();
            FixedPageTableRegister = AllocateTempRegisterWithPreferencing();
        }

        public void MarkGprAsUsed(int index)
        {
            UsedGprsMask |= 1u << index;
        }

        public void MarkFpSimdAsUsed(int index)
        {
            UsedFpSimdMask |= 1u << index;
        }

        public void MarkFpSimdRangeAsUsed(int index, int count)
        {
            UsedFpSimdMask |= (uint.MaxValue >> (32 - count)) << index;
        }

        public Operand RemapGprRegister(int index)
        {
            MarkGprAsUsed(index);

            return new Operand(OperandKind.Register, OperandType.I32, (ulong)index);
        }

        public Operand RemapFpRegister(int index, bool isFP32)
        {
            MarkFpSimdAsUsed(index);

            return new Operand(OperandKind.Register, isFP32 ? OperandType.FP32 : OperandType.FP64, (ulong)index);
        }

        public Operand RemapSimdRegister(int index)
        {
            MarkFpSimdAsUsed(index);

            return new Operand(OperandKind.Register, OperandType.V128, (ulong)index);
        }

        public Operand RemapSimdRegister(int index, int count)
        {
            MarkFpSimdRangeAsUsed(index, count);

            return new Operand(OperandKind.Register, OperandType.V128, (ulong)index);
        }

        public void EnsureTempGprRegisters(int count)
        {
            if (count != 0)
            {
                Span<int> registers = stackalloc int[count];

                for (int index = 0; index < count; index++)
                {
                    registers[index] = AllocateTempGprRegister();
                }

                for (int index = 0; index < count; index++)
                {
                    FreeTempGprRegister(registers[index]);
                }
            }
        }

        public int AllocateTempGprRegister()
        {
            int index = AllocateTempRegister(ref _gprMask, AbiConstants.ReservedRegsMask);

            MarkGprAsUsed(index);

            return index;
        }

        private int AllocateTempRegisterWithPreferencing()
        {
            int firstCalleeSaved = BitOperations.TrailingZeroCount(~_gprMask & AbiConstants.GprCalleeSavedRegsMask);
            if (firstCalleeSaved < 32)
            {
                uint regMask = 1u << firstCalleeSaved;
                if ((regMask & AbiConstants.ReservedRegsMask) == 0)
                {
                    _gprMask |= regMask;
                    UsedGprsMask |= regMask;

                    return firstCalleeSaved;
                }
            }

            return AllocateTempRegister(ref _gprMask, AbiConstants.ReservedRegsMask);
        }

        public int AllocateTempFpSimdRegister()
        {
            int index = AllocateTempRegister(ref _fpSimdMask, 0);

            MarkFpSimdAsUsed(index);

            return index;
        }

        public ScopedRegister AllocateTempGprRegisterScoped()
        {
            return new(this, new(OperandKind.Register, OperandType.I32, (ulong)AllocateTempGprRegister()));
        }

        public ScopedRegister AllocateTempFpRegisterScoped(bool isFP32)
        {
            return new(this, new(OperandKind.Register, isFP32 ? OperandType.FP32 : OperandType.FP64, (ulong)AllocateTempFpSimdRegister()));
        }

        public ScopedRegister AllocateTempSimdRegisterScoped()
        {
            return new(this, new(OperandKind.Register, OperandType.V128, (ulong)AllocateTempFpSimdRegister()));
        }

        public void FreeTempGprRegister(int index)
        {
            FreeTempRegister(ref _gprMask, index);
        }

        public void FreeTempFpSimdRegister(int index)
        {
            FreeTempRegister(ref _fpSimdMask, index);
        }

        private static int AllocateTempRegister(ref uint mask, uint reservedMask)
        {
            int index = BitOperations.TrailingZeroCount(~(mask | reservedMask));
            if (index == sizeof(uint) * 8)
            {
                throw new InvalidOperationException("No free registers.");
            }

            mask |= 1u << index;

            return index;
        }

        private static void FreeTempRegister(ref uint mask, int index)
        {
            mask &= ~(1u << index);
        }
    }
}
