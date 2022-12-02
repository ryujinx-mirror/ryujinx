using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.HLE.FileSystem;
using Ryujinx.Input;
using Ryujinx.Input.Assigner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TimeZone = Ryujinx.Ava.Ui.Models.TimeZone;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class SettingsWindow : StyleableWindow
    {
        private ButtonKeyAssigner _currentAssigner;

        internal SettingsViewModel ViewModel { get; set; }

        public SettingsWindow(VirtualFileSystem virtualFileSystem, ContentManager contentManager)
        {
            Title = $"Ryujinx {Program.Version} - {LocaleManager.Instance["Settings"]}";

            ViewModel   = new SettingsViewModel(virtualFileSystem, contentManager, this);
            DataContext = ViewModel;

            InitializeComponent();
            Load();

            FuncMultiValueConverter<string, string> converter = new(parts => string.Format("{0}  {1}   {2}", parts.ToArray()).Trim());
            MultiBinding tzMultiBinding = new() { Converter = converter };
            tzMultiBinding.Bindings.Add(new Binding("UtcDifference"));
            tzMultiBinding.Bindings.Add(new Binding("Location"));
            tzMultiBinding.Bindings.Add(new Binding("Abbreviation"));

            TimeZoneBox.ValueMemberBinding = tzMultiBinding;
        }

        public SettingsWindow()
        {
            ViewModel   = new SettingsViewModel();
            DataContext = ViewModel;

            InitializeComponent();
            Load();
        }

        private void Load()
        {
            Pages.Children.Clear();
            NavPanel.SelectionChanged += NavPanelOnSelectionChanged;
            NavPanel.SelectedItem = NavPanel.MenuItems.ElementAt(0);
        }

        private void Button_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                if (_currentAssigner != null && button == _currentAssigner.ToggledButton)
                {
                    return;
                }

                if (_currentAssigner == null && (bool)button.IsChecked)
                {
                    _currentAssigner = new ButtonKeyAssigner(button);

                    FocusManager.Instance.Focus(this, NavigationMethod.Pointer);

                    PointerPressed += MouseClick;

                    IKeyboard       keyboard = (IKeyboard)ViewModel.AvaloniaKeyboardDriver.GetGamepad(ViewModel.AvaloniaKeyboardDriver.GamepadsIds[0]);
                    IButtonAssigner assigner = new KeyboardKeyAssigner(keyboard);

                    _currentAssigner.GetInputAndAssign(assigner);
                }
                else
                {
                    if (_currentAssigner != null)
                    {
                        ToggleButton oldButton = _currentAssigner.ToggledButton;

                        _currentAssigner.Cancel();
                        _currentAssigner = null;

                        button.IsChecked = false;
                    }
                }
            }
        }

        private void Button_Unchecked(object sender, RoutedEventArgs e)
        {
            _currentAssigner?.Cancel();
            _currentAssigner = null;
        }

        private void MouseClick(object sender, PointerPressedEventArgs e)
        {
            bool shouldUnbind = false;

            if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
            {
                shouldUnbind = true;
            }

            _currentAssigner?.Cancel(shouldUnbind);

            PointerPressed -= MouseClick;
        }

        private void NavPanelOnSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is NavigationViewItem navitem)
            {
                NavPanel.Content = navitem.Tag.ToString() switch
                {
                    "UiPage"       => UiPage,
                    "InputPage"    => InputPage,
                    "HotkeysPage"  => HotkeysPage,
                    "SystemPage"   => SystemPage,
                    "CpuPage"      => CpuPage,
                    "GraphicsPage" => GraphicsPage,
                    "AudioPage"    => AudioPage,
                    "NetworkPage"  => NetworkPage,
                    "LoggingPage"  => LoggingPage,
                    _              => throw new NotImplementedException()
                };
            }
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
                path = await new OpenFolderDialog().ShowAsync(this);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    ViewModel.GameDirectories.Add(path);
                    ViewModel.DirectoryChanged = true;
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

        private void TimeZoneBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is TimeZone timeZone)
                {
                    e.Handled = true;

                    ViewModel.ValidateAndSetTimeZone(timeZone.Location);
                }
            }
        }

        private void TimeZoneBox_OnTextChanged(object sender, EventArgs e)
        {
            if (sender is AutoCompleteBox box)
            {
                if (box.SelectedItem != null && box.SelectedItem is TimeZone timeZone)
                {
                    ViewModel.ValidateAndSetTimeZone(timeZone.Location);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ControllerSettings.Dispose();

            _currentAssigner?.Cancel();
            _currentAssigner = null;
            
            base.OnClosed(e);
        }
    }
}