using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Windows;
using System.Threading.Tasks;

namespace Ryujinx.Ava.Ui.Applet
{
    internal partial class ErrorAppletWindow : StyleableWindow
    {
        private readonly Window _owner;
        private object _buttonResponse;

        public ErrorAppletWindow(Window owner, string[] buttons, string message)
        {
            _owner = owner;
            Message = message;
            DataContext = this;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            int responseId = 0;

            if (buttons != null)
            {
                foreach (string buttonText in buttons)
                {
                    AddButton(buttonText, responseId);
                    responseId++;
                }
            }
            else
            {
                AddButton(LocaleManager.Instance["InputDialogOk"], 0);
            }
        }

        public ErrorAppletWindow()
        {
            DataContext = this;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public string Message { get; set; }

        private void AddButton(string label, object tag)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Button button = new() { Content = label, Tag = tag };

                button.Click += Button_Click;
                ButtonStack.Children.Add(button);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _buttonResponse = button.Tag;
            }

            Close();
        }

        public async Task<object> Run()
        {
            await ShowDialog(_owner);

            return _buttonResponse;
        }
    }
}