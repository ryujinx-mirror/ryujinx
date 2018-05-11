using System.Runtime.Intrinsics.X86;

public static class AOptimizations
{
    public static bool DisableMemoryChecks = false;

    public static bool GenerateCallStack = true;

    public static bool UseSse2IfAvailable = true;

    internal static bool UseSse2 = UseSse2IfAvailable && Sse2.IsSupported;
}