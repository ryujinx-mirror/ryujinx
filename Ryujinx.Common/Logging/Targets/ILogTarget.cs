using System;

namespace Ryujinx.Common.Logging
{
    public interface ILogTarget : IDisposable
    {
        void Log(object sender, LogEventArgs args);
    }
}
