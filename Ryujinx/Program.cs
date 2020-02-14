using Gtk;
using Ryujinx.Common.Logging;
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

            string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");

            // Now load the configuration as the other subsystems are now registered
            if (File.Exists(configurationPath))
            {
                ConfigurationFileFormat configurationFileFormat = ConfigurationFileFormat.Load(configurationPath);
                ConfigurationState.Instance.Load(configurationFileFormat);
            }
            else
            {
                // No configuration, we load the default values and save it on disk
                ConfigurationState.Instance.LoadDefault();
                ConfigurationState.Instance.ToFileFormat().SaveConfig(configurationPath);
            }

            Profile.Initialize();

            Application.Init();

            string appDataPath     = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx", "system", "prod.keys");
            string userProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch", "prod.keys");
            if (!File.Exists(appDataPath) && !File.Exists(userProfilePath) && !Migration.IsMigrationNeeded())
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