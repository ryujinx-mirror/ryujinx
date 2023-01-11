using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ns;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Controls;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.Models;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Path = System.IO.Path;
using SpanHelpers = LibHac.Common.SpanHelpers;

namespace Ryujinx.Ava.UI.Windows
{
    public partial class TitleUpdateWindow : StyleableWindow
    {
        private readonly string     _titleUpdateJsonPath;
        private TitleUpdateMetadata _titleUpdateWindowData;

        private VirtualFileSystem              _virtualFileSystem { get; }
        private AvaloniaList<TitleUpdateModel> _titleUpdates      { get; set; }

        private ulong  _titleId   { get; }
        private string _titleName { get; }

        public TitleUpdateWindow()
        {
            DataContext = this;

            InitializeComponent();
            
            Title = $"Ryujinx {Program.Version} - {LocaleManager.Instance[LocaleKeys.UpdateWindowTitle]} - {_titleName} ({_titleId:X16})";
        }

        public TitleUpdateWindow(VirtualFileSystem virtualFileSystem, ulong titleId, string titleName)
        {
            _virtualFileSystem = virtualFileSystem;
            _titleUpdates      = new AvaloniaList<TitleUpdateModel>();

            _titleId   = titleId;
            _titleName = titleName;

            _titleUpdateJsonPath = Path.Combine(AppDataManager.GamesDirPath, titleId.ToString("x16"), "updates.json");

            try
            {
                _titleUpdateWindowData = JsonHelper.DeserializeFromFile<TitleUpdateMetadata>(_titleUpdateJsonPath);
            }
            catch
            {
                _titleUpdateWindowData = new TitleUpdateMetadata 
                {
                    Selected = "",
                    Paths    = new List<string>()
                };
            }

            DataContext = this;

            InitializeComponent();

            Title = $"Ryujinx {Program.Version} - {LocaleManager.Instance[LocaleKeys.UpdateWindowTitle]} - {_titleName} ({_titleId:X16})";

            LoadUpdates();
            PrintHeading();
        }

        private void PrintHeading()
        {
            Heading.Text = string.Format(LocaleManager.Instance[LocaleKeys.GameUpdateWindowHeading], _titleUpdates.Count - 1, _titleName, _titleId.ToString("X16"));
        }

        private void LoadUpdates()
        {
            _titleUpdates.Add(new TitleUpdateModel(default, string.Empty, true));

            foreach (string path in _titleUpdateWindowData.Paths)
            {
                AddUpdate(path);
            }

            if (_titleUpdateWindowData.Selected == "")
            {
                _titleUpdates[0].IsEnabled = true;
            }
            else
            {
                TitleUpdateModel       selected = _titleUpdates.FirstOrDefault(x => x.Path == _titleUpdateWindowData.Selected);
                List<TitleUpdateModel> enabled  = _titleUpdates.Where(x => x.IsEnabled).ToList();

                foreach (TitleUpdateModel update in enabled)
                {
                    update.IsEnabled = false;
                }

                if (selected != null)
                {
                    selected.IsEnabled = true;
                }
            }

            SortUpdates();
        }

        private void AddUpdate(string path)
        {
            if (File.Exists(path) && !_titleUpdates.Any(x => x.Path == path))
            {
                using FileStream file = new(path, FileMode.Open, FileAccess.Read);

                try
                {
                    (Nca patchNca, Nca controlNca) = ApplicationLoader.GetGameUpdateDataFromPartition(_virtualFileSystem, new PartitionFileSystem(file.AsStorage()), _titleId.ToString("x16"), 0);

                    if (controlNca != null && patchNca != null)
                    {
                        ApplicationControlProperty controlData = new();

                        using UniqueRef<IFile> nacpFile = new();

                        controlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None).OpenFile(ref nacpFile.Ref(), "/control.nacp".ToU8Span(), OpenMode.Read).ThrowIfFailure();
                        nacpFile.Get.Read(out _, 0, SpanHelpers.AsByteSpan(ref controlData), ReadOption.None).ThrowIfFailure();

                        _titleUpdates.Add(new TitleUpdateModel(controlData, path));

                        foreach (var update in _titleUpdates)
                        {
                            update.IsEnabled = false;
                        }

                        _titleUpdates.Last().IsEnabled = true;
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogUpdateAddUpdateErrorMessage]);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance[LocaleKeys.DialogDlcLoadNcaErrorMessage], ex.Message, path));
                    });
                }
            }
        }

        private void RemoveUpdates(bool removeSelectedOnly = false)
        {
            if (removeSelectedOnly)
            {
                _titleUpdates.RemoveAll(_titleUpdates.Where(x => x.IsEnabled && !x.IsNoUpdate).ToList());
            }
            else
            {
                _titleUpdates.RemoveAll(_titleUpdates.Where(x => !x.IsNoUpdate).ToList());
            }

            _titleUpdates.FirstOrDefault(x => x.IsNoUpdate).IsEnabled = true;

            SortUpdates();
            PrintHeading();
        }

        public void RemoveSelected()
        {
            RemoveUpdates(true);
        }

        public void RemoveAll()
        {
            RemoveUpdates();
        }

        public async void Add()
        {
            OpenFileDialog dialog = new()
            {
                Title         = LocaleManager.Instance[LocaleKeys.SelectUpdateDialogTitle],
                AllowMultiple = true
            };

            dialog.Filters.Add(new FileDialogFilter
            {
                Name       = "NSP", 
                Extensions = { "nsp" }
            });

            string[] files = await dialog.ShowAsync(this);

            if (files != null)
            {
                foreach (string file in files)
                {
                    AddUpdate(file);
                }
            }

            SortUpdates();
            PrintHeading();
        }

        private void SortUpdates()
        {
            var list = _titleUpdates.ToList();

            list.Sort((first, second) =>
            {
                if (string.IsNullOrEmpty(first.Control.DisplayVersionString.ToString()))
                {
                    return -1;
                }
                else if (string.IsNullOrEmpty(second.Control.DisplayVersionString.ToString()))
                {
                    return 1;
                }

                return Version.Parse(first.Control.DisplayVersionString.ToString()).CompareTo(Version.Parse(second.Control.DisplayVersionString.ToString())) * -1;
            });

            _titleUpdates.Clear();
            _titleUpdates.AddRange(list);
        }

        public void Save()
        {
            _titleUpdateWindowData.Paths.Clear();

            _titleUpdateWindowData.Selected = "";

            foreach (TitleUpdateModel update in _titleUpdates)
            {
                _titleUpdateWindowData.Paths.Add(update.Path);

                if (update.IsEnabled)
                {
                    _titleUpdateWindowData.Selected = update.Path;
                }
            }

            using (FileStream titleUpdateJsonStream = File.Create(_titleUpdateJsonPath, 4096, FileOptions.WriteThrough))
            {
                titleUpdateJsonStream.Write(Encoding.UTF8.GetBytes(JsonHelper.Serialize(_titleUpdateWindowData, true)));
            }

            if (Owner is MainWindow window)
            {
                window.ViewModel.LoadApplications();
            }

            Close();
        }
    }
}