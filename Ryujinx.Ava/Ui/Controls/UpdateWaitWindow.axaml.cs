using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Ui.Windows;

namespace Ryujinx.Ava.Ui.Controls
{
    public class UpdateWaitWindow : StyleableWindow
    {
        public UpdateWaitWindow(string primaryText, string secondaryText) : this()
        {
            PrimaryText.Text = primaryText;
            SecondaryText.Text = secondaryText;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public UpdateWaitWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public TextBlock PrimaryText { get; private set; }
        public TextBlock SecondaryText { get; private set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            PrimaryText = this.FindControl<TextBlock>("PrimaryText");
            SecondaryText = this.FindControl<TextBlock>("SecondaryText");
        }
    }
}