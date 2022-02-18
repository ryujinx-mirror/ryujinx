using CommandLine;
using Ryujinx.Common.Configuration;
using Ryujinx.HLE.HOS.SystemState;

namespace Ryujinx.Headless.SDL2
{
    public class Options
    {
        // Input

        [Option("input-profile-1", Required = false, HelpText = "Set the input profile in use for Player 1.")]
        public string InputProfile1Name { get; set; }

        [Option("input-profile-2", Required = false, HelpText = "Set the input profile in use for Player 2.")]
        public string InputProfile2Name { get; set; }

        [Option("input-profile-3", Required = false, HelpText = "Set the input profile in use for Player 3.")]
        public string InputProfile3Name { get; set; }

        [Option("input-profile-4", Required = false, HelpText = "Set the input profile in use for Player 4.")]
        public string InputProfile4Name { get; set; }

        [Option("input-profile-5", Required = false, HelpText = "Set the input profile in use for Player 5.")]
        public string InputProfile5Name { get; set; }

        [Option("input-profile-6", Required = false, HelpText = "Set the input profile in use for Player 5.")]
        public string InputProfile6Name { get; set; }

        [Option("input-profile-7", Required = false, HelpText = "Set the input profile in use for Player 7.")]
        public string InputProfile7Name { get; set; }

        [Option("input-profile-8", Required = false, HelpText = "Set the input profile in use for Player 8.")]
        public string InputProfile8Name { get; set; }

        [Option("input-profile-handheld", Required = false, HelpText = "Set the input profile in use for the Handheld Player.")]
        public string InputProfileHandheldName { get; set; }

        [Option("input-id-1", Required = false, HelpText = "Set the input id in use for Player 1.")]
        public string InputId1 { get; set; }

        [Option("input-id-2", Required = false, HelpText = "Set the input id in use for Player 2.")]
        public string InputId2 { get; set; }

        [Option("input-id-3", Required = false, HelpText = "Set the input id in use for Player 3.")]
        public string InputId3 { get; set; }

        [Option("input-id-4", Required = false, HelpText = "Set the input id in use for Player 4.")]
        public string InputId4 { get; set; }

        [Option("input-id-5", Required = false, HelpText = "Set the input id in use for Player 5.")]
        public string InputId5 { get; set; }

        [Option("input-id-6", Required = false, HelpText = "Set the input id in use for Player 6.")]
        public string InputId6 { get; set; }

        [Option("input-id-7", Required = false, HelpText = "Set the input id in use for Player 7.")]
        public string InputId7 { get; set; }

        [Option("input-id-8", Required = false, HelpText = "Set the input id in use for Player 8.")]
        public string InputId8 { get; set; }

        [Option("input-id-handheld", Required = false, HelpText = "Set the input id in use for the Handheld Player.")]
        public string InputIdHandheld { get; set; }

        [Option("enable-keyboard", Required = false, Default = false, HelpText = "Enable or disable keyboard support (Independent from controllers binding).")]
        public bool? EnableKeyboard { get; set; }

        [Option("enable-mouse", Required = false, Default = false, HelpText = "Enable or disable mouse support.")]
        public bool? EnableMouse { get; set; }

        [Option("list-input-profiles", Required = false, HelpText = "List inputs profiles.")]
        public bool? ListInputProfiles { get; set; }

        [Option("list-inputs-ids", Required = false, HelpText = "List inputs ids.")]
        public bool ListInputIds { get; set; }

        // System

        [Option("enable-ptc", Required = false, Default = true, HelpText = "Enables profiled translation cache persistency.")]
        public bool? EnablePtc { get; set; }

        [Option("enable-internet-connection", Required = false, Default = false, HelpText = "Enables guest Internet connection.")]
        public bool? EnableInternetAccess { get; set; }

        [Option("enable-fs-integrity-checks", Required = false, Default = true, HelpText = "Enables integrity checks on Game content files.")]
        public bool? EnableFsIntegrityChecks { get; set; }

        [Option("fs-global-access-log-mode", Required = false, Default = 0, HelpText = "Enables FS access log output to the console.")]
        public int FsGlobalAccessLogMode { get; set; }

        [Option("enable-vsync", Required = false, Default = true, HelpText = "Enables Vertical Sync.")]
        public bool? EnableVsync { get; set; }

        [Option("enable-shader-cache", Required = false, Default = true, HelpText = "Enables Shader cache.")]
        public bool? EnableShaderCache { get; set; }

        [Option("enable-docked-mode", Required = false, Default = true, HelpText = "Enables Docked Mode.")]
        public bool? EnableDockedMode { get; set; }

        [Option("system-language", Required = false, Default = SystemLanguage.AmericanEnglish, HelpText = "Change System Language.")]
        public SystemLanguage SystemLanguage { get; set; }

        [Option("system-region", Required = false, Default = RegionCode.USA, HelpText = "Change System Region.")]
        public RegionCode SystemRegion { get; set; }

        [Option("system-timezone", Required = false, Default = "UTC", HelpText = "Change System TimeZone.")]
        public string SystemTimeZone { get; set; }

        [Option("system-time-offset", Required = false, Default = 0, HelpText = "Change System Time Offset in seconds.")]
        public long SystemTimeOffset { get; set; }

        [Option("memory-manager-mode", Required = false, Default = MemoryManagerMode.HostMappedUnsafe, HelpText = "The selected memory manager mode.")]
        public MemoryManagerMode MemoryManagerMode { get; set; }

        [Option("audio-volume", Required = false, Default = 1.0f, HelpText ="The audio level (0 to 1).")]
        public float AudioVolume { get; set; }

        // Logging

        [Option("enable-file-logging", Required = false, Default = false, HelpText = "Enables logging to a file on disk.")]
        public bool? EnableFileLog { get; set; }

        [Option("enable-debug-logs", Required = false, Default = false, HelpText = "Enables printing debug log messages.")]
        public bool? LoggingEnableDebug { get; set; }

        [Option("enable-stub-logs", Required = false, Default = true, HelpText = "Enables printing stub log messages.")]
        public bool? LoggingEnableStub { get; set; }

        [Option("enable-info-logs", Required = false, Default = true, HelpText = "Enables printing info log messages.")]
        public bool? LoggingEnableInfo { get; set; }

        [Option("enable-warning-logs", Required = false, Default = true, HelpText = "Enables printing warning log messages.")]
        public bool? LoggingEnableWarning { get; set; }

        [Option("enable-error-logs", Required = false, Default = true, HelpText = "Enables printing error log messages.")]
        public bool? LoggingEnableError { get; set; }

        [Option("enable-trace-logs", Required = false, Default = false, HelpText = "Enables printing trace log messages.")]
        public bool? LoggingEnableTrace { get; set; }

        [Option("enable-guest-logs", Required = false, Default = true, HelpText = "Enables printing guest log messages.")]
        public bool? LoggingEnableGuest { get; set; }

        [Option("enable-fs-access-logs", Required = false, Default = false, HelpText = "Enables printing FS access log messages.")]
        public bool? LoggingEnableFsAccessLog { get; set; }

        [Option("graphics-debug-level", Required = false, Default = GraphicsDebugLevel.None, HelpText = "Change Graphics API debug log level.")]
        public GraphicsDebugLevel LoggingGraphicsDebugLevel { get; set; }

        // Graphics

        [Option("resolution-scale", Required = false, Default = 1, HelpText = "Resolution Scale. A floating point scale applied to applicable render targets.")]
        public float ResScale { get; set; }

        [Option("max-anisotropy", Required = false, Default = -1, HelpText = "Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.")]
        public float MaxAnisotropy { get; set; }

        [Option("aspect-ratio", Required = false, Default = AspectRatio.Fixed16x9, HelpText = "Aspect Ratio applied to the renderer window.")]
        public AspectRatio AspectRatio { get; set; }

        [Option("backend-threading", Required = false, Default = BackendThreading.Auto, HelpText = "Whether or not backend threading is enabled. The \"Auto\" setting will determine whether threading should be enabled at runtime.")]
        public BackendThreading BackendThreading { get; set; }

        [Option("graphics-shaders-dump-path", Required = false, HelpText = "Dumps shaders in this local directory. (Developer only)")]
        public string GraphicsShadersDumpPath { get; set; }

        // Hacks

        [Option("expand-ram", Required = false, Default = false, HelpText = "Expands the RAM amount on the emulated system from 4GB to 6GB.")]
        public bool? ExpandRam { get; set; }

        [Option("ignore-missing-services", Required = false, Default = false, HelpText = "Enable ignoring missing services.")]
        public bool? IgnoreMissingServices { get; set; }

        // Values

        [Value(0, MetaName = "input", HelpText = "Input to load.", Required = true)]
        public string InputPath { get; set; }
    }
}
