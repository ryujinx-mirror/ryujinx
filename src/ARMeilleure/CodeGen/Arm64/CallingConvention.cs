using System;

namespace ARMeilleure.CodeGen.Arm64
{
    static class CallingConvention
    {
        private const int RegistersMask = unchecked((int)0xffffffff);

        // Some of those register have specific roles and can't be used as general purpose registers.
        // X18 - Reserved for platform specific usage.
        // X29 - Frame pointer.
        // X30 - Return address.
        // X31 - Not an actual register, in some cases maps to SP, and in others to ZR.
        private const int ReservedRegsMask = (1 << CodeGenCommon.ReservedRegister) | (1 << 18) | (1 << 29) | (1 << 30) | (1 << 31);

        public static int GetIntAvailableRegisters()
        {
            return RegistersMask & ~ReservedRegsMask;
        }

        public static int GetVecAvailableRegisters()
        {
            return RegistersMask;
        }

        public static int GetIntCallerSavedRegisters()
        {
            return (GetIntCalleeSavedRegisters() ^ RegistersMask) & ~ReservedRegsMask;
        }

        public static int GetFpCallerSavedRegisters()
        {
            return GetFpCalleeSavedRegisters() ^ RegistersMask;
        }

        public static int GetVecCallerSavedRegisters()
        {
            return GetVecCalleeSavedRegisters() ^ RegistersMask;
        }

        public static int GetIntCalleeSavedRegisters()
        {
            return 0x1ff80000; // X19 to X28
        }

        public static int GetFpCalleeSavedRegisters()
        {
            return 0xff00; // D8 to D15
        }

        public static int GetVecCalleeSavedRegisters()
        {
            return 0;
        }

        public static int GetArgumentsOnRegsCount()
        {
            return 8;
        }

        public static int GetIntArgumentRegister(int index)
        {
            if ((uint)index < (uint)GetArgumentsOnRegsCount())
            {
                return index;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static int GetVecArgumentRegister(int index)
        {
            if ((uint)index < (uint)GetArgumentsOnRegsCount())
            {
                return index;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static int GetIntReturnRegister()
        {
            return 0;
        }

        public static int GetIntReturnRegisterHigh()
        {
            return 1;
        }

        public static int GetVecReturnRegister()
        {
            return 0;
        }
    }
}
