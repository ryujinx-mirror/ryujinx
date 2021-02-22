namespace ARMeilleure.Common
{
    static class BitMapPool
    {
        public static BitMap Allocate(int initialCapacity)
        {
            return BitMap().Reset(initialCapacity);
        }

        #region "ThreadStaticPool"
        public static void PrepareBitMapPool(int groupId = 0)
        {
            ThreadStaticPool<BitMap>.PreparePool(groupId, ChunkSizeLimit.Small);
        }

        private static BitMap BitMap()
        {
            return ThreadStaticPool<BitMap>.Instance.Allocate();
        }

        public static void ResetBitMapPool(int groupId = 0)
        {
            ThreadStaticPool<BitMap>.ResetPool(groupId);
        }

        public static void DisposeBitMapPools()
        {
            ThreadStaticPool<BitMap>.DisposePools();
        }
        #endregion
    }
}
