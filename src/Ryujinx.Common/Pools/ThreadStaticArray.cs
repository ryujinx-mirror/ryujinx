using System;

namespace Ryujinx.Common.Pools
{
    public static class ThreadStaticArray<T>
    {
        [ThreadStatic]
        private static T[] _array;

        public static ref T[] Get()
        {
            _array ??= new T[1];

            return ref _array;
        }
    }
}
