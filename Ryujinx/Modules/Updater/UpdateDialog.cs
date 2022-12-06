using Gdk;
using Gtk;
using Ryujinx.Ui;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Ui.Common.Helper;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Ryujinx.Modules
{
    public class UpdateDialog : Gtk.Window
    {
#pragma warning disable CS0649, IDE0044
        [Builder.Object] public Label    MainText;
        [Builder.Object] public Label    SecondaryText;
        [Builder.Object] public LevelBar ProgressBar;
        [Builder.Object] public Button   YesButton;
        [Builder.Object] public Button   NoButton;
#pragma warning restore CS0649, IDE0044

        private readonly MainWindow _mainWindow;
        private readonly string     _buildUrl;
        private          bool       _restartQuery;

        public UpdateDialog(MainWindow mainWindow, Version newVersion, string buildUrl) : this(new Builder("Ryujinx.Modules.Updater.UpdateDialog.glade"), mainWindow, newVersion, buildUrl) { }

        private UpdateDialog(Builder builder, MainWindow mainWindow, Version newVersion, string buildUrl) : base(builder.GetRawOwnedObject("UpdateDialog"))
        {
            builder.Autoconnect(this);

            _mainWindow = mainWindow;
            _buildUrl   = buildUrl;

            Icon = new Gdk.Pixbuf(Assembly.GetAssembly(typeof(ConfigurationState)), "Ryujinx.Ui.Common.Resources.Logo_Ryujinx.png");
            MainText.Text      = "Do you want to update Ryujinx to the latest version?";
            SecondaryText.Text = $"{Program.Version} -> {newVersion}";

            ProgressBar.Hide();

            YesButton.Clicked += YesButton_Clicked;
            NoButton.Clicked  += NoButton_Clicked;
        }

        private void YesButton_Clicked(object sender, EventArgs args)
        {
            if (_restartQuery)
            {
                string ryuName = OperatingSystem.IsWindows() ? "Ryujinx.exe" : "Ryujinx";
                string ryuExe  = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ryuName);

                Process.Start(ryuExe, CommandLineState.Arguments);

                Environment.Exit(0);
            }
            else
            {
                Window.Functions = _mainWindow.Window.Functions = WMFunction.All & WMFunction.Close;
                _mainWindow.ExitMenuItem.Sensitive = false;

                YesButton.Hide();
                NoButton.Hide();
                ProgressBar.Show();

                SecondaryText.Text = "";
                _restartQuery      = true;

                Updater.UpdateRyujinx(this, _buildUrl);
            }
        }

        private void NoButton_Clicked(object sender, EventArgs args)
        {
            Updater.Running = false;
            _mainWindow.Window.Functions = WMFunction.All;

            _mainWindow.ExitMenuItem.Sensitive   = true;
            _mainWindow.UpdateMenuItem.Sensitive = true;

            Dispose();
        }
    }
}