using Gtk;
using JsonPrettyPrinterPlus;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ns;
using LibHac.Spl;
using Ryujinx.Common.Configuration;
using Ryujinx.HLE.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class TitleUpdateWindow : Window
    {
        private readonly string            _titleId;
        private readonly VirtualFileSystem _virtualFileSystem;

        private TitleUpdateMetadata _titleUpdateWindowData;
        private Dictionary<RadioButton, string> _radioButtonToPathDictionary = new Dictionary<RadioButton, string>();

#pragma warning disable CS0649
#pragma warning disable IDE0044
        [GUI] Label       _baseTitleInfoLabel;
        [GUI] Box         _availableUpdatesBox;
        [GUI] RadioButton _noUpdateRadioButton;
#pragma warning restore CS0649
#pragma warning restore IDE0044

        public TitleUpdateWindow(string titleId, string titleName, VirtualFileSystem virtualFileSystem) : this(new Builder("Ryujinx.Ui.TitleUpdateWindow.glade"), titleId, titleName, virtualFileSystem) { }

        private TitleUpdateWindow(Builder builder, string titleId, string titleName, VirtualFileSystem virtualFileSystem) : base(builder.GetObject("_titleUpdateWindow").Handle)
        {
            builder.Autoconnect(this);

            _titleId           = titleId;
            _virtualFileSystem = virtualFileSystem;

            try
            {
                using (Stream stream = File.OpenRead(System.IO.Path.Combine(_virtualFileSystem.GetBasePath(), "games", _titleId, "updates.json")))
                {
                    IJsonFormatterResolver resolver = CompositeResolver.Create(StandardResolver.AllowPrivateSnakeCase);

                    _titleUpdateWindowData = JsonSerializer.Deserialize<TitleUpdateMetadata>(stream, resolver);
                }
            }
            catch
            {
                _titleUpdateWindowData = new TitleUpdateMetadata
                {
                    Selected = "",
                    Paths    = new List<string>()
                };
            }

            _baseTitleInfoLabel.Text = $"Updates Available for {titleName} [{titleId}]";

            foreach (string path in _titleUpdateWindowData.Paths)
            {
                AddUpdate(path);
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

        private void AddUpdate(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

                    foreach (DirectoryEntryEx ticketEntry in nsp.EnumerateEntries("/", "*.tik"))
                    {
                        Result result = nsp.OpenFile(out IFile ticketFile, ticketEntry.FullPath.ToU8Span(), OpenMode.Read);

                        if (result.IsSuccess())
                        {
                            Ticket ticket = new Ticket(ticketFile.AsStream());

                            _virtualFileSystem.KeySet.ExternalKeySet.Add(new RightsId(ticket.RightsId), new AccessKey(ticket.GetTitleKey(_virtualFileSystem.KeySet)));
                        }
                    }

                    foreach (DirectoryEntryEx fileEntry in nsp.EnumerateEntries("/", "*.nca"))
                    {
                        nsp.OpenFile(out IFile ncaFile, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                        Nca nca = new Nca(_virtualFileSystem.KeySet, ncaFile.AsStorage());

                        if ($"{nca.Header.TitleId.ToString("x16")[..^3]}000" == _titleId)
                        {
                            if (nca.Header.ContentType == NcaContentType.Control)
                            {
                                ApplicationControlProperty controlData = new ApplicationControlProperty();

                                nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None).OpenFile(out IFile nacpFile, "/control.nacp".ToU8Span(), OpenMode.Read).ThrowIfFailure();
                                nacpFile.Read(out long _, 0, SpanHelpers.AsByteSpan(ref controlData), ReadOption.None).ThrowIfFailure();

                                RadioButton radioButton = new RadioButton($"Version {controlData.DisplayVersion.ToString()} - {path}");
                                radioButton.JoinGroup(_noUpdateRadioButton);

                                _availableUpdatesBox.Add(radioButton);
                                _radioButtonToPathDictionary.Add(radioButton, path);

                                radioButton.Show();
                                radioButton.Active = true;
                            }
                        }
                        else
                        {
                            GtkDialog.CreateErrorDialog("The specified file does not contain an update for the selected title!");
                            break;
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

            IJsonFormatterResolver resolver = CompositeResolver.Create(StandardResolver.AllowPrivateSnakeCase);

            string path = System.IO.Path.Combine(_virtualFileSystem.GetBasePath(), "games", _titleId, "updates.json");
            byte[] data = JsonSerializer.Serialize(_titleUpdateWindowData, resolver);

            File.WriteAllText(path, Encoding.UTF8.GetString(data, 0, data.Length).PrettyPrintJson());

            MainWindow.UpdateGameTable();
            Dispose();
        }

        private void CancelButton_Clicked(object sender, EventArgs args)
        {
            Dispose();
        }
    }
}