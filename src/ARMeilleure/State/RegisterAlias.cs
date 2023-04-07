namespace ARMeilleure.State
{
    static class RegisterAlias
    {
        public const int R8Usr  = 8;
        public const int R9Usr  = 9;
        public const int R10Usr = 10;
        public const int R11Usr = 11;
        public const int R12Usr = 12;
        public const int SpUsr  = 13;
        public const int LrUsr  = 14;

        public const int SpHyp = 15;

        public const int LrIrq = 16;
        public const int SpIrq = 17;

        public const int LrSvc = 18;
        public const int SpSvc = 19;

        public const int LrAbt = 20;
        public const int SpAbt = 21;

        public const int LrUnd = 22;
        public const int SpUnd = 23;

        public const int R8Fiq  = 24;
        public const int R9Fiq  = 25;
        public const int R10Fiq = 26;
        public const int R11Fiq = 27;
        public const int R12Fiq = 28;
        public const int SpFiq  = 29;
        public const int LrFiq  = 30;

        public const int Aarch32Sp = 13;
        public const int Aarch32Lr = 14;
        public const int Aarch32Pc = 15;

        public const int Lr = 30;
        public const int Zr = 31;
    }
}