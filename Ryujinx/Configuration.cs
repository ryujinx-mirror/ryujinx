using ARMeilleure;
using LibHac.Fs;
using OpenTK.Input;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.HOS.Services;
using Ryujinx.HLE.Input;
using Ryujinx.UI.Input;
using System;
using System.IO;
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
        public string GraphicsShadersDumpPath { get; private set; }

        /// <summary>
        /// Enables printing debug log messages
        /// </summary>
        public bool LoggingEnableDebug { get; private set; }

        /// <summary>
        /// Enables printing stub log messages
        /// </summary>
        public bool LoggingEnableStub { get; private set; }

        /// <summary>
        /// Enables printing info log messages
        /// </summary>
        public bool LoggingEnableInfo { get; private set; }

        /// <summary>
        /// Enables printing warning log messages
        /// </summary>
        public bool LoggingEnableWarn { get; private set; }

        /// <summary>
        /// Enables printing error log messages
        /// </summary>
        public bool LoggingEnableError { get; private set; }

        /// <summary>
        /// Enables printing guest log messages
        /// </summary>
        public bool LoggingEnableGuest { get; private set; }

        /// <summary>
        /// Enables printing FS access log messages
        /// </summary>
        public bool LoggingEnableFsAccessLog { get; private set; }

        /// <summary>
        /// Controls which log messages are written to the log targets
        /// </summary>
        public LogClass[] LoggingFilteredClasses { get; private set; }

        /// <summary>
        /// Enables or disables logging to a file on disk
        /// </summary>
        public bool EnableFileLog { get; private set; }

        /// <summary>
        /// Change System Language
        /// </summary>
        public SystemLanguage SystemLanguage { get; private set; }

        /// <summary>
        /// Enables or disables Docked Mode
        /// </summary>
        public bool DockedMode { get; private set; }

        /// <summary>
        /// Enables or disables Discord Rich Presence
        /// </summary>
        public bool EnableDiscordIntegration { get; private set; }

        /// <summary>
        /// Enables or disables Vertical Sync
        /// </summary>
        public bool EnableVsync { get; private set; }

        /// <summary>
        /// Enables or disables multi-core scheduling of threads
        /// </summary>
        public bool EnableMulticoreScheduling { get; private set; }

        /// <summary>
        /// Enables integrity checks on Game content files
        /// </summary>
        public bool EnableFsIntegrityChecks { get; private set; }

        /// <summary>
        /// Enables FS access log output to the console. Possible modes are 0-3
        /// </summary>
        public int FsGlobalAccessLogMode { get; private set; }

        /// <summary>
        /// Use old ChocolArm64 ARM emulator
        /// </summary>
        public bool EnableLegacyJit { get; private set; }

        /// <summary>
        /// Enable or disable ignoring missing services
        /// </summary>
        public bool IgnoreMissingServices { get; private set; }

        /// <summary>
        ///  The primary controller's type
        /// </summary>
        public ControllerStatus ControllerType { get; private set; }

        /// <summary>
        /// Enable or disable keyboard support (Independent from controllers binding)
        /// </summary>
        public bool EnableKeyboard { get; private set; }

        /// <summary>
        /// Keyboard control bindings
        /// </summary>
        public NpadKeyboard KeyboardControls { get; private set; }

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
            var resolver = CompositeResolver.Create(
                new[] { new ConfigurationEnumFormatter<Key>() },
                new[] { StandardResolver.AllowPrivateSnakeCase }
            );

            using (Stream stream = File.OpenRead(path))
            {
                Instance = await JsonSerializer.DeserializeAsync<Configuration>(stream, resolver);
            }
        }

        /// <summary>
        /// Configures a <see cref="Switch"/> instance
        /// </summary>
        /// <param name="device">The instance to configure</param>
        public static void Configure(Switch device)
        {
            if (Instance == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet.");
            }

            GraphicsConfig.ShadersDumpPath = Instance.GraphicsShadersDumpPath;

            Logger.AddTarget(new AsyncLogTargetWrapper(
                new ConsoleLogTarget(),
                1000,
                AsyncLogTargetOverflowAction.Block
            ));

            if (Instance.EnableFileLog)
            {
                Logger.AddTarget(new AsyncLogTargetWrapper(
                    new FileLogTarget(Path.Combine(Program.ApplicationDirectory, "Ryujinx.log")),
                    1000,
                    AsyncLogTargetOverflowAction.Block
                ));
            }

            Logger.SetEnable(LogLevel.Debug,     Instance.LoggingEnableDebug);
            Logger.SetEnable(LogLevel.Stub,      Instance.LoggingEnableStub);
            Logger.SetEnable(LogLevel.Info,      Instance.LoggingEnableInfo);
            Logger.SetEnable(LogLevel.Warning,   Instance.LoggingEnableWarn);
            Logger.SetEnable(LogLevel.Error,     Instance.LoggingEnableError);
            Logger.SetEnable(LogLevel.Guest,     Instance.LoggingEnableGuest);
            Logger.SetEnable(LogLevel.AccessLog, Instance.LoggingEnableFsAccessLog);

            if (Instance.LoggingFilteredClasses.Length > 0)
            {
                foreach (var logClass in EnumExtensions.GetValues<LogClass>())
                {
                    Logger.SetEnable(logClass, false);
                }

                foreach (var logClass in Instance.LoggingFilteredClasses)
                {
                    Logger.SetEnable(logClass, true);
                }
            }

            device.System.State.DiscordIntegrationEnabled = Instance.EnableDiscordIntegration;

            device.EnableDeviceVsync = Instance.EnableVsync;

            device.System.State.DockedMode = Instance.DockedMode;

            device.System.State.SetLanguage(Instance.SystemLanguage);

            if (Instance.EnableMulticoreScheduling)
            {
                device.System.EnableMultiCoreScheduling();
            }

            device.System.FsIntegrityCheckLevel = Instance.EnableFsIntegrityChecks
                ? IntegrityCheckLevel.ErrorOnInvalid
                : IntegrityCheckLevel.None;

            device.System.GlobalAccessLogMode = Instance.FsGlobalAccessLogMode;

            device.System.UseLegacyJit = Instance.EnableLegacyJit;

            ServiceConfiguration.IgnoreMissingServices = Instance.IgnoreMissingServices;

            if (Instance.JoystickControls.Enabled)
            {
                if (!Joystick.GetState(Instance.JoystickControls.Index).IsConnected)
                {
                    Instance.JoystickControls.SetEnabled(false);
                }
            }

            device.Hid.InitializePrimaryController(Instance.ControllerType);
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