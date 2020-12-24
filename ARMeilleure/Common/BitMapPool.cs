namespace ARMeilleure.Common
{
    static class BitMapPool
    {
        public static BitMap Allocate(int initialCapacity)
        {
            BitMap result = ThreadStaticPool<BitMap>.Instance.Allocate();
            result.Reset(initialCapacity);

            return result;
        }

        public static void Release()
        {
            ThreadStaticPool<BitMap>.Instance.Clear();
        }
    }
}
