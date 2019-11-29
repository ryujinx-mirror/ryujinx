using Gtk;
using Ryujinx.Common.Logging;
using Ryujinx.Profiler;
using Ryujinx.Ui;
using System;
using System.IO;

namespace Ryujinx
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Ryujinx Console";

            string systemPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("Path", $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")};{systemPath}");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit        += CurrentDomain_ProcessExit;
            GLib.ExceptionManager.UnhandledException   += Glib_UnhandledException;

            Profile.Initialize();

            Application.Init();

            string appDataPath     = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RyuFs", "system", "prod.keys");
            string userProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch", "prod.keys");
            if (!File.Exists(appDataPath) && !File.Exists(userProfilePath))
            {
                GtkDialog.CreateErrorDialog($"Key file was not found. Please refer to `KEYS.md` for more info");
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            if (args.Length == 1)
            {
                mainWindow.LoadApplication(args[0]);
            }

            Application.Run();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.Shutdown();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            Logger.PrintError(LogClass.Emulation, $"Unhandled exception caught: {exception}");

            if (e.IsTerminating)
            {
                Logger.Shutdown();
            }
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