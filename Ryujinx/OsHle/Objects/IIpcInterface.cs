using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Objects
{
    interface IIpcInterface
    {
        IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; } 
    }
}