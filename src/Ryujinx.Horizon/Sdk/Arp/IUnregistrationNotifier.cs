using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Arp
{
    public interface IUnregistrationNotifier
    {
        public Result GetReadableHandle(out int readableHandle);
    }
}
