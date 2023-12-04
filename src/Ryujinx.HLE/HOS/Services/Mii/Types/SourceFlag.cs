using System;

namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    [Flags]
    enum SourceFlag
    {
        Database = 1 << Source.Database,
        Default = 1 << Source.Default,
        All = Database | Default,
    }
}
