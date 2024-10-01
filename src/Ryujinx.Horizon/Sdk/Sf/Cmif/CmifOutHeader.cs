using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    struct CmifOutHeader
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint Magic;
        public uint Version;
        public Result Result;
        public uint Token;
#pragma warning restore CS0649
    }
}
