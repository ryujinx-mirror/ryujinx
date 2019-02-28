using ChocolArm64.State;

namespace ChocolArm64.Translation
{
    struct TranslatorQueueItem
    {
        public long Position { get; }

        public ExecutionMode Mode { get; }

        public TranslationTier Tier { get; }

        public bool IsComplete { get; }

        public TranslatorQueueItem(
            long            position,
            ExecutionMode   mode,
            TranslationTier tier,
            bool            isComplete = false)
        {
            Position   = position;
            Mode       = mode;
            Tier       = tier;
            IsComplete = isComplete;
        }
    }
}