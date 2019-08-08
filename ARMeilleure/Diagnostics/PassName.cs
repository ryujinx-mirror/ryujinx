namespace ARMeilleure.Diagnostics
{
    enum PassName
    {
        Decoding,
        Translation,
        RegisterUsage,
        Dominance,
        SsaConstruction,
        Optimization,
        PreAllocation,
        RegisterAllocation,
        CodeGeneration,

        Count
    }
}