namespace ARMeilleure.Diagnostics
{
    enum PassName
    {
        Decoding,
        Translation,
        RegisterUsage,
        TailMerge,
        Dominance,
        SsaConstruction,
        RegisterToLocal,
        Optimization,
        PreAllocation,
        RegisterAllocation,
        CodeGeneration,

        Count,
    }
}
