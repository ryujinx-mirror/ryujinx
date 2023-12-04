namespace Ryujinx.Common
{
    public static class SharedPools
    {
        private static class DefaultPool<T>
            where T : class, new()
        {
            public static readonly ObjectPool<T> Instance = new(() => new T(), 20);
        }

        public static ObjectPool<T> Default<T>()
            where T : class, new()
        {
            return DefaultPool<T>.Instance;
        }
    }
}
