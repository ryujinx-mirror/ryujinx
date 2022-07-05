using ARMeilleure.Translation.PTC;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering;
using Avalonia.Threading;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.GraphicsDriver;
using Ryujinx.Common.Logging;
using Ryujinx.Common.System;
using Ryujinx.Common.SystemInfo;
using Ryujinx.Modules;
using Ryujinx.Ui.Common;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Ryujinx.Ava
{
    internal class Program
    {
        public static double WindowScaleFactor { get; set; }
        public static string Version { get; private set; }
        public static string ConfigurationPath { get; private set; }
        public static string CommandLineProfile { get; set; }
        public static bool PreviewerDetached { get; private set; }

        public static RenderTimer RenderTimer { get; private set; }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int MessageBoxA(IntPtr hWnd, string text, string caption, uint type);

        private const uint MB_ICONWARNING = 0x30;

        public static void Main(string[] args)
        {
            Version = ReleaseInformations.GetVersion();

            if (OperatingSystem.IsWindows() && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
            {
                MessageBoxA(IntPtr.Zero, "You are running an outdated version of Windows.\n\nStarting on June 1st 2022, Ryujinx will only support Windows 10 1803 and newer.\n", $"Ryujinx {Version}", MB_ICONWARNING);
            }

            PreviewerDetached = true;

            Initialize(args);

            RenderTimer = new RenderTimer();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

            RenderTimer.Dispose();
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions
                {
                    EnableMultiTouch = true,
                    EnableIme = true,
                    UseEGL = false,
                    UseGpu = true,
                    GlProfiles = new List<GlVersion>()
                    {
                        new GlVersion(GlProfileType.OpenGL, 4, 3)
                    }
                })
                .With(new Win32PlatformOptions
                {
                    EnableMultitouch = true,
                    UseWgl = true,
                    WglProfiles = new List<GlVersion>()
                    {
                        new GlVersion(GlProfileType.OpenGL, 4, 3)
                    },
                    AllowEglInitialization = false,
                    CompositionBackdropCornerRadius = 8f,
                })
                .UseSkia()
                .AfterSetup(_ =>
                {
                    AvaloniaLocator.CurrentMutable
                        .Bind<IRenderTimer>().ToConstant(RenderTimer)
                        .Bind<IRenderLoop>().ToConstant(new RenderLoop(RenderTimer, Dispatcher.UIThread));
                })
                .LogToTrace();
        }

        private static void Initialize(string[] args)
        {
            // Parse Arguments.
            string launchPathArg = null;
            string baseDirPathArg = null;
            bool startFullscreenArg = false;

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
                else
                {
                    launchPathArg = arg;
                }
            }

            // Make process DPI aware for proper window sizing on high-res screens.
            WindowScaleFactor = ForceDpiAware.GetWindowScaleFactor();

            // Delete backup files after updating.
            Task.Run(Updater.CleanupUpdate);

            Console.Title = $"Ryujinx Console {Version}";

            // Hook unhandled exception and process exit events.
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => ProcessUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => Exit();

            // Setup base data directory.
            AppDataManager.Initialize(baseDirPathArg);

            // Initialize the configuration.
            ConfigurationState.Initialize();

            // Initialize the logger system.
            LoggerModule.Initialize();

            // Initialize Discord integration.
            DiscordIntegrationModule.Initialize();

            ReloadConfig();

            // Logging system information.
            PrintSystemInfo();

            // Enable OGL multithreading on the driver, when available.
            BackendThreading threadingMode = ConfigurationState.Instance.Graphics.BackendThreading;
            DriverUtilities.ToggleOGLThreading(threadingMode == BackendThreading.Off);

            // Check if keys exists.
            bool hasSystemProdKeys = File.Exists(Path.Combine(AppDataManager.KeysDirPath, "prod.keys"));
            if (!hasSystemProdKeys)
            {
                if (!(AppDataManager.Mode == AppDataManager.LaunchMode.UserProfile && File.Exists(Path.Combine(AppDataManager.KeysDirPathUser, "prod.keys"))))
                {
                    MainWindow.ShowKeyErrorOnLoad = true;
                }
            }

            if (launchPathArg != null)
            {
                MainWindow.DeferLoadApplication(launchPathArg, startFullscreenArg);
            }
        }

        public static void ReloadConfig()
        {
            string localConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
            string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, "Config.json");

            // Now load the configuration as the other subsystems are now registered
            if (File.Exists(localConfigurationPath))
            {
                ConfigurationPath = localConfigurationPath;
            }
            else if (File.Exists(appDataConfigurationPath))
            {
                ConfigurationPath = appDataConfigurationPath;
            }

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