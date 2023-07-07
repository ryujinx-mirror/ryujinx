using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ryujinx.Ava.UI.Views.Settings
{
    public partial class SettingsUiView : UserControl
    {
        public SettingsViewModel ViewModel;

        public SettingsUiView()
        {
            InitializeComponent();
        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            string path = PathBox.Text;

            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path) && !ViewModel.GameDirectories.Contains(path))
            {
                ViewModel.GameDirectories.Add(path);
                ViewModel.DirectoryChanged = true;
            }
            else
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    path = await new OpenFolderDialog().ShowAsync(desktop.MainWindow);

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        ViewModel.GameDirectories.Add(path);
                        ViewModel.DirectoryChanged = true;
                    }
                }
            }
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            int oldIndex = GameList.SelectedIndex;

            foreach (string path in new List<string>(GameList.SelectedItems.Cast<string>()))
            {
                ViewModel.GameDirectories.Remove(path);
                ViewModel.DirectoryChanged = true;
            }

            if (GameList.ItemCount > 0)
            {
                GameList.SelectedIndex = oldIndex < GameList.ItemCount ? oldIndex : 0;
            }
        }

        public async void BrowseTheme(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = LocaleManager.Instance[LocaleKeys.SettingsSelectThemeFileDialogTitle],
                AllowMultiple = false,
            };

            dialog.Filters.Add(new FileDialogFilter { Extensions = { "xaml" }, Name = LocaleManager.Instance[LocaleKeys.SettingsXamlThemeFile] });

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var file = await dialog.ShowAsync(desktop.MainWindow);

                if (file != null && file.Length > 0)
                {
                    ViewModel.CustomThemePath = file[0];
                }
            }
        }
    }
}
