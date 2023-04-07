namespace Ryujinx.Graphics.Device
{
    public interface IDeviceStateWithContext : IDeviceState
    {
        long CreateContext();
        void DestroyContext(long id);
        void BindContext(long id);
    }
}
