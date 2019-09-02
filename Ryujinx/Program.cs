using Gtk;
using Ryujinx.Common.Logging;
using Ryujinx.Profiler;
using Ryujinx.UI;
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

            Profile.Initialize();

            Application.Init();

            Application gtkApplication = new Application("Ryujinx.Ryujinx", GLib.ApplicationFlags.None);
            MainWindow  mainWindow     = new MainWindow(args, gtkApplication);

            gtkApplication.Register(GLib.Cancellable.Current);
            gtkApplication.AddWindow(mainWindow);
            mainWindow.Show();

            Application.Run();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.Shutdown();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            Logger.PrintError(LogClass.Emulation, $"Unhandled exception caught: {exception}");

            if (e.IsTerminating)
            {
                Logger.Shutdown();
            }
        }
    }
}