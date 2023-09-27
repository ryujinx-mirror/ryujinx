
namespace Ryujinx.Horizon.Sdk.Fs
{
    public readonly struct FileHandle
    {
        public object Value { get; }

        public FileHandle(object value)
        {
            Value = value;
        }
    }
}
