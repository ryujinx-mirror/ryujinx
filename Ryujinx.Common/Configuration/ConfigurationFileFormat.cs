using JsonPrettyPrinterPlus;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;
using Ryujinx.Configuration.System;
using Ryujinx.Configuration.Hid;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.UI.Input;
using Ryujinx.Configuration.Ui;

namespace Ryujinx.Configuration
{
    public class ConfigurationFileFormat
    {
        /// <summary>
        /// The current version of the file format
        /// </summary>
        public const int CurrentVersion = 4;

        public int Version { get; set; }

        /// <summary>
        /// Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.
        /// </summary>
        public float MaxAnisotropy { get; set; }

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
        /// Enables or disables Docked Mode
        /// </summary>
        public bool DockedMode { get; set; }

        /// <summary>
        /// Enables or disables Discord Rich Presence
        /// </summary>
        public bool EnableDiscordIntegration { get; set; }

        /// <summary>
        /// Enables or disables Vertical Sync
        /// </summary>
        public bool EnableVsync { get; set; }

        /// <summary>
        /// Enables or disables multi-core scheduling of threads
        /// </summary>
        public bool EnableMulticoreScheduling { get; set; }

        /// <summary>
        /// Enables integrity checks on Game content files
        /// </summary>
        public bool EnableFsIntegrityChecks { get; set; }

        /// <summary>
        /// Enables FS access log output to the console. Possible modes are 0-3
        /// </summary>
        public int FsGlobalAccessLogMode { get; set; }

        /// <summary>
        /// Enable or disable ignoring missing services
        /// </summary>
        public bool IgnoreMissingServices { get; set; }

        /// <summary>
        ///  The primary controller's type
        /// </summary>
        public ControllerType ControllerType { get; set; }

        /// <summary>
        /// Used to toggle columns in the GUI
        /// </summary>
        public GuiColumns GuiColumns { get; set; }

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
        /// Enable or disable keyboard support (Independent from controllers binding)
        /// </summary>
        public bool EnableKeyboard { get; set; }

        /// <summary>
        /// Keyboard control bindings
        /// </summary>
        public NpadKeyboard KeyboardControls { get; set; }

        /// <summary>
        /// Controller control bindings
        /// </summary>
        public NpadController JoystickControls { get; set; }

        /// <summary>
        /// Loads a configuration file from disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public static ConfigurationFileFormat Load(string path)
        {
            var resolver = CompositeResolver.Create(
                new[] { new ConfigurationEnumFormatter<Key>() },
                new[] { StandardResolver.AllowPrivateSnakeCase }
            );

            using (Stream stream = File.OpenRead(path))
            {
                return JsonSerializer.Deserialize<ConfigurationFileFormat>(stream, resolver);
            }
        }

        /// <summary>
        /// Save a configuration file to disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public void SaveConfig(string path)
        {
            IJsonFormatterResolver resolver = CompositeResolver.Create(
                new[] { new ConfigurationEnumFormatter<Key>()  },
                new[] { StandardResolver.AllowPrivateSnakeCase }
            );

            byte[] data = JsonSerializer.Serialize(this, resolver);
            File.WriteAllText(path, Encoding.UTF8.GetString(data, 0, data.Length).PrettyPrintJson());
        }

        public class ConfigurationEnumFormatter<T> : IJsonFormatter<T>
            where T : struct
        {
            public void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
            {
                formatterResolver.GetFormatterWithVerify<string>()
                                 .Serialize(ref writer, value.ToString(), formatterResolver);
            }

            public T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
            {
                if (reader.ReadIsNull())
                {
                    return default(T);
                }

                string enumName = formatterResolver.GetFormatterWithVerify<string>()
                                                   .Deserialize(ref reader, formatterResolver);

                if (Enum.TryParse<T>(enumName, out T result))
                {
                    return result;
                }

                return default(T);
            }
        }
    }
}