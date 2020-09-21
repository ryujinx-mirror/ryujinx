using Gtk;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ns;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GUI        = Gtk.Builder.ObjectAttribute;
using JsonHelper = Ryujinx.Common.Utilities.JsonHelper;

namespace Ryujinx.Ui
{
    public class TitleUpdateWindow : Window
    {
        private readonly VirtualFileSystem _virtualFileSystem;
        private readonly string            _titleId;
        private readonly string            _updateJsonPath;

        private TitleUpdateMetadata             _titleUpdateWindowData;
        private Dictionary<RadioButton, string> _radioButtonToPathDictionary;

#pragma warning disable CS0649, IDE0044
        [GUI] Label       _baseTitleInfoLabel;
        [GUI] Box         _availableUpdatesBox;
        [GUI] RadioButton _noUpdateRadioButton;
#pragma warning restore CS0649, IDE0044

        public TitleUpdateWindow(string titleId, string titleName, VirtualFileSystem virtualFileSystem) : this(new Builder("Ryujinx.Ui.TitleUpdateWindow.glade"), titleId, titleName, virtualFileSystem) { }

        private TitleUpdateWindow(Builder builder, string titleId, string titleName, VirtualFileSystem virtualFileSystem) : base(builder.GetObject("_titleUpdateWindow").Handle)
        {
            builder.Autoconnect(this);

            _titleId                     = titleId;
            _virtualFileSystem           = virtualFileSystem;
            _updateJsonPath              = System.IO.Path.Combine(AppDataManager.GamesDirPath, _titleId, "updates.json");
            _radioButtonToPathDictionary = new Dictionary<RadioButton, string>();

            try
            {
                _titleUpdateWindowData = JsonHelper.DeserializeFromFile<TitleUpdateMetadata>(_updateJsonPath);
            }
            catch
            {
                _titleUpdateWindowData = new TitleUpdateMetadata
                {
                    Selected = "",
                    Paths    = new List<string>()
                };
            }

            _baseTitleInfoLabel.Text = $"Updates Available for {titleName} [{titleId.ToUpper()}]";

            foreach (string path in _titleUpdateWindowData.Paths)
            {
                AddUpdate(path, false);
            }

            _noUpdateRadioButton.Active = true;
            foreach (KeyValuePair<RadioButton, string> keyValuePair in _radioButtonToPathDictionary)
            {
                if (keyValuePair.Value == _titleUpdateWindowData.Selected)
                {
                    keyValuePair.Key.Active = true;
                }
            }
        }

        private void AddUpdate(string path, bool showErrorDialog = true)
        {
            if (File.Exists(path))
            {
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

                    try
                    {
                        (Nca patchNca, Nca controlNca) = ApplicationLoader.GetGameUpdateDataFromPartition(_virtualFileSystem, nsp, _titleId, 0);

                        if (controlNca != null && patchNca != null)
                        {
                            ApplicationControlProperty controlData = new ApplicationControlProperty();

                            controlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None).OpenFile(out IFile nacpFile, "/control.nacp".ToU8Span(), OpenMode.Read).ThrowIfFailure();
                            nacpFile.Read(out _, 0, SpanHelpers.AsByteSpan(ref controlData), ReadOption.None).ThrowIfFailure();

                            RadioButton radioButton = new RadioButton($"Version {controlData.DisplayVersion.ToString()} - {path}");
                            radioButton.JoinGroup(_noUpdateRadioButton);

                            _availableUpdatesBox.Add(radioButton);
                            _radioButtonToPathDictionary.Add(radioButton, path);

                            radioButton.Show();
                            radioButton.Active = true;
                        }
                        else
                        {
                            GtkDialog.CreateErrorDialog("The specified file does not contain an update for the selected title!");
                        }
                    }
                    catch (InvalidDataException exception)
                    {
                        Logger.Error?.Print(LogClass.Application, $"{exception.Message}. Errored File: {path}");

                        if (showErrorDialog)
                        {
                            GtkDialog.CreateInfoDialog("Ryujinx - Error", "Add Update Failed!", "The NCA header content type check has failed. This is usually because the header key is incorrect or missing.");
                        }
                    }
                    catch (MissingKeyException exception)
                    {
                        Logger.Error?.Print(LogClass.Application, $"Your key set is missing a key with the name: {exception.Name}. Errored File: {path}");

                        if (showErrorDialog)
                        {
                            GtkDialog.CreateInfoDialog("Ryujinx - Error", "Add Update Failed!", $"Your key set is missing a key with the name: {exception.Name}");
                        }
                    }
                }
            }
        }

        private void AddButton_Clicked(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Select update files", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Add", ResponseType.Accept)
            {
                SelectMultiple = true,
                Filter         = new FileFilter()
            };
            fileChooser.SetPosition(WindowPosition.Center);
            fileChooser.Filter.AddPattern("*.nsp");

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                foreach (string path in fileChooser.Filenames)
                {
                    AddUpdate(path);
                }
            }

            fileChooser.Dispose();
        }

        private void RemoveButton_Clicked(object sender, EventArgs args)
        {
            foreach (RadioButton radioButton in _noUpdateRadioButton.Group)
            {
                if (radioButton.Label != "No Update" && radioButton.Active)
                {
                    _availableUpdatesBox.Remove(radioButton);
                    _radioButtonToPathDictionary.Remove(radioButton);
                    radioButton.Dispose();
                }
            }
        }

        private void SaveButton_Clicked(object sender, EventArgs args)
        {
            _titleUpdateWindowData.Paths.Clear();
            foreach (string paths in _radioButtonToPathDictionary.Values)
            {
                _titleUpdateWindowData.Paths.Add(paths);
            }

            foreach (RadioButton radioButton in _noUpdateRadioButton.Group)
            {
                if (radioButton.Active)
                {
                    _titleUpdateWindowData.Selected = _radioButtonToPathDictionary.TryGetValue(radioButton, out string updatePath) ? updatePath : "";
                }
            }

            using (FileStream dlcJsonStream = File.Create(_updateJsonPath, 4096, FileOptions.WriteThrough))
            {
                dlcJsonStream.Write(Encoding.UTF8.GetBytes(JsonHelper.Serialize(_titleUpdateWindowData, true)));
            }

            MainWindow.UpdateGameTable();
            Dispose();
        }

        private void CancelButton_Clicked(object sender, EventArgs args)
        {
            Dispose();
        }
    }
}