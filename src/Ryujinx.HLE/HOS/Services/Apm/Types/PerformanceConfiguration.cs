namespace Ryujinx.HLE.HOS.Services.Apm
{
    enum PerformanceConfiguration : uint  // Clocks are all in MHz.
    {                                            // CPU  | GPU   | RAM    | NOTE
        PerformanceConfiguration1  = 0x00010000, // 1020 | 384   | 1600   | Only available while docked.
        PerformanceConfiguration2  = 0x00010001, // 1020 | 768   | 1600   | Only available while docked.
        PerformanceConfiguration3  = 0x00010002, // 1224 | 691.2 | 1600   | Only available for SDEV units.
        PerformanceConfiguration4  = 0x00020000, // 1020 | 230.4 | 1600   | Only available for SDEV units.
        PerformanceConfiguration5  = 0x00020001, // 1020 | 307.2 | 1600   |
        PerformanceConfiguration6  = 0x00020002, // 1224 | 230.4 | 1600   |
        PerformanceConfiguration7  = 0x00020003, // 1020 | 307   | 1331.2 |
        PerformanceConfiguration8  = 0x00020004, // 1020 | 384   | 1331.2 |
        PerformanceConfiguration9  = 0x00020005, // 1020 | 307.2 | 1065.6 |
        PerformanceConfiguration10 = 0x00020006, // 1020 | 384   | 1065.6 |
        PerformanceConfiguration11 = 0x92220007, // 1020 | 460.8 | 1600   |
        PerformanceConfiguration12 = 0x92220008, // 1020 | 460.8 | 1331.2 |
        PerformanceConfiguration13 = 0x92220009, // 1785 | 768   | 1600   | 7.0.0+
        PerformanceConfiguration14 = 0x9222000A, // 1785 | 768   | 1331.2 | 7.0.0+
        PerformanceConfiguration15 = 0x9222000B, // 1020 | 768   | 1600   | 7.0.0+
        PerformanceConfiguration16 = 0x9222000C  // 1020 | 768   | 1331.2 | 7.0.0+
    }
}