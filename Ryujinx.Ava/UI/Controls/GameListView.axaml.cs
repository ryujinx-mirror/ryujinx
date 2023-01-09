using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LibHac.Common;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Ui.App.Common;
using System;

namespace Ryujinx.Ava.UI.Controls
{
    public partial class GameListView : UserControl
    {
        private ApplicationData _selectedApplication;
        public static readonly RoutedEvent<ApplicationOpenedEventArgs> ApplicationOpenedEvent =
            RoutedEvent.Register<GameGridView, ApplicationOpenedEventArgs>(nameof(ApplicationOpened), RoutingStrategies.Bubble);

        public event EventHandler<ApplicationOpenedEventArgs> ApplicationOpened
        {
            add { AddHandler(ApplicationOpenedEvent, value); }
            remove { RemoveHandler(ApplicationOpenedEvent, value); }
        }

        public void GameList_DoubleTapped(object sender, RoutedEventArgs args)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem is ApplicationData selected)
                {
                    RaiseEvent(new ApplicationOpenedEventArgs(selected, ApplicationOpenedEvent));
                }
            }
        }

        public void GameList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (sender is ListBox listBox)
            {
                _selectedApplication = listBox.SelectedItem as ApplicationData;

                (DataContext as MainWindowViewModel).ListSelectedApplication = _selectedApplication;
            }
        }

        public ApplicationData SelectedApplication => _selectedApplication;

        public GameListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            (DataContext as MainWindowViewModel).SearchText = (sender as TextBox).Text;
        }

        private void MenuBase_OnMenuOpened(object sender, EventArgs e)
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                if (sender is ContextMenu menu)
                {
                    bool canHaveUserSave = !Utilities.IsZeros(selection.ControlHolder.ByteSpan) && selection.ControlHolder.Value.UserAccountSaveDataSize > 0;
                    bool canHaveDeviceSave = !Utilities.IsZeros(selection.ControlHolder.ByteSpan) && selection.ControlHolder.Value.DeviceSaveDataSize > 0;
                    bool canHaveBcatSave = !Utilities.IsZeros(selection.ControlHolder.ByteSpan) && selection.ControlHolder.Value.BcatDeliveryCacheStorageSize > 0;

                    ((menu.Items as AvaloniaList<object>)[2] as MenuItem).IsEnabled = canHaveUserSave;
                    ((menu.Items as AvaloniaList<object>)[3] as MenuItem).IsEnabled = canHaveDeviceSave;
                    ((menu.Items as AvaloniaList<object>)[4] as MenuItem).IsEnabled = canHaveBcatSave;
                }
            }
        }
    }
}
