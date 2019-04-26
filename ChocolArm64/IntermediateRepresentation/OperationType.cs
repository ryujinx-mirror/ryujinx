namespace ChocolArm64.IntermediateRepresentation
{
    enum OperationType
    {
        Call,
        CallVirtual,
        IL,
        ILBranch,
        LoadArgument,
        LoadConstant,
        LoadContext,
        LoadField,
        LoadLocal,
        MarkLabel,
        StoreContext,
        StoreLocal
    }
}