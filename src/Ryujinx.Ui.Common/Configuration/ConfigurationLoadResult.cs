namespace Ryujinx.Ui.Common.Configuration
{
    public enum ConfigurationLoadResult
    {
        Success = 0,
        NotLoaded = 1,
        MigratedFromPreVulkan = 1 << 8,
    }
}
