using System.Runtime.Intrinsics.X86;

public static class AOptimizations
{
    public static bool GenerateCallStack = true;

    private static bool UseAllSseIfAvailable = true;

    private static bool UseSseIfAvailable   = true;
    private static bool UseSse2IfAvailable  = true;
    private static bool UseSse41IfAvailable = true;
    private static bool UseSse42IfAvailable = true;

    internal static bool UseSse   = (UseAllSseIfAvailable && UseSseIfAvailable)   && Sse.IsSupported;
    internal static bool UseSse2  = (UseAllSseIfAvailable && UseSse2IfAvailable)  && Sse2.IsSupported;
    internal static bool UseSse41 = (UseAllSseIfAvailable && UseSse41IfAvailable) && Sse41.IsSupported;
    internal static bool UseSse42 = (UseAllSseIfAvailable && UseSse42IfAvailable) && Sse42.IsSupported;
}