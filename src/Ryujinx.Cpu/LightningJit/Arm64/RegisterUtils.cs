using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm64
{
    static class RegisterUtils
    {
        private const int RdRtBit = 0;
        private const int RnBit = 5;
        private const int RmRsBit = 16;
        private const int RaRt2Bit = 10;

        // Some of those register have specific roles and can't be used as general purpose registers.
        // X18 - Reserved for platform specific usage.
        // X29 - Frame pointer.
        // X30 - Return address.
        // X31 - Not an actual register, in some cases maps to SP, and in others to ZR.
        public const uint ReservedRegsMask = (1u << 18) | (1u << 29) | (1u << 30) | (1u << 31);

        public const int LrIndex = 30;
        public const int SpIndex = 31;
        public const int ZrIndex = 31;
        public const int SpecialZrIndex = 32;

        public static uint RemapRegisters(RegisterAllocator regAlloc, InstFlags flags, uint encoding)
        {
            if (flags.HasFlag(InstFlags.Rd) && (!flags.HasFlag(InstFlags.FpSimd) || IsFpToGpr(flags, encoding)))
            {
                encoding = ReplaceGprRegister(regAlloc, encoding, RdRtBit, flags.HasFlag(InstFlags.RdSP));
            }

            if (flags.HasFlag(InstFlags.Rn) && (!flags.HasFlag(InstFlags.FpSimd) || IsFpFromGpr(flags, encoding) || flags.HasFlag(InstFlags.Memory)))
            {
                encoding = ReplaceGprRegister(regAlloc, encoding, RnBit, flags.HasFlag(InstFlags.RnSP));
            }

            if (!flags.HasFlag(InstFlags.FpSimd))
            {
                if (flags.HasFlag(InstFlags.Rm) || flags.HasFlag(InstFlags.Rs))
                {
                    encoding = ReplaceGprRegister(regAlloc, encoding, RmRsBit);
                }

                if (flags.HasFlag(InstFlags.Ra) || flags.HasFlag(InstFlags.Rt2))
                {
                    encoding = ReplaceGprRegister(regAlloc, encoding, RaRt2Bit);
                }

                if (flags.HasFlag(InstFlags.Rt))
                {
                    encoding = ReplaceGprRegister(regAlloc, encoding, RdRtBit);
                }
            }
            else if (flags.HasFlag(InstFlags.Rm) && flags.HasFlag(InstFlags.Memory))
            {
                encoding = ReplaceGprRegister(regAlloc, encoding, RmRsBit);
            }

            return encoding;
        }

        public static uint ReplaceRt(uint encoding, int newIndex)
        {
            return ReplaceRegister(encoding, newIndex, RdRtBit);
        }

        public static uint ReplaceRn(uint encoding, int newIndex)
        {
            return ReplaceRegister(encoding, newIndex, RnBit);
        }

        private static uint ReplaceRegister(uint encoding, int newIndex, int bit)
        {
            encoding &= ~(0x1fu << bit);
            encoding |= (uint)newIndex << bit;

            return encoding;
        }

        private static uint ReplaceGprRegister(RegisterAllocator regAlloc, uint encoding, int bit, bool hasSP = false)
        {
            int oldIndex = (int)(encoding >> bit) & 0x1f;
            if (oldIndex == ZrIndex && !hasSP)
            {
                return encoding;
            }

            int newIndex = regAlloc.RemapReservedGprRegister(oldIndex);

            encoding &= ~(0x1fu << bit);
            encoding |= (uint)newIndex << bit;

            return encoding;
        }

        public static (uint, uint) PopulateReadMasks(InstName name, InstFlags flags, uint encoding)
        {
            uint gprMask = 0;
            uint fpSimdMask = 0;

            if (flags.HasFlag(InstFlags.FpSimd))
            {
                if (flags.HasFlag(InstFlags.Rd) && flags.HasFlag(InstFlags.ReadRd))
                {
                    uint mask = MaskFromIndex(ExtractRd(flags, encoding));

                    if (IsFpToGpr(flags, encoding))
                    {
                        gprMask |= mask;
                    }
                    else
                    {
                        fpSimdMask |= mask;
                    }
                }

                if (flags.HasFlag(InstFlags.Rn))
                {
                    uint mask = MaskFromIndex(ExtractRn(flags, encoding));

                    if (flags.HasFlag(InstFlags.RnSeq))
                    {
                        int count = GetRnSequenceCount(encoding);

                        for (int index = 0; index < count; index++, mask <<= 1)
                        {
                            fpSimdMask |= mask;
                        }
                    }
                    else if (IsFpFromGpr(flags, encoding) || flags.HasFlag(InstFlags.Memory))
                    {
                        gprMask |= mask;
                    }
                    else
                    {
                        fpSimdMask |= mask;
                    }
                }

                if (flags.HasFlag(InstFlags.Rm))
                {
                    uint mask = MaskFromIndex(ExtractRm(flags, encoding));

                    if (flags.HasFlag(InstFlags.Memory))
                    {
                        gprMask |= mask;
                    }
                    else
                    {
                        fpSimdMask |= mask;
                    }
                }

                if (flags.HasFlag(InstFlags.Ra))
                {
                    fpSimdMask |= MaskFromIndex(ExtractRa(flags, encoding));
                }

                if (flags.HasFlag(InstFlags.ReadRt))
                {
                    if (flags.HasFlag(InstFlags.Rt))
                    {
                        uint mask = MaskFromIndex(ExtractRt(flags, encoding));

                        if (flags.HasFlag(InstFlags.RtSeq))
                        {
                            int count = GetRtSequenceCount(name, encoding);

                            for (int index = 0; index < count; index++, mask <<= 1)
                            {
                                fpSimdMask |= mask;
                            }
                        }
                        else
                        {
                            fpSimdMask |= mask;
                        }
                    }

                    if (flags.HasFlag(InstFlags.Rt2))
                    {
                        fpSimdMask |= MaskFromIndex(ExtractRt2(flags, encoding));
                    }
                }
            }
            else
            {
                if (flags.HasFlag(InstFlags.Rd) && flags.HasFlag(InstFlags.ReadRd))
                {
                    gprMask |= MaskFromIndex(ExtractRd(flags, encoding));
                }

                if (flags.HasFlag(InstFlags.Rn))
                {
                    gprMask |= MaskFromIndex(ExtractRn(flags, encoding));
                }

                if (flags.HasFlag(InstFlags.Rm))
                {
                    gprMask |= MaskFromIndex(ExtractRm(flags, encoding));
                }

                if (flags.HasFlag(InstFlags.Ra))
                {
                    gprMask |= MaskFromIndex(ExtractRa(flags, encoding));
                }

                if (flags.HasFlag(InstFlags.ReadRt))
                {
                    if (flags.HasFlag(InstFlags.Rt))
                    {
                        gprMask |= MaskFromIndex(ExtractRt(flags, encoding));
                    }

                    if (flags.HasFlag(InstFlags.Rt2))
                    {
                        gprMask |= MaskFromIndex(ExtractRt2(flags, encoding));
                    }
                }
            }

            return (gprMask, fpSimdMask);
        }

        public static (uint, uint) PopulateWriteMasks(InstName name, InstFlags flags, uint encoding)
        {
            uint gprMask = 0;
            uint fpSimdMask = 0;

            if (flags.HasFlag(InstFlags.MemWBack))
            {
                gprMask |= MaskFromIndex(ExtractRn(flags, encoding));
            }

            if (flags.HasFlag(InstFlags.FpSimd))
            {
                if (flags.HasFlag(InstFlags.Rd))
                {
                    uint mask = MaskFromIndex(ExtractRd(flags, encoding));

                    if (IsFpToGpr(flags, encoding))
                    {
                        gprMask |= mask;
                    }
                    else
                    {
                        fpSimdMask |= mask;
                    }
                }

                if (!flags.HasFlag(InstFlags.ReadRt) || name.IsPartialRegisterUpdateMemory())
                {
                    if (flags.HasFlag(InstFlags.Rt))
                    {
                        uint mask = MaskFromIndex(ExtractRt(flags, encoding));

                        if (flags.HasFlag(InstFlags.RtSeq))
                        {
                            int count = GetRtSequenceCount(name, encoding);

                            for (int index = 0; index < count; index++, mask <<= 1)
                            {
                                fpSimdMask |= mask;
                            }
                        }
                        else
                        {
                            fpSimdMask |= mask;
                        }
                    }

                    if (flags.HasFlag(InstFlags.Rt2))
                    {
                        fpSimdMask |= MaskFromIndex(ExtractRt2(flags, encoding));
                    }
                }
            }
            else
            {
                if (flags.HasFlag(InstFlags.Rd))
                {
                    gprMask |= MaskFromIndex(ExtractRd(flags, encoding));
                }

                if (!flags.HasFlag(InstFlags.ReadRt) || name.IsPartialRegisterUpdateMemory())
                {
                    if (flags.HasFlag(InstFlags.Rt))
                    {
                        gprMask |= MaskFromIndex(ExtractRt(flags, encoding));
                    }

                    if (flags.HasFlag(InstFlags.Rt2))
                    {
                        gprMask |= MaskFromIndex(ExtractRt2(flags, encoding));
                    }
                }

                if (flags.HasFlag(InstFlags.Rs))
                {
                    gprMask |= MaskFromIndex(ExtractRs(flags, encoding));
                }
            }

            return (gprMask, fpSimdMask);
        }

        private static uint MaskFromIndex(int index)
        {
            if (index < SpecialZrIndex)
            {
                return 1u << index;
            }

            return 0u;
        }

        private static bool IsFpFromGpr(InstFlags flags, uint encoding)
        {
            InstFlags bothFlags = InstFlags.FpSimdFromGpr | InstFlags.FpSimdToGpr;

            if ((flags & bothFlags) == bothFlags) // FMOV (general)
            {
                return (encoding & (1u << 16)) != 0;
            }

            return flags.HasFlag(InstFlags.FpSimdFromGpr);
        }

        private static bool IsFpToGpr(InstFlags flags, uint encoding)
        {
            InstFlags bothFlags = InstFlags.FpSimdFromGpr | InstFlags.FpSimdToGpr;

            if ((flags & bothFlags) == bothFlags) // FMOV (general)
            {
                return (encoding & (1u << 16)) == 0;
            }

            return flags.HasFlag(InstFlags.FpSimdToGpr);
        }

        private static int GetRtSequenceCount(InstName name, uint encoding)
        {
            switch (name)
            {
                case InstName.Ld1AdvsimdMultAsNoPostIndex:
                case InstName.Ld1AdvsimdMultAsPostIndex:
                case InstName.St1AdvsimdMultAsNoPostIndex:
                case InstName.St1AdvsimdMultAsPostIndex:
                    return ((encoding >> 12) & 0xf) switch
                    {
                        0b0000 => 4,
                        0b0010 => 4,
                        0b0100 => 3,
                        0b0110 => 3,
                        0b0111 => 1,
                        0b1000 => 2,
                        0b1010 => 2,
                        _ => 1,
                    };
                case InstName.Ld1rAdvsimdAsNoPostIndex:
                case InstName.Ld1rAdvsimdAsPostIndex:
                case InstName.Ld1AdvsimdSnglAsNoPostIndex:
                case InstName.Ld1AdvsimdSnglAsPostIndex:
                case InstName.St1AdvsimdSnglAsNoPostIndex:
                case InstName.St1AdvsimdSnglAsPostIndex:
                    return 1;
                case InstName.Ld2rAdvsimdAsNoPostIndex:
                case InstName.Ld2rAdvsimdAsPostIndex:
                case InstName.Ld2AdvsimdMultAsNoPostIndex:
                case InstName.Ld2AdvsimdMultAsPostIndex:
                case InstName.Ld2AdvsimdSnglAsNoPostIndex:
                case InstName.Ld2AdvsimdSnglAsPostIndex:
                case InstName.St2AdvsimdMultAsNoPostIndex:
                case InstName.St2AdvsimdMultAsPostIndex:
                case InstName.St2AdvsimdSnglAsNoPostIndex:
                case InstName.St2AdvsimdSnglAsPostIndex:
                    return 2;
                case InstName.Ld3rAdvsimdAsNoPostIndex:
                case InstName.Ld3rAdvsimdAsPostIndex:
                case InstName.Ld3AdvsimdMultAsNoPostIndex:
                case InstName.Ld3AdvsimdMultAsPostIndex:
                case InstName.Ld3AdvsimdSnglAsNoPostIndex:
                case InstName.Ld3AdvsimdSnglAsPostIndex:
                case InstName.St3AdvsimdMultAsNoPostIndex:
                case InstName.St3AdvsimdMultAsPostIndex:
                case InstName.St3AdvsimdSnglAsNoPostIndex:
                case InstName.St3AdvsimdSnglAsPostIndex:
                    return 3;
                case InstName.Ld4rAdvsimdAsNoPostIndex:
                case InstName.Ld4rAdvsimdAsPostIndex:
                case InstName.Ld4AdvsimdMultAsNoPostIndex:
                case InstName.Ld4AdvsimdMultAsPostIndex:
                case InstName.Ld4AdvsimdSnglAsNoPostIndex:
                case InstName.Ld4AdvsimdSnglAsPostIndex:
                case InstName.St4AdvsimdMultAsNoPostIndex:
                case InstName.St4AdvsimdMultAsPostIndex:
                case InstName.St4AdvsimdSnglAsNoPostIndex:
                case InstName.St4AdvsimdSnglAsPostIndex:
                    return 4;
            }

            return 1;
        }

        private static int GetRnSequenceCount(uint encoding)
        {
            return ((int)(encoding >> 13) & 3) + 1;
        }

        public static int ExtractRd(InstFlags flags, uint encoding)
        {
            Debug.Assert(flags.HasFlag(InstFlags.Rd));
            int index = (int)(encoding >> RdRtBit) & 0x1f;

            if (!flags.HasFlag(InstFlags.RdSP) && index == ZrIndex)
            {
                return SpecialZrIndex;
            }

            return index;
        }

        public static int ExtractRn(uint encoding)
        {
            return (int)(encoding >> RnBit) & 0x1f;
        }

        public static int ExtractRn(InstFlags flags, uint encoding)
        {
            Debug.Assert(flags.HasFlag(InstFlags.Rn));
            int index = ExtractRn(encoding);

            if (!flags.HasFlag(InstFlags.RnSP) && index == ZrIndex)
            {
                return SpecialZrIndex;
            }

            return index;
        }

        public static int ExtractRm(uint encoding)
        {
            return (int)(encoding >> RmRsBit) & 0x1f;
        }

        public static int ExtractRm(InstFlags flags, uint encoding)
        {
            Debug.Assert(flags.HasFlag(InstFlags.Rm));
            int index = ExtractRm(encoding);

            return index == ZrIndex ? SpecialZrIndex : index;
        }

        public static int ExtractRs(uint encoding)
        {
            return (int)(encoding >> RmRsBit) & 0x1f;
        }

        public static int ExtractRs(InstFlags flags, uint encoding)
        {
            Debug.Assert(flags.HasFlag(InstFlags.Rs));
            int index = ExtractRs(encoding);

            return index == ZrIndex ? SpecialZrIndex : index;
        }

        public static int ExtractRa(InstFlags flags, uint encoding)
        {
            Debug.Assert(flags.HasFlag(InstFlags.Ra));
            int index = (int)(encoding >> RaRt2Bit) & 0x1f;

            return index == ZrIndex ? SpecialZrIndex : index;
        }

        public static int ExtractRt(uint encoding)
        {
            return (int)(encoding >> RdRtBit) & 0x1f;
        }

        public static int ExtractRt(InstFlags flags, uint encoding)
        {
            Debug.Assert(flags.HasFlag(InstFlags.Rt));
            int index = ExtractRt(encoding);

            return index == ZrIndex ? SpecialZrIndex : index;
        }

        public static int ExtractRt2(InstFlags flags, uint encoding)
        {
            Debug.Assert(flags.HasFlag(InstFlags.Rt2));
            int index = (int)(encoding >> RaRt2Bit) & 0x1f;

            return index == ZrIndex ? SpecialZrIndex : index;
        }
    }
}
