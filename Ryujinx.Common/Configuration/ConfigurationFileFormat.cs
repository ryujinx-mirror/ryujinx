using System.Collections.Generic;
using System.IO;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.Configuration.System;
using Ryujinx.Configuration.Ui;

namespace Ryujinx.Configuration
{
    public class ConfigurationFileFormat
    {
        /// <summary>
        /// The current version of the file format
        /// </summary>
        public const int CurrentVersion = 22;

        public int Version { get; set; }

        /// <summary>
        /// Resolution Scale. An integer scale applied to applicable render targets. Values 1-4, or -1 to use a custom floating point scale instead.
        /// </summary>
        public int ResScale { get; set; }

        /// <summary>
        /// Custom Resolution Scale. A custom floating point scale applied to applicable render targets. Only active when Resolution Scale is -1.
        /// </summary>
        public float ResScaleCustom { get; set; }

        /// <summary>
        /// Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.
        /// </summary>
        public float MaxAnisotropy { get; set; }

        /// <summary>
        /// Aspect Ratio applied to the renderer window.
        /// </summary>
        public AspectRatio AspectRatio { get; set; }

        /// <summary>
        /// Dumps shaders in this local directory
        /// </summary>
        public string GraphicsShadersDumpPath { get; set; }

        /// <summary>
        /// Enables printing debug log messages
        /// </summary>
        public bool LoggingEnableDebug { get; set; }

        /// <summary>
        /// Enables printing stub log messages
        /// </summary>
        public bool LoggingEnableStub { get; set; }

        /// <summary>
        /// Enables printing info log messages
        /// </summary>
        public bool LoggingEnableInfo { get; set; }

        /// <summary>
        /// Enables printing warning log messages
        /// </summary>
        public bool LoggingEnableWarn { get; set; }

        /// <summary>
        /// Enables printing error log messages
        /// </summary>
        public bool LoggingEnableError { get; set; }

        /// <summary>
        /// Enables printing guest log messages
        /// </summary>
        public bool LoggingEnableGuest { get; set; }

        /// <summary>
        /// Enables printing FS access log messages
        /// </summary>
        public bool LoggingEnableFsAccessLog { get; set; }

        /// <summary>
        /// Controls which log messages are written to the log targets
        /// </summary>
        public LogClass[] LoggingFilteredClasses { get; set; }

        /// <summary>
        /// Change Graphics API debug log level
        /// </summary>
        public GraphicsDebugLevel LoggingGraphicsDebugLevel { get; set; }

        /// <summary>
        /// Enables or disables logging to a file on disk
        /// </summary>
        public bool EnableFileLog { get; set; }

        /// <summary>
        /// Change System Language
        /// </summary>
        public Language SystemLanguage { get; set; }

        /// <summary>
        /// Change System Region
        /// </summary>
        public Region SystemRegion { get; set; }

        /// <summary>
        /// Change System TimeZone
        /// </summary>
        public string SystemTimeZone { get; set; }

        /// <summary>
        /// Change System Time Offset in seconds
        /// </summary>
        public long SystemTimeOffset { get; set; }

        /// <summary>
        /// Enables or disables Docked Mode
        /// </summary>
        public bool DockedMode { get; set; }

        /// <summary>
        /// Enables or disables Discord Rich Presence
        /// </summary>
        public bool EnableDiscordIntegration { get; set; }

        /// <summary>
        /// Checks for updates when Ryujinx starts when enabled
        /// </summary>
        public bool CheckUpdatesOnStart { get; set; }

        /// <summary>
        /// Show "Confirm Exit" Dialog
        /// </summary>
        public bool ShowConfirmExit { get; set; }

        /// <summary>
        /// Hide Cursor on Idle
        /// </summary>
        public bool HideCursorOnIdle { get; set; }

        /// <summary>
        /// Enables or disables Vertical Sync
        /// </summary>
        public bool EnableVsync { get; set; }

        /// <summary>
        /// Enables or disables Shader cache
        /// </summary>
        public bool EnableShaderCache { get; set; }

        /// <summary>
        /// Enables or disables multi-core scheduling of threads
        /// </summary>
        public bool EnableMulticoreScheduling { get; set; }

        /// <summary>
        /// Enables or disables profiled translation cache persistency
        /// </summary>
        public bool EnablePtc { get; set; }

        /// <summary>
        /// Enables integrity checks on Game content files
        /// </summary>
        public bool EnableFsIntegrityChecks { get; set; }

        /// <summary>
        /// Enables FS access log output to the console. Possible modes are 0-3
        /// </summary>
        public int FsGlobalAccessLogMode { get; set; }

        /// <summary>
        /// The selected audio backend
        /// </summary>
        public AudioBackend AudioBackend { get; set; }

        /// <summary>
        /// Enable or disable ignoring missing services
        /// </summary>
        public bool IgnoreMissingServices { get; set; }

        /// <summary>
        /// Used to toggle columns in the GUI
        /// </summary>
        public GuiColumns GuiColumns { get; set; }

        /// <summary>
        /// Used to configure column sort settings in the GUI
        /// </summary>
        public ColumnSort ColumnSort { get; set; }

        /// <summary>
        /// A list of directories containing games to be used to load games into the games list
        /// </summary>
        public List<string> GameDirs { get; set; }

        /// <summary>
        /// Enable or disable custom themes in the GUI
        /// </summary>
        public bool EnableCustomTheme { get; set; }

        /// <summary>
        /// Path to custom GUI theme
        /// </summary>
        public string CustomThemePath { get; set; }

        /// <summary>
        /// Start games in fullscreen mode
        /// </summary>
        public bool StartFullscreen { get; set; }

        /// <summary>
        /// Enable or disable keyboard support (Independent from controllers binding)
        /// </summary>
        public bool EnableKeyboard { get; set; }

        /// <summary>
        /// Hotkey Keyboard Bindings
        /// </summary>
        public KeyboardHotkeys Hotkeys { get; set; }

        /// <summary>
        /// Keyboard control bindings
        /// </summary>
        public List<KeyboardConfig> KeyboardConfig { get; set; }

        /// <summary>
        /// Controller control bindings
        /// </summary>
        public List<ControllerConfig> ControllerConfig { get; set; }

        /// <summary>
        /// Loads a configuration file from disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public static bool TryLoad(string path, out ConfigurationFileFormat configurationFileFormat)
        {
            try
            {
                configurationFileFormat = JsonHelper.DeserializeFromFile<ConfigurationFileFormat>(path);

                return true;
            }
            catch
            {
                configurationFileFormat = null;

                return false;
            }
        }

        /// <summary>
        /// Save a configuration file to disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public void SaveConfig(string path)
        {
            using FileStream fileStream = File.Create(path, 4096, FileOptions.WriteThrough);
            JsonHelper.Serialize(fileStream, this, true);
        }
    }
}
