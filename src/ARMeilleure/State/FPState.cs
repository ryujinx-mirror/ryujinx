namespace ARMeilleure.State
{
    public enum FPState
    {
        // FPSR Flags.
        IocFlag = 0,
        DzcFlag = 1,
        OfcFlag = 2,
        UfcFlag = 3,
        IxcFlag = 4,
        IdcFlag = 7,
        QcFlag = 27,
        VFlag = 28,
        CFlag = 29,
        ZFlag = 30,
        NFlag = 31,

        // FPCR Flags.
        IoeFlag = 8,
        DzeFlag = 9,
        OfeFlag = 10,
        UfeFlag = 11,
        IxeFlag = 12,
        IdeFlag = 15,
        RMode0Flag = 22,
        RMode1Flag = 23,
        FzFlag = 24,
        DnFlag = 25,
        AhpFlag = 26,
    }
}
