namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger.Types
{
    class AndroidStrongPointer<T> where T : unmanaged, IFlattenable
    {
        public T Object;

        private bool _hasObject;

        public bool IsNull => !_hasObject;

        public AndroidStrongPointer()
        {
            _hasObject = false;
        }

        public AndroidStrongPointer(T obj)
        {
            Set(obj);
        }

        public void Set(AndroidStrongPointer<T> other)
        {
            Object = other.Object;
            _hasObject = other._hasObject;
        }

        public void Set(T obj)
        {
            Object = obj;
            _hasObject = true;
        }

        public void Reset()
        {
            _hasObject = false;
        }
    }
}
