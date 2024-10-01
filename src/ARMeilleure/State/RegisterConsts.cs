namespace ARMeilleure.State
{
    static class RegisterConsts
    {
        public const int IntRegsCount = 32;
        public const int VecRegsCount = 32;
        public const int FlagsCount = 32;
        public const int FpFlagsCount = 32;
        public const int IntAndVecRegsCount = IntRegsCount + VecRegsCount;
        public const int FpFlagsOffset = IntRegsCount + VecRegsCount + FlagsCount;
        public const int TotalCount = IntRegsCount + VecRegsCount + FlagsCount + FpFlagsCount;

        public const int ZeroIndex = 31;
    }
}
