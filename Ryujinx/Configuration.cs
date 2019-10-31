using JsonPrettyPrinterPlus;
using LibHac.FsSystem;
using OpenTK.Input;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.HOS.Services;
using Ryujinx.HLE.Input;
using Ryujinx.UI;
using Ryujinx.UI.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Ryujinx
{
    public class Configuration
    {
        /// <summary>
        /// The default configuration instance
        /// </summary>
        public static Configuration Instance { get; private set; }

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
        public SystemLanguage SystemLanguage { get; set; }

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
        public ControllerStatus ControllerType { get; set; }

        /// <summary>
        /// Used to toggle columns in the GUI
        /// </summary>
        public List<bool> GuiColumns { get; set; }

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
        public UI.Input.NpadController JoystickControls { get; private set; }

        /// <summary>
        /// Loads a configuration file from disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public static void Load(string path)
        {
            var resolver = CompositeResolver.Create(
                new[] { new ConfigurationEnumFormatter<Key>() },
                new[] { StandardResolver.AllowPrivateSnakeCase }
            );

            using (Stream stream = File.OpenRead(path))
            {
                Instance = JsonSerializer.Deserialize<Configuration>(stream, resolver);
            }
        }

        /// <summary>
        /// Loads a configuration file asynchronously from disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public static async Task LoadAsync(string path)
        {
            IJsonFormatterResolver resolver = CompositeResolver.Create(
                new[] { new ConfigurationEnumFormatter<Key>()  },
                new[] { StandardResolver.AllowPrivateSnakeCase }
            );

            using (Stream stream = File.OpenRead(path))
            {
                Instance = await JsonSerializer.DeserializeAsync<Configuration>(stream, resolver);
            }
        }

        /// <summary>
        /// Save a configuration file to disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public static void SaveConfig(Configuration config, string path)
        {
            IJsonFormatterResolver resolver = CompositeResolver.Create(
                new[] { new ConfigurationEnumFormatter<Key>()  },
                new[] { StandardResolver.AllowPrivateSnakeCase }
            );

            byte[] data = JsonSerializer.Serialize(config, resolver);
            File.WriteAllText(path, Encoding.UTF8.GetString(data, 0, data.Length).PrettyPrintJson());
        }

        /// <summary>
        /// Configures a <see cref="Switch"/> instance
        /// </summary>
        /// <param name="device">The instance to configure</param>
        public static void InitialConfigure(Switch device)
        {
            if (Instance == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet.");
            }

            SwitchSettings.ConfigureSettings(Instance);

            Logger.AddTarget(new AsyncLogTargetWrapper(
                new ConsoleLogTarget(),
                1000,
                AsyncLogTargetOverflowAction.Block
            ));

            if (Instance.EnableFileLog)
            {
                Logger.AddTarget(new AsyncLogTargetWrapper(
                    new FileLogTarget(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ryujinx.log")),
                    1000,
                    AsyncLogTargetOverflowAction.Block
                ));
            }

            Configure(device, Instance);
        }

        public static void Configure(Switch device, Configuration SwitchConfig)
        {
            GraphicsConfig.ShadersDumpPath = SwitchConfig.GraphicsShadersDumpPath;

            Logger.SetEnable(LogLevel.Debug,     SwitchConfig.LoggingEnableDebug      );
            Logger.SetEnable(LogLevel.Stub,      SwitchConfig.LoggingEnableStub       );
            Logger.SetEnable(LogLevel.Info,      SwitchConfig.LoggingEnableInfo       );
            Logger.SetEnable(LogLevel.Warning,   SwitchConfig.LoggingEnableWarn       );
            Logger.SetEnable(LogLevel.Error,     SwitchConfig.LoggingEnableError      );
            Logger.SetEnable(LogLevel.Guest,     SwitchConfig.LoggingEnableGuest      );
            Logger.SetEnable(LogLevel.AccessLog, SwitchConfig.LoggingEnableFsAccessLog);

            if (SwitchConfig.LoggingFilteredClasses.Length > 0)
            {
                foreach (var logClass in EnumExtensions.GetValues<LogClass>())
                {
                    Logger.SetEnable(logClass, false);
                }

                foreach (var logClass in SwitchConfig.LoggingFilteredClasses)
                {
                    Logger.SetEnable(logClass, true);
                }
            }

            MainWindow.DiscordIntegrationEnabled = SwitchConfig.EnableDiscordIntegration;

            device.EnableDeviceVsync = SwitchConfig.EnableVsync;

            device.System.State.DockedMode = SwitchConfig.DockedMode;

            device.System.State.SetLanguage(SwitchConfig.SystemLanguage);

            if (SwitchConfig.EnableMulticoreScheduling)
            {
                device.System.EnableMultiCoreScheduling();
            }

            device.System.FsIntegrityCheckLevel = SwitchConfig.EnableFsIntegrityChecks
                ? IntegrityCheckLevel.ErrorOnInvalid
                : IntegrityCheckLevel.None;

            device.System.GlobalAccessLogMode = SwitchConfig.FsGlobalAccessLogMode;

            ServiceConfiguration.IgnoreMissingServices = SwitchConfig.IgnoreMissingServices;
        }

        public static void ConfigureHid(Switch device, Configuration SwitchConfig)
        {
            if (SwitchConfig.JoystickControls.Enabled)
            {
                if (!Joystick.GetState(SwitchConfig.JoystickControls.Index).IsConnected)
                {
                    SwitchConfig.JoystickControls.SetEnabled(false);
                }
            }
            device.Hid.InitializePrimaryController(SwitchConfig.ControllerType);
            device.Hid.InitializeKeyboard();
        }

        private class ConfigurationEnumFormatter<T> : IJsonFormatter<T>
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