namespace Ryujinx.HLE.HOS.Services.Apm
{
    enum CpuBoostMode
    {
        Disabled = 0,
        Mode1    = 1, // Use PerformanceConfiguration13 and PerformanceConfiguration14, or PerformanceConfiguration15 and PerformanceConfiguration16
        Mode2    = 2  // Use PerformanceConfiguration15 and PerformanceConfiguration16.
    }
}