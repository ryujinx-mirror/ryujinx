using Gtk;
using Ryujinx.HLE.FileSystem;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class GameTableContextMenu : Menu
    {
        private static ListStore _gameTableStore;
        private static TreeIter  _rowIter;

#pragma warning disable CS0649
#pragma warning disable IDE0044
        [GUI] MenuItem _openSaveDir;
#pragma warning restore CS0649
#pragma warning restore IDE0044

        public GameTableContextMenu(ListStore gameTableStore, TreeIter rowIter) : this(new Builder("Ryujinx.Ui.GameTableContextMenu.glade"), gameTableStore, rowIter) { }

        private GameTableContextMenu(Builder builder, ListStore gameTableStore, TreeIter rowIter) : base(builder.GetObject("_contextMenu").Handle)
        {
            builder.Autoconnect(this);

            _openSaveDir.Activated += OpenSaveDir_Clicked;

            _gameTableStore = gameTableStore;
            _rowIter        = rowIter;
        }

        //Events
        private void OpenSaveDir_Clicked(object sender, EventArgs args)
        {
            string titleName = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[0];
            string titleId   = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();
            string saveDir   = System.IO.Path.Combine(new VirtualFileSystem().GetNandPath(), "user", "save", "0000000000000000", "00000000000000000000000000000001", titleId, "0");

            if (!Directory.Exists(saveDir))
            {
                MessageDialog messageDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, null)
                {
                    Title          = "Ryujinx",
                    Icon           = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                    Text           = $"Could not find save directory for {titleName} [{titleId}]",
                    SecondaryText  = "Would you like to create the directory?",
                    WindowPosition = WindowPosition.Center
                };

                if (messageDialog.Run() == (int)ResponseType.Yes)
                {
                    Directory.CreateDirectory(saveDir);
                }
                else
                {
                    messageDialog.Dispose();

                    return;
                }

                messageDialog.Dispose();
            }

            Process.Start(new ProcessStartInfo()
            {
                FileName        = saveDir,
                UseShellExecute = true,
                Verb            = "open"
            });
        }
    }
}
