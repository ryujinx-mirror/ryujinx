namespace Ryujinx.Debugger.Profiler
{
    public class ProfilerSettings
    {
        // Default settings for profiler
        public bool   Enabled         { get; set; } = false;
        public bool   FileDumpEnabled { get; set; } = false;
        public string DumpLocation    { get; set; } = "";
        public float  UpdateRate      { get; set; } = 0.1f;
        public int    MaxLevel        { get; set; } = 0;
        public int    MaxFlags        { get; set; } = 1000;

        // 19531225 = 5 seconds in ticks on most pc's.
        // It should get set on boot to the time specified in config
        public long History { get; set; } = 19531225;
    }
}
