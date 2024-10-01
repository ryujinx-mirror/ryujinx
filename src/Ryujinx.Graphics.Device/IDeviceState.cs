namespace Ryujinx.Graphics.Device
{
    public interface IDeviceState
    {
        int Read(int offset);
        void Write(int offset, int data);
    }
}
