using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Objects
{
    interface IIpcInterface
    {
        IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; } 
    }
}