using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices
{
    interface IIpcService
    {
        IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; } 
    }
}