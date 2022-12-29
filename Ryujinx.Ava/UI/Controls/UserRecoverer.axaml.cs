using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.UI.Models;
using Ryujinx.Ava.UI.ViewModels;

namespace Ryujinx.Ava.UI.Controls
{
    public partial class UserRecoverer : UserControl
    {
        private UserProfileViewModel _viewModel;
        private NavigationDialogHost _parent;

        public UserRecoverer()
        {
            InitializeComponent();
            AddHandler(Frame.NavigatedToEvent, (s, e) =>
            {
                NavigatedTo(e);
            }, RoutingStrategies.Direct);
        }

        private void NavigatedTo(NavigationEventArgs arg)
        {
            if (Program.PreviewerDetached)
            {
                switch (arg.NavigationMode)
                {
                    case NavigationMode.New:
                        var args = ((NavigationDialogHost parent, UserProfileViewModel viewModel))arg.Parameter;

                        _viewModel = args.viewModel;
                        _parent = args.parent;
                        break;
                }

                DataContext = _viewModel;
            }
        }
    }
}
