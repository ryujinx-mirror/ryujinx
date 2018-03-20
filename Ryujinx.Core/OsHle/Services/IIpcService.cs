using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services
{
    interface IIpcService
    {
        IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; } 
    }
}