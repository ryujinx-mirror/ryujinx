namespace ARMeilleure.Common
{
    public enum PoolSizeIncrement
    {
        Default = 200
    }

    public enum ChunkSizeLimit
    {
        Large = 200000 / PoolSizeIncrement.Default,
        Medium = 100000 / PoolSizeIncrement.Default,
        Small = 50000 / PoolSizeIncrement.Default
    }
}