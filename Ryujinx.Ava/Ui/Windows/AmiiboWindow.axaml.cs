using Avalonia;
using Avalonia.Interactivity;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.ViewModels;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class AmiiboWindow : StyleableWindow
    {
        public AmiiboWindow(bool showAll, string lastScannedAmiiboId, string titleId)
        {
            ViewModel = new AmiiboWindowViewModel(this, lastScannedAmiiboId, titleId);

            ViewModel.ShowAllAmiibo = showAll;

            DataContext = ViewModel;

            InitializeComponent();

            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["Amiibo"];
        }

        public AmiiboWindow()
        {
            ViewModel = new AmiiboWindowViewModel(this, string.Empty, string.Empty);

            DataContext = ViewModel;

            InitializeComponent();

            if (Program.PreviewerDetached)
            {
                Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["Amiibo"];
            }
        }

        public bool IsScanned { get; set; }
        public Amiibo.AmiiboApi ScannedAmiibo { get; set; }
        public AmiiboWindowViewModel ViewModel { get; set; }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.AmiiboSelectedIndex > -1)
            {
                Amiibo.AmiiboApi amiibo = ViewModel.AmiiboList[ViewModel.AmiiboSelectedIndex];
                ScannedAmiibo = amiibo;
                IsScanned = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsScanned = false;

            Close();
        }
    }
}