using Gtk;
using LibHac;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.Ncm;
using Ryujinx.HLE.FileSystem;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class GameTableContextMenu : Menu
    {
        private static ListStore _gameTableStore;
        private static TreeIter  _rowIter;
        private VirtualFileSystem _virtualFileSystem;

#pragma warning disable CS0649
#pragma warning disable IDE0044
        [GUI] MenuItem _openSaveDir;
#pragma warning restore CS0649
#pragma warning restore IDE0044

        public GameTableContextMenu(ListStore gameTableStore, TreeIter rowIter, VirtualFileSystem virtualFileSystem)
            : this(new Builder("Ryujinx.Ui.GameTableContextMenu.glade"), gameTableStore, rowIter, virtualFileSystem) { }

        private GameTableContextMenu(Builder builder, ListStore gameTableStore, TreeIter rowIter, VirtualFileSystem virtualFileSystem) : base(builder.GetObject("_contextMenu").Handle)
        {
            builder.Autoconnect(this);

            _openSaveDir.Activated += OpenSaveDir_Clicked;

            _gameTableStore    = gameTableStore;
            _rowIter           = rowIter;
            _virtualFileSystem = virtualFileSystem;
        }

        //Events
        private void OpenSaveDir_Clicked(object sender, EventArgs args)
        {
            string titleName = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[0];
            string titleId   = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();

            if (!TryFindSaveData(titleName, titleId, out ulong saveDataId))
            {
                return;
            }

            string saveDir = GetSaveDataDirectory(saveDataId);

            Process.Start(new ProcessStartInfo()
            {
                FileName        = saveDir,
                UseShellExecute = true,
                Verb            = "open"
            });
        }

        private bool TryFindSaveData(string titleName, string titleIdText, out ulong saveDataId)
        {
            saveDataId = default;

            if (!ulong.TryParse(titleIdText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleId))
            {
                GtkDialog.CreateErrorDialog("UI error: The selected game did not have a valid title ID");

                return false;
            }

            SaveDataFilter filter = new SaveDataFilter();
            filter.SetUserId(new UserId(1, 0));
            filter.SetTitleId(new TitleId(titleId));

            Result result = _virtualFileSystem.FsClient.FindSaveDataWithFilter(out SaveDataInfo saveDataInfo, SaveDataSpaceId.User, ref filter);

            if (result == ResultFs.TargetNotFound)
            {
                // Savedata was not found. Ask the user if they want to create it
                using MessageDialog messageDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, null)
                {
                    Title          = "Ryujinx",
                    Icon           = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                    Text           = $"There is no savedata for {titleName} [{titleId:x16}]",
                    SecondaryText  = "Would you like to create savedata for this game?",
                    WindowPosition = WindowPosition.Center
                };

                if (messageDialog.Run() != (int)ResponseType.Yes)
                {
                    return false;
                }

                result = _virtualFileSystem.FsClient.CreateSaveData(new TitleId(titleId), new UserId(1, 0), new TitleId(titleId), 0, 0, 0);

                if (result.IsFailure())
                {
                    GtkDialog.CreateErrorDialog($"There was an error creating the specified savedata: {result.ToStringWithName()}");

                    return false;
                }

                // Try to find the savedata again after creating it
                result = _virtualFileSystem.FsClient.FindSaveDataWithFilter(out saveDataInfo, SaveDataSpaceId.User, ref filter);
            }

            if (result.IsSuccess())
            {
                saveDataId = saveDataInfo.SaveDataId;

                return true;
            }

            GtkDialog.CreateErrorDialog($"There was an error finding the specified savedata: {result.ToStringWithName()}");

            return false;
        }

        private string GetSaveDataDirectory(ulong saveDataId)
        {
            string saveRootPath = System.IO.Path.Combine(_virtualFileSystem.GetNandPath(), $"user/save/{saveDataId:x16}");

            if (!Directory.Exists(saveRootPath))
            {
                // Inconsistent state. Create the directory
                Directory.CreateDirectory(saveRootPath);
            }

            string committedPath = System.IO.Path.Combine(saveRootPath, "0");
            string workingPath = System.IO.Path.Combine(saveRootPath, "1");

            // If the committed directory exists, that path will be loaded the next time the savedata is mounted
            if (Directory.Exists(committedPath))
            {
                return committedPath;
            }

            // If the working directory exists and the committed directory doesn't,
            // the working directory will be loaded the next time the savedata is mounted
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            return workingPath;
        }
    }
}
