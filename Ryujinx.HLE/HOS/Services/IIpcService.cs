using System.Collections.Generic;
using System.Reflection;

namespace Ryujinx.HLE.HOS.Services
{
    interface IIpcService
    {
        IReadOnlyDictionary<int, MethodInfo> Commands { get; }
    }
}