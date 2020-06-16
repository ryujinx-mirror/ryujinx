namespace ARMeilleure.CodeGen.Unwinding
{
    enum UnwindPseudoOp
    {
        PushReg    = 0,
        SetFrame   = 1,
        AllocStack = 2,
        SaveReg    = 3,
        SaveXmm128 = 4
    }
}