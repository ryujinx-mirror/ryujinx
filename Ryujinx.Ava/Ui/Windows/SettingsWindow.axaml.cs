using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.HLE.FileSystem;
using Ryujinx.Input;
using Ryujinx.Input.Assigner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeZone = Ryujinx.Ava.Ui.Models.TimeZone;

namespace Ryujinx.Ava.Ui.Windows
{
    public class SettingsWindow : StyleableWindow
    {
        private ListBox _gameList;
        private TextBox _pathBox;
        private AutoCompleteBox _timeZoneBox;
        private ControllerSettingsWindow _controllerSettings;

        // Pages
        private Control _uiPage;
        private Control _inputPage;
        private Control _hotkeysPage;
        private Control _systemPage;
        private Control _cpuPage;
        private Control _graphicsPage;
        private Control _audioPage;
        private Control _networkPage;
        private Control _loggingPage;
        private NavigationView _navPanel;
        private ButtonKeyAssigner _currentAssigner;

        internal SettingsViewModel ViewModel { get; set; }

        public SettingsWindow(VirtualFileSystem virtualFileSystem, ContentManager contentManager)
        {
            Title = $"Ryujinx {Program.Version} - {LocaleManager.Instance["Settings"]}";

            ViewModel = new SettingsViewModel(virtualFileSystem, contentManager, this);
            DataContext = ViewModel;

            InitializeComponent();
            AttachDebugDevTools();

            FuncMultiValueConverter<string, string> converter = new(parts => string.Format("{0}  {1}   {2}", parts.ToArray()));
            MultiBinding tzMultiBinding = new() { Converter = converter };
            tzMultiBinding.Bindings.Add(new Binding("UtcDifference"));
            tzMultiBinding.Bindings.Add(new Binding("Location"));
            tzMultiBinding.Bindings.Add(new Binding("Abbreviation"));

            _timeZoneBox.ValueMemberBinding = tzMultiBinding;
        }

        public SettingsWindow()
        {
            ViewModel = new SettingsViewModel();
            DataContext = ViewModel;

            InitializeComponent();
            AttachDebugDevTools();
        }

        [Conditional("DEBUG")]
        private void AttachDebugDevTools()
        {
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _pathBox = this.FindControl<TextBox>("PathBox");
            _gameList = this.FindControl<ListBox>("GameList");
            _timeZoneBox = this.FindControl<AutoCompleteBox>("TimeZoneBox");
            _controllerSettings = this.FindControl<ControllerSettingsWindow>("ControllerSettings");

            _uiPage = this.FindControl<Control>("UiPage");
            _inputPage = this.FindControl<Control>("InputPage");
            _hotkeysPage = this.FindControl<Control>("HotkeysPage");
            _systemPage = this.FindControl<Control>("SystemPage");
            _cpuPage = this.FindControl<Control>("CpuPage");
            _graphicsPage = this.FindControl<Control>("GraphicsPage");
            _audioPage = this.FindControl<Control>("AudioPage");
            _networkPage = this.FindControl<Control>("NetworkPage");
            _loggingPage = this.FindControl<Control>("LoggingPage");

            var pageGrid = this.FindControl<Grid>("Pages");
            pageGrid.Children.Clear();

            _navPanel = this.FindControl<NavigationView>("NavPanel");
            _navPanel.SelectionChanged += NavPanelOnSelectionChanged;
            _navPanel.SelectedItem = _navPanel.MenuItems.ElementAt(0);
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

                    IKeyboard keyboard = (IKeyboard)ViewModel.AvaloniaKeyboardDriver.GetGamepad(ViewModel.AvaloniaKeyboardDriver.GamepadsIds[0]);
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
                switch (navitem.Tag.ToString())
                {
                    case "UiPage":
                        _navPanel.Content = _uiPage;
                        break;
                    case "InputPage":
                        _navPanel.Content = _inputPage;
                        break;
                    case "HotkeysPage":
                        _navPanel.Content = _hotkeysPage;
                        break;
                    case "SystemPage":
                        _navPanel.Content = _systemPage;
                        break;
                    case "CpuPage":
                        _navPanel.Content = _cpuPage;
                        break;
                    case "GraphicsPage":
                        _navPanel.Content = _graphicsPage;
                        break;
                    case "AudioPage":
                        _navPanel.Content = _audioPage;
                        break;
                    case "NetworkPage":
                        _navPanel.Content = _networkPage;
                        break;
                    case "LoggingPage":
                        _navPanel.Content = _loggingPage;
                        break;
                }
            }
        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            string path = _pathBox.Text;

            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path) && !ViewModel.GameDirectories.Contains(path))
            {
                ViewModel.GameDirectories.Add(path);
            }
            else
            {
                path = await new OpenFolderDialog().ShowAsync(this);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    ViewModel.GameDirectories.Add(path);
                }
            }
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            List<string> selected = new(_gameList.SelectedItems.Cast<string>());

            foreach (string path in selected)
            {
                ViewModel.GameDirectories.Remove(path);
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

        private void SaveButton_Clicked(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            Close();
        }

        private void CloseButton_Clicked(object sender, RoutedEventArgs e)
        {
            ViewModel.RevertIfNotSaved();
            Close();
        }

        private void ApplyButton_Clicked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            ViewModel.SaveSettings();

            _controllerSettings?.SaveCurrentProfile();

            if (Owner is MainWindow window)
            {
                window.ViewModel.LoadApplications();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _controllerSettings.Dispose();
            _currentAssigner?.Cancel();
            _currentAssigner = null;
            base.OnClosed(e);
        }
    }
}