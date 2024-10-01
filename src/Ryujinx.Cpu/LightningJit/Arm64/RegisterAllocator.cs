using ARMeilleure.Memory;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;
using System.Numerics;

namespace Ryujinx.Cpu.LightningJit.Arm64
{
    class RegisterAllocator
    {
        private uint _gprMask;
        private readonly uint _fpSimdMask;
        private readonly uint _pStateMask;

        private uint _tempGprsMask;

        private readonly int[] _registerMap;

        public int FixedContextRegister { get; }
        public int FixedPageTableRegister { get; }

        public uint AllGprMask => (_gprMask & ~RegisterUtils.ReservedRegsMask) | _tempGprsMask;
        public uint AllFpSimdMask => _fpSimdMask;
        public uint AllPStateMask => _pStateMask;

        public RegisterAllocator(MemoryManagerType mmType, uint gprMask, uint fpSimdMask, uint pStateMask, bool hasHostCall)
        {
            _gprMask = gprMask;
            _fpSimdMask = fpSimdMask;
            _pStateMask = pStateMask;

            if (hasHostCall)
            {
                // If the function has calls, we can avoid the need to spill those registers across
                // calls by puting them on callee saved registers.

                FixedContextRegister = AllocateAndMarkTempGprRegisterWithPreferencing();
                FixedPageTableRegister = AllocateAndMarkTempGprRegisterWithPreferencing();
            }
            else
            {
                FixedContextRegister = AllocateAndMarkTempGprRegister();
                FixedPageTableRegister = AllocateAndMarkTempGprRegister();
            }

            _tempGprsMask = (1u << FixedContextRegister) | (1u << FixedPageTableRegister);

            _registerMap = new int[32];

            for (int index = 0; index < _registerMap.Length; index++)
            {
                _registerMap[index] = index;
            }

            BuildRegisterMap(_registerMap);

            Span<int> tempRegisters = stackalloc int[CalculateMaxTemps(mmType)];

            for (int index = 0; index < tempRegisters.Length; index++)
            {
                tempRegisters[index] = AllocateAndMarkTempGprRegister();
            }

            for (int index = 0; index < tempRegisters.Length; index++)
            {
                FreeTempGprRegister(tempRegisters[index]);
            }
        }

        private void BuildRegisterMap(Span<int> map)
        {
            uint mask = _gprMask & RegisterUtils.ReservedRegsMask;

            while (mask != 0)
            {
                int index = BitOperations.TrailingZeroCount(mask);
                int remapIndex = AllocateAndMarkTempGprRegister();

                map[index] = remapIndex;
                _tempGprsMask |= 1u << remapIndex;

                mask &= ~(1u << index);
            }
        }

        public int RemapReservedGprRegister(int index)
        {
            return _registerMap[index];
        }

        private int AllocateAndMarkTempGprRegister()
        {
            int index = AllocateTempGprRegister();
            _tempGprsMask |= 1u << index;

            return index;
        }

        private int AllocateAndMarkTempGprRegisterWithPreferencing()
        {
            int index = AllocateTempRegisterWithPreferencing();
            _tempGprsMask |= 1u << index;

            return index;
        }

        public int AllocateTempGprRegister()
        {
            return AllocateTempRegister(ref _gprMask);
        }

        public void FreeTempGprRegister(int index)
        {
            FreeTempRegister(ref _gprMask, index);
        }

        private int AllocateTempRegisterWithPreferencing()
        {
            int firstCalleeSaved = BitOperations.TrailingZeroCount(~_gprMask & AbiConstants.GprCalleeSavedRegsMask);
            if (firstCalleeSaved < 32)
            {
                uint regMask = 1u << firstCalleeSaved;
                if ((regMask & RegisterUtils.ReservedRegsMask) == 0)
                {
                    _gprMask |= regMask;

                    return firstCalleeSaved;
                }
            }

            return AllocateTempRegister(ref _gprMask);
        }

        private static int AllocateTempRegister(ref uint mask)
        {
            int index = BitOperations.TrailingZeroCount(~(mask | RegisterUtils.ReservedRegsMask));
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

        public static int CalculateMaxTemps(MemoryManagerType mmType)
        {
            return mmType.IsHostMapped() ? 1 : 2;
        }

        public static int CalculateMaxTempsInclFixed(MemoryManagerType mmType)
        {
            return CalculateMaxTemps(mmType) + 2;
        }
    }
}
