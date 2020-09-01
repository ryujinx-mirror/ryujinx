using ARMeilleure.Translation.PTC;
using Gtk;
using OpenTK;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.SystemInfo;
using Ryujinx.Configuration;
using Ryujinx.Ui;
using Ryujinx.Ui.Diagnostic;
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
            // Parse Arguments
            string launchPath = null;
            string baseDirPath = null;
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];

                if (arg == "-r" || arg == "--root-data-dir")
                {
                    if (i + 1 >= args.Length)
                    {
                        Logger.Error?.Print(LogClass.Application, $"Invalid option '{arg}'");
                        continue;
                    }

                    baseDirPath = args[++i];
                }
                else if (launchPath == null)
                {
                    launchPath = arg;
                }
            }

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

            // Setup base data directory
            AppDataManager.Initialize(baseDirPath);

            // Initialize the configuration
            ConfigurationState.Initialize();

            // Initialize the logger system
            LoggerModule.Initialize();

            // Initialize Discord integration
            DiscordIntegrationModule.Initialize();

            string localConfigurationPath   = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
            string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, "Config.json");

            // Now load the configuration as the other subsystems are now registered
            if (File.Exists(localConfigurationPath))
            {
                ConfigurationPath = localConfigurationPath;

                ConfigurationFileFormat configurationFileFormat = ConfigurationFileFormat.Load(localConfigurationPath);

                ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
            }
            else if (File.Exists(appDataConfigurationPath))
            {
                ConfigurationPath = appDataConfigurationPath;

                ConfigurationFileFormat configurationFileFormat = ConfigurationFileFormat.Load(appDataConfigurationPath);

                ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
            }
            else
            {
                // No configuration, we load the default values and save it on disk
                ConfigurationPath = appDataConfigurationPath;

                ConfigurationState.Instance.LoadDefault();
                ConfigurationState.Instance.ToFileFormat().SaveConfig(appDataConfigurationPath);
            }

            PrintSystemInfo();

            Application.Init();

            bool hasGlobalProdKeys = File.Exists(Path.Combine(AppDataManager.KeysDirPath, "prod.keys"));
            bool hasAltProdKeys    = !AppDataManager.IsCustomBasePath && File.Exists(Path.Combine(AppDataManager.KeysDirPathAlt, "prod.keys"));
            if (!hasGlobalProdKeys && !hasAltProdKeys && !Migration.IsMigrationNeeded())
            {
                UserErrorDialog.CreateUserErrorDialog(UserError.NoKeys);
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            if (launchPath != null)
            {
                mainWindow.LoadApplication(launchPath);
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

            if (AppDataManager.IsCustomBasePath)
            {
                Logger.Notice.Print(LogClass.Application, $"Custom Data Directory: {AppDataManager.BaseDirPath}");
            }
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
