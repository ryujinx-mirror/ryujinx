using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Ui.Windows;

namespace Ryujinx.Ava.Ui.Controls
{
    public partial class UpdateWaitWindow : StyleableWindow
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
        }
    }
}