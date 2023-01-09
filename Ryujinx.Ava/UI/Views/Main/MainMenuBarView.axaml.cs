using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Ava.UI.Windows;
using System.Threading.Tasks;
using LibHac.FsSystem;
using LibHac.Ncm;
using Ryujinx.HLE.HOS;
using Ryujinx.Modules;

namespace Ryujinx.Ava.UI.Views.Main
{
    public partial class MainMenuBarView : UserControl
    {
        public MainWindow Window { get; private set; }
        public MainWindowViewModel ViewModel { get; private set; }

        public MainMenuBarView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (this.VisualRoot is MainWindow window)
            {
                Window = window;
            }

            ViewModel = Window.ViewModel;
            DataContext = ViewModel;
        }

        private async void StopEmulation_Click(object sender, RoutedEventArgs e)
        {
            await Window.ViewModel.AppHost?.ShowExitPrompt();
        }

        private async void PauseEmulation_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Window.ViewModel.AppHost?.Pause();
            });
        }

        private async void ResumeEmulation_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Window.ViewModel.AppHost?.Resume();
            });
        }

        public async void OpenSettings(object sender, RoutedEventArgs e)
        {
            Window.SettingsWindow = new(Window.VirtualFileSystem, Window.ContentManager);

            await Window.SettingsWindow.ShowDialog(Window);

            ViewModel.LoadConfigurableHotKeys();
        }

        public void OpenMiiApplet(object sender, RoutedEventArgs e)
        {
            string contentPath = ViewModel.ContentManager.GetInstalledContentPath(0x0100000000001009, StorageId.BuiltInSystem, NcaContentType.Program);

            if (!string.IsNullOrEmpty(contentPath))
            {
                ViewModel.LoadApplication(contentPath, false, "Mii Applet");
            }
        }

        public async void OpenAmiiboWindow(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsAmiiboRequested)
            {
                return;
            }

            if (ViewModel.AppHost.Device.System.SearchingForAmiibo(out int deviceId))
            {
                string titleId = ViewModel.AppHost.Device.Application.TitleIdText.ToUpper();
                AmiiboWindow window = new(ViewModel.ShowAll, ViewModel.LastScannedAmiiboId, titleId);

                await window.ShowDialog(Window);

                if (window.IsScanned)
                {
                    ViewModel.ShowAll = window.ViewModel.ShowAllAmiibo;
                    ViewModel.LastScannedAmiiboId = window.ScannedAmiibo.GetId();

                    ViewModel.AppHost.Device.System.ScanAmiibo(deviceId, ViewModel.LastScannedAmiiboId, window.ViewModel.UseRandomUuid);
                }
            }
        }

        public async void OpenCheatManagerForCurrentApp(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsGameRunning)
            {
                return;
            }

            ApplicationLoader application = ViewModel.AppHost.Device.Application;
            if (application != null)
            {
                await new CheatWindow(Window.VirtualFileSystem, application.TitleIdText, application.TitleName).ShowDialog(Window);

                ViewModel.AppHost.Device.EnableCheats();
            }
        }

        private void ScanAmiiboMenuItem_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is MenuItem)
            {
                ViewModel.IsAmiiboRequested = Window.ViewModel.AppHost.Device.System.SearchingForAmiibo(out _);
            }
        }

        public async void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            if (Updater.CanUpdate(true, Window))
            {
                await Updater.BeginParse(Window, true);
            }
        }

        public async void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            await AboutWindow.Show();
        }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}