namespace Ryujinx.Common.Memory
{
    public class Box<T> where T : unmanaged
    {
        public T Data;

        public Box()
        {
            Data = new T();
        }
    }
}
