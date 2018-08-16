using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services
{
    interface IIpcService
    {
        IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; }
    }
}