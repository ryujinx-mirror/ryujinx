namespace ARMeilleure.CodeGen.Unwinding
{
    enum UnwindPseudoOp
    {
        PushReg,
        SetFrame,
        AllocStack,
        SaveReg,
        SaveXmm128
    }
}