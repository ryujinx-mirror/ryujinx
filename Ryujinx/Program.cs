using ARMeilleure.Translation.PTC;
using Gtk;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.GraphicsDriver;
using Ryujinx.Common.Logging;
using Ryujinx.Common.System;
using Ryujinx.Common.SystemInfo;
using Ryujinx.Configuration;
using Ryujinx.Modules;
using Ryujinx.Ui;
using Ryujinx.Ui.Widgets;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Ryujinx
{
    class Program
    {
        public static double WindowScaleFactor { get; private set; }

        public static string Version { get; private set; }

        public static string ConfigurationPath { get; set; }

        public static string CommandLineProfile { get; set; }

        [DllImport("libX11")]
        private extern static int XInitThreads();

        static void Main(string[] args)
        { 
            // Parse Arguments.
            string launchPathArg      = null;
            string baseDirPathArg     = null;
            bool   startFullscreenArg = false;

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

                    baseDirPathArg = args[++i];
                }
                else if (arg == "-p" || arg == "--profile")
                {
                    if (i + 1 >= args.Length)
                    {
                        Logger.Error?.Print(LogClass.Application, $"Invalid option '{arg}'");

                        continue;
                    }

                    CommandLineProfile = args[++i];
                }
                else if (arg == "-f" || arg == "--fullscreen")
                {
                    startFullscreenArg = true;
                }
                else if (launchPathArg == null)
                {
                    launchPathArg = arg;
                }
            }

            // Make process DPI aware for proper window sizing on high-res screens.
            ForceDpiAware.Windows();
            WindowScaleFactor = ForceDpiAware.GetWindowScaleFactor();

            // Delete backup files after updating.
            Task.Run(Updater.CleanupUpdate);

            Version = ReleaseInformations.GetVersion();

            Console.Title = $"Ryujinx Console {Version}";

            // NOTE: GTK3 doesn't init X11 in a multi threaded way.
            // This ends up causing race condition and abort of XCB when a context is created by SPB (even if SPB do call XInitThreads).
            if (OperatingSystem.IsLinux())
            {
                XInitThreads();
            }

            string systemPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("Path", $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")};{systemPath}");

            // Hook unhandled exception and process exit events.
            GLib.ExceptionManager.UnhandledException   += (GLib.UnhandledExceptionArgs e)                => ProcessUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => ProcessUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
            AppDomain.CurrentDomain.ProcessExit        += (object sender, EventArgs e)                   => Exit();

            // Setup base data directory.
            AppDataManager.Initialize(baseDirPathArg);

            // Initialize the configuration.
            ConfigurationState.Initialize();

            // Initialize the logger system.
            LoggerModule.Initialize();

            // Initialize Discord integration.
            DiscordIntegrationModule.Initialize();

            // Sets ImageSharp Jpeg Encoder Quality.
            SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.SetEncoder(JpegFormat.Instance, new JpegEncoder()
            {
                Quality = 100
            });

            string localConfigurationPath   = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
            string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath,            "Config.json");

            // Now load the configuration as the other subsystems are now registered
            ConfigurationPath = File.Exists(localConfigurationPath)
                ? localConfigurationPath
                : File.Exists(appDataConfigurationPath)
                    ? appDataConfigurationPath
                    : null;

            if (ConfigurationPath == null)
            {
                // No configuration, we load the default values and save it to disk
                ConfigurationPath = appDataConfigurationPath;

                ConfigurationState.Instance.LoadDefault();
                ConfigurationState.Instance.ToFileFormat().SaveConfig(ConfigurationPath);
            }
            else
            {
                if (ConfigurationFileFormat.TryLoad(ConfigurationPath, out ConfigurationFileFormat configurationFileFormat))
                {
                    ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
                }
                else
                {
                    ConfigurationState.Instance.LoadDefault();
                    Logger.Warning?.PrintMsg(LogClass.Application, $"Failed to load config! Loading the default config instead.\nFailed config location {ConfigurationPath}");
                }
            }

            // Logging system information.
            PrintSystemInfo();

            // Enable OGL multithreading on the driver, when available.
            BackendThreading threadingMode = ConfigurationState.Instance.Graphics.BackendThreading;
            DriverUtilities.ToggleOGLThreading(threadingMode == BackendThreading.Off);

            // Initialize Gtk.
            Application.Init();

            // Check if keys exists.
            bool hasSystemProdKeys = File.Exists(Path.Combine(AppDataManager.KeysDirPath, "prod.keys"));
            bool hasCommonProdKeys = AppDataManager.Mode == AppDataManager.LaunchMode.UserProfile && File.Exists(Path.Combine(AppDataManager.KeysDirPathUser, "prod.keys"));
            if (!hasSystemProdKeys && !hasCommonProdKeys)
            {
                UserErrorDialog.CreateUserErrorDialog(UserError.NoKeys);
            }

            // Show the main window UI.
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            if (launchPathArg != null)
            {
                mainWindow.LoadApplication(launchPathArg, startFullscreenArg);
            }

            if (ConfigurationState.Instance.CheckUpdatesOnStart.Value && Updater.CanUpdate(false))
            {
                Updater.BeginParse(mainWindow, false).ContinueWith(task =>
                {
                    Logger.Error?.Print(LogClass.Application, $"Updater Error: {task.Exception}");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }

            Application.Run();
        }

        private static void PrintSystemInfo()
        {
            Logger.Notice.Print(LogClass.Application, $"Ryujinx Version: {Version}");
            SystemInfo.Gather().Print();

            var enabledLogs = Logger.GetEnabledLevels();
            Logger.Notice.Print(LogClass.Application, $"Logs Enabled: {(enabledLogs.Count == 0 ? "<None>" : string.Join(", ", enabledLogs))}");

            if (AppDataManager.Mode == AppDataManager.LaunchMode.Custom)
            {
                Logger.Notice.Print(LogClass.Application, $"Launch Mode: Custom Path {AppDataManager.BaseDirPath}");
            }
            else
            {
                Logger.Notice.Print(LogClass.Application, $"Launch Mode: {AppDataManager.Mode}");
            }
        }

        private static void ProcessUnhandledException(Exception ex, bool isTerminating)
        {
            Ptc.Close();
            PtcProfiler.Stop();

            string message = $"Unhandled exception caught: {ex}";

            Logger.Error?.PrintMsg(LogClass.Application, message);

            if (Logger.Error == null)
            {
                Logger.Notice.PrintMsg(LogClass.Application, message);
            }

            if (isTerminating)
            {
                Exit();
            }
        }

        public static void Exit()
        {
            DiscordIntegrationModule.Exit();

            Ptc.Dispose();
            PtcProfiler.Dispose();

            Logger.Shutdown();
        }
    }
}