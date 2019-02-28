using System.Runtime.Intrinsics.X86;

public static class Optimizations
{
    public static bool AssumeStrictAbiCompliance { get; set; }

    public static bool FastFP { get; set; } = true;

    private const bool UseAllSseIfAvailable = true;

    public static bool UseSseIfAvailable   { get; set; } = UseAllSseIfAvailable;
    public static bool UseSse2IfAvailable  { get; set; } = UseAllSseIfAvailable;
    public static bool UseSse3IfAvailable  { get; set; } = UseAllSseIfAvailable;
    public static bool UseSsse3IfAvailable { get; set; } = UseAllSseIfAvailable;
    public static bool UseSse41IfAvailable { get; set; } = UseAllSseIfAvailable;
    public static bool UseSse42IfAvailable { get; set; } = UseAllSseIfAvailable;

    internal static bool UseSse   => UseSseIfAvailable   && Sse.IsSupported;
    internal static bool UseSse2  => UseSse2IfAvailable  && Sse2.IsSupported;
    internal static bool UseSse3  => UseSse3IfAvailable  && Sse3.IsSupported;
    internal static bool UseSsse3 => UseSsse3IfAvailable && Ssse3.IsSupported;
    internal static bool UseSse41 => UseSse41IfAvailable && Sse41.IsSupported;
    internal static bool UseSse42 => UseSse42IfAvailable && Sse42.IsSupported;
}