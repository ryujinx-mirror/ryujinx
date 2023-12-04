using LibHac.Bcat;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Bcat
{
    internal interface IDeliveryCacheDirectoryService : IServiceObject
    {
        Result GetCount(out int count);
        Result Open(DirectoryName directoryName);
        Result Read(out int entriesRead, Span<DeliveryCacheDirectoryEntry> entriesBuffer);
    }
}
