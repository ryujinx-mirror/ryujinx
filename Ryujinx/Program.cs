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

            GLib.ExceptionManager.UnhandledException += Glib_UnhandledException;

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

            Logger.PrintInfo(LogClass.Application, $"Ryujinx Version: {Version}");

            Logger.PrintInfo(LogClass.Application, $"Operating System: {SystemInfo.Instance.OsDescription}");
            Logger.PrintInfo(LogClass.Application, $"CPU: {SystemInfo.Instance.CpuName}");
            Logger.PrintInfo(LogClass.Application, $"Total RAM: {SystemInfo.Instance.RamSizeInMB}");

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

        private static void Glib_UnhandledException(GLib.UnhandledExceptionArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            Logger.PrintError(LogClass.Application, $"Unhandled exception caught: {exception}");

            if (e.IsTerminating)
            {
                Logger.Shutdown();
            }
        }
    }
}