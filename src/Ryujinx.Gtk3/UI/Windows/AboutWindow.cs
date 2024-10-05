using Gtk;
using Ryujinx.Common.Utilities;
using Ryujinx.UI.Common.Helper;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;

namespace Ryujinx.UI.Windows
{
    public partial class AboutWindow : Window
    {
        public AboutWindow() : base($"Ryujinx {Program.Version} - About")
        {
            Icon = new Gdk.Pixbuf(Assembly.GetAssembly(typeof(OpenHelper)), "Ryujinx.UI.Common.Resources.Logo_Ryujinx.png");
            InitializeComponent();
        }

        //
        // Events
        //
        private void RyujinxButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://example.com/");
        }

        private void AmiiboApiButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://amiiboapi.com");
        }

        private void PatreonButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://example.com/");
        }

        private void GitHubButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/ryujinx-mirror/Ryujinx");
        }

        private void DiscordButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://discord.gg/xmHPGDfVCa");
        }

        private void TwitterButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://example.com/");
        }

        private void ContributorsButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/ryujinx-mirror/Ryujinx/graphs/contributors?type=a");
        }

        private void ChangelogButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/ryujinx-mirror/Ryujinx/wiki/Changelog#ryujinx-changelog");
        }
    }
}
