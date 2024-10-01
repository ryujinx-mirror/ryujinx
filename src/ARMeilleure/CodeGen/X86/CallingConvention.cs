using System;

namespace ARMeilleure.CodeGen.X86
{
    static class CallingConvention
    {
        private const int RegistersMask = 0xffff;

        public static int GetIntAvailableRegisters()
        {
            return RegistersMask & ~(1 << (int)X86Register.Rsp);
        }

        public static int GetVecAvailableRegisters()
        {
            return RegistersMask;
        }

        public static int GetIntCallerSavedRegisters()
        {
            if (GetCurrentCallConv() == CallConvName.Windows)
            {
#pragma warning disable IDE0055 // Disable formatting
                return (1 << (int)X86Register.Rax) |
                       (1 << (int)X86Register.Rcx) |
                       (1 << (int)X86Register.Rdx) |
                       (1 << (int)X86Register.R8)  |
                       (1 << (int)X86Register.R9)  |
                       (1 << (int)X86Register.R10) |
                       (1 << (int)X86Register.R11);
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                return (1 << (int)X86Register.Rax) |
                       (1 << (int)X86Register.Rcx) |
                       (1 << (int)X86Register.Rdx) |
                       (1 << (int)X86Register.Rsi) |
                       (1 << (int)X86Register.Rdi) |
                       (1 << (int)X86Register.R8)  |
                       (1 << (int)X86Register.R9)  |
                       (1 << (int)X86Register.R10) |
                       (1 << (int)X86Register.R11);
#pragma warning restore IDE0055
            }
        }

        public static int GetVecCallerSavedRegisters()
        {
            if (GetCurrentCallConv() == CallConvName.Windows)
            {
                return (1 << (int)X86Register.Xmm0) |
                       (1 << (int)X86Register.Xmm1) |
                       (1 << (int)X86Register.Xmm2) |
                       (1 << (int)X86Register.Xmm3) |
                       (1 << (int)X86Register.Xmm4) |
                       (1 << (int)X86Register.Xmm5);
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                return RegistersMask;
            }
        }

        public static int GetIntCalleeSavedRegisters()
        {
            return GetIntCallerSavedRegisters() ^ RegistersMask;
        }

        public static int GetVecCalleeSavedRegisters()
        {
            return GetVecCallerSavedRegisters() ^ RegistersMask;
        }

        public static int GetArgumentsOnRegsCount()
        {
            return 4;
        }

        public static int GetIntArgumentsOnRegsCount()
        {
            return 6;
        }

        public static int GetVecArgumentsOnRegsCount()
        {
            return 8;
        }

        public static X86Register GetIntArgumentRegister(int index)
        {
            if (GetCurrentCallConv() == CallConvName.Windows)
            {
                switch (index)
                {
                    case 0:
                        return X86Register.Rcx;
                    case 1:
                        return X86Register.Rdx;
                    case 2:
                        return X86Register.R8;
                    case 3:
                        return X86Register.R9;
                }
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                switch (index)
                {
                    case 0:
                        return X86Register.Rdi;
                    case 1:
                        return X86Register.Rsi;
                    case 2:
                        return X86Register.Rdx;
                    case 3:
                        return X86Register.Rcx;
                    case 4:
                        return X86Register.R8;
                    case 5:
                        return X86Register.R9;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static X86Register GetVecArgumentRegister(int index)
        {
            int count;

            if (GetCurrentCallConv() == CallConvName.Windows)
            {
                count = 4;
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                count = 8;
            }

            if ((uint)index < count)
            {
                return X86Register.Xmm0 + index;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static X86Register GetIntReturnRegister()
        {
            return X86Register.Rax;
        }

        public static X86Register GetIntReturnRegisterHigh()
        {
            return X86Register.Rdx;
        }

        public static X86Register GetVecReturnRegister()
        {
            return X86Register.Xmm0;
        }

        public static CallConvName GetCurrentCallConv()
        {
            return OperatingSystem.IsWindows()
                ? CallConvName.Windows
                : CallConvName.SystemV;
        }
    }
}
