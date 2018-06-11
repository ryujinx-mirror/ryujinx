using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services
{
    interface IIpcService
    {
        IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; }
    }
}