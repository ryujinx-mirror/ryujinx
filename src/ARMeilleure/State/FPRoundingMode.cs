namespace ARMeilleure.State
{
    public enum FPRoundingMode
    {
        ToNearest = 0, // With ties to even.
        TowardsPlusInfinity = 1,
        TowardsMinusInfinity = 2,
        TowardsZero = 3,
        ToNearestAway = 4, // With ties to away.
    }
}
