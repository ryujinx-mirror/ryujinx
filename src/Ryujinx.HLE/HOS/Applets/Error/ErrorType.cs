namespace Ryujinx.HLE.HOS.Applets.Error
{
    enum ErrorType : byte
    {
        ErrorCommonArg,
        SystemErrorArg,
        ApplicationErrorArg,
        ErrorEulaArg,
        ErrorPctlArg,
        ErrorRecordArg,
        SystemUpdateEulaArg = 8,
    }
}
