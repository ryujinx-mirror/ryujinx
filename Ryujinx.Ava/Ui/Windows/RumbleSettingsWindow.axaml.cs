using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.Common.Configuration.Hid.Controller;
using System.Threading.Tasks;

namespace Ryujinx.Ava.Ui.Windows
{
    public class RumbleSettingsWindow : UserControl
    {
        private readonly InputConfiguration<GamepadInputId, StickInputId> _viewmodel;

        public RumbleSettingsWindow()
        {
            InitializeComponent();
        }

        public RumbleSettingsWindow(ControllerSettingsViewModel viewmodel)
        {
            var config = viewmodel.Configuration as InputConfiguration<GamepadInputId, StickInputId>;

            _viewmodel = new InputConfiguration<GamepadInputId, StickInputId>()
            {
                StrongRumble = config.StrongRumble,
                WeakRumble = config.WeakRumble
            };

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            DataContext = _viewmodel;

            AvaloniaXamlLoader.Load(this);
        }

        public static async Task Show(ControllerSettingsViewModel viewmodel, StyleableWindow window)
        {
            ContentDialog contentDialog = window.ContentDialog;

            string name = string.Empty;

            RumbleSettingsWindow content = new RumbleSettingsWindow(viewmodel);

            if (contentDialog != null)
            {
                contentDialog.Title = LocaleManager.Instance["ControllerRumbleTitle"];
                contentDialog.PrimaryButtonText = LocaleManager.Instance["ControllerSettingsSave"];
                contentDialog.SecondaryButtonText = "";
                contentDialog.CloseButtonText = LocaleManager.Instance["ControllerSettingsClose"];
                contentDialog.Content = content;
                contentDialog.PrimaryButtonClick += (sender, args) =>
                {
                    var config = viewmodel.Configuration as InputConfiguration<GamepadInputId, StickInputId>;
                    config.StrongRumble = content._viewmodel.StrongRumble;
                    config.WeakRumble = content._viewmodel.WeakRumble;
                };

                await contentDialog.ShowAsync();
            }
        }
    }
}