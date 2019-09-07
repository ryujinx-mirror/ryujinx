using Gtk;
using GUI = Gtk.Builder.ObjectAttribute;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Utf8Json;
using Utf8Json.Resolvers;
using System.IO;

namespace Ryujinx.UI
{
    public struct Info
    {
        public string InstallVersion;
        public string InstallCommit;
        public string InstallBranch;
    }

    public class AboutWindow : Window
    {
        public static Info Information { get; private set; }

#pragma warning disable 649
        [GUI] Window _aboutWin;
        [GUI] Label  _versionText;
        [GUI] Image  _ryujinxLogo;
        [GUI] Image  _patreonLogo;
        [GUI] Image  _gitHubLogo;
        [GUI] Image  _discordLogo;
        [GUI] Image  _twitterLogo;
#pragma warning restore 649

        public AboutWindow() : this(new Builder("Ryujinx.Ui.AboutWindow.glade")) { }

        private AboutWindow(Builder builder) : base(builder.GetObject("_aboutWin").Handle)
        {
            builder.Autoconnect(this);

            _aboutWin.Icon      = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.ryujinxIcon.png");
            _ryujinxLogo.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.ryujinxIcon.png", 100, 100);
            _patreonLogo.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.PatreonLogo.png", 30 , 30 );
            _gitHubLogo.Pixbuf  = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.GitHubLogo.png" , 30 , 30 );
            _discordLogo.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.DiscordLogo.png", 30 , 30 );
            _twitterLogo.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.TwitterLogo.png", 30 , 30 );

            try
            {
                IJsonFormatterResolver resolver = CompositeResolver.Create(new[] { StandardResolver.AllowPrivateSnakeCase });

                using (Stream stream = File.OpenRead(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RyuFS", "Installer", "Config", "Config.json")))
                {
                    Information = JsonSerializer.Deserialize<Info>(stream, resolver);
                }

                _versionText.Text = $"Version {Information.InstallVersion} - {Information.InstallBranch} ({Information.InstallCommit})";
            }
            catch
            {
                _versionText.Text = "Unknown Version";
            }
        }

        public void OpenUrl(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        //Events
        private void RyujinxButton_Pressed(object obj, ButtonPressEventArgs args)
        {
            OpenUrl("https://ryujinx.org");
        }

        private void PatreonButton_Pressed(object obj, ButtonPressEventArgs args)
        {
            OpenUrl("https://www.patreon.com/ryujinx");
        }

        private void GitHubButton_Pressed(object obj, ButtonPressEventArgs args)
        {
            OpenUrl("https://github.com/Ryujinx/Ryujinx");
        }

        private void DiscordButton_Pressed(object obj, ButtonPressEventArgs args)
        {
            OpenUrl("https://discordapp.com/invite/N2FmfVc");
        }

        private void TwitterButton_Pressed(object obj, ButtonPressEventArgs args)
        {
            OpenUrl("https://twitter.com/RyujinxEmu");
        }

        private void ContributersButton_Pressed(object obj, ButtonPressEventArgs args)
        {
            OpenUrl("https://github.com/Ryujinx/Ryujinx/graphs/contributors?type=a");
        }

        private void CloseToggle_Activated(object obj, EventArgs args)
        {
            Destroy();
        }
    }
}
