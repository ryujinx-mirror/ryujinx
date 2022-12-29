using Avalonia.Controls;
using Ryujinx.Ava.UI.Windows;

namespace Ryujinx.Ava.UI.Controls
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