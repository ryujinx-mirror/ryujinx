namespace Ryujinx.Common.Logging
{
    interface ILogFormatter
    {
        string Format(LogEventArgs args);
    }
}
