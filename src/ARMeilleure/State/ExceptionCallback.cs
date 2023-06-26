namespace ARMeilleure.State
{
    public delegate void ExceptionCallbackNoArgs(ExecutionContext context);
    public delegate void ExceptionCallback(ExecutionContext context, ulong address, int id);
}
