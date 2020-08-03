using ARMeilleure.Translation.PTC;
using Gtk;
using Ryujinx.Common.Logging;
using Ryujinx.Common.SystemInfo;
using Ryujinx.Configuration;
using Ryujinx.Debugger.Profiler;
using Ryujinx.Ui;
using OpenTK;
using System;
using System.IO;
using System.Reflection;

namespace Ryujinx
{
    class Program
    {
        public static string Version { get; private set; }

        public static string ConfigurationPath { get; set; }

        static void Main(string[] args)
        {
            Toolkit.Init(new ToolkitOptions
            {
                Backend = PlatformBackend.PreferNative,
                EnableHighResolution = true
            });

            Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            Console.Title = $"Ryujinx Console {Version}";

            string systemPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("Path", $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")};{systemPath}");

            // Hook unhandled exception and process exit events
            GLib.ExceptionManager.UnhandledException += (GLib.UnhandledExceptionArgs e) => ProcessUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => ProcessUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => ProgramExit();

            // Initialize the configuration
            ConfigurationState.Initialize();

            // Initialize the logger system
            LoggerModule.Initialize();

            // Initialize Discord integration
            DiscordIntegrationModule.Initialize();

            string localConfigurationPath  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
            string globalBasePath          = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx");
            string globalConfigurationPath = Path.Combine(globalBasePath, "Config.json");

            // Now load the configuration as the other subsystems are now registered
            if (File.Exists(localConfigurationPath))
            {
                ConfigurationPath = localConfigurationPath;

                ConfigurationFileFormat configurationFileFormat = ConfigurationFileFormat.Load(localConfigurationPath);

                ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
            }
            else if (File.Exists(globalConfigurationPath))
            {
                ConfigurationPath = globalConfigurationPath;

                ConfigurationFileFormat configurationFileFormat = ConfigurationFileFormat.Load(globalConfigurationPath);

                ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
            }
            else
            {
                // No configuration, we load the default values and save it on disk
                ConfigurationPath = globalConfigurationPath;

                // Make sure to create the Ryujinx directory if needed.
                Directory.CreateDirectory(globalBasePath);

                ConfigurationState.Instance.LoadDefault();
                ConfigurationState.Instance.ToFileFormat().SaveConfig(globalConfigurationPath);
            }

            PrintSystemInfo();

            Profile.Initialize();

            Application.Init();

            string globalProdKeysPath = Path.Combine(globalBasePath, "system", "prod.keys");
            string userProfilePath    = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch", "prod.keys");
            if (!File.Exists(globalProdKeysPath) && !File.Exists(userProfilePath) && !Migration.IsMigrationNeeded())
            {
                GtkDialog.CreateWarningDialog("Key file was not found", "Please refer to `KEYS.md` for more info");
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            if (args.Length == 1)
            {
                mainWindow.LoadApplication(args[0]);
            }

            Application.Run();
        }

        private static void PrintSystemInfo()
        {
            Logger.Notice.Print(LogClass.Application, $"Ryujinx Version: {Version}");

            Logger.Notice.Print(LogClass.Application, $"Operating System: {SystemInfo.Instance.OsDescription}");
            Logger.Notice.Print(LogClass.Application, $"CPU: {SystemInfo.Instance.CpuName}");
            Logger.Notice.Print(LogClass.Application, $"Total RAM: {SystemInfo.Instance.RamSizeInMB}");

            var enabledLogs = Logger.GetEnabledLevels();
            Logger.Notice.Print(LogClass.Application, $"Logs Enabled: {(enabledLogs.Count == 0 ? "<None>" : string.Join(", ", enabledLogs))}");
        }

        private static void ProcessUnhandledException(Exception e, bool isTerminating)
        {
            Ptc.Close();
            PtcProfiler.Stop();

            string message = $"Unhandled exception caught: {e}";

            Logger.Error?.PrintMsg(LogClass.Application, message);

            if (Logger.Error == null) Logger.Notice.PrintMsg(LogClass.Application, message);

            if (isTerminating)
            {
                ProgramExit();
            }
        }

        private static void ProgramExit()
        {
            Ptc.Dispose();
            PtcProfiler.Dispose();

            Logger.Shutdown();
        }
    }
}
