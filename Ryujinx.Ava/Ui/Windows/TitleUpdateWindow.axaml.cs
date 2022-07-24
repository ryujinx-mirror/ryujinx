using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Ns;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Path = System.IO.Path;
using SpanHelpers = LibHac.Common.SpanHelpers;
using LibHac.Tools.FsSystem;
using Avalonia.Threading;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class TitleUpdateWindow : StyleableWindow
    {
        private readonly string _updateJsonPath;
        private TitleUpdateMetadata _titleUpdateWindowData;

        public VirtualFileSystem VirtualFileSystem { get; }

        internal AvaloniaList<TitleUpdateModel> TitleUpdates { get; set; } = new AvaloniaList<TitleUpdateModel>();
        public string TitleId { get; }
        public string TitleName { get; }

        public string Heading => string.Format(LocaleManager.Instance["GameUpdateWindowHeading"], TitleName, TitleId.ToUpper());

        public TitleUpdateWindow()
        {
            DataContext = this;

            InitializeComponent();
            AttachDebugDevTools();
            
            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["UpdateWindowTitle"];
        }

        public TitleUpdateWindow(VirtualFileSystem virtualFileSystem, string titleId, string titleName)
        {
            VirtualFileSystem = virtualFileSystem;
            TitleId = titleId;
            TitleName = titleName;

            _updateJsonPath = Path.Combine(AppDataManager.GamesDirPath, titleId, "updates.json");

            try
            {
                _titleUpdateWindowData = JsonHelper.DeserializeFromFile<TitleUpdateMetadata>(_updateJsonPath);
            }
            catch
            {
                _titleUpdateWindowData = new TitleUpdateMetadata {Selected = "", Paths = new List<string>()};
            }

            DataContext = this;

            InitializeComponent();
            AttachDebugDevTools();
            
            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["UpdateWindowTitle"];

            LoadUpdates();
        }

        [Conditional("DEBUG")]
        private void AttachDebugDevTools()
        {
            this.AttachDevTools();
        }

        private void LoadUpdates()
        {
            TitleUpdates.Add(new TitleUpdateModel(default, string.Empty, true));

            foreach (string path in _titleUpdateWindowData.Paths)
            {
                AddUpdate(path);
            }

            if (_titleUpdateWindowData.Selected == "")
            {
                TitleUpdates[0].IsEnabled = true;
            }
            else
            {
                TitleUpdateModel selected = TitleUpdates.FirstOrDefault(x => x.Path == _titleUpdateWindowData.Selected);
                List<TitleUpdateModel> enabled = TitleUpdates.Where(x => x.IsEnabled).ToList();

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
            if (File.Exists(path) && !TitleUpdates.Any(x => x.Path == path))
            {
                using (FileStream file = new(path, FileMode.Open, FileAccess.Read))
                {
                    PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

                    try
                    {
                        (Nca patchNca, Nca controlNca) =
                            ApplicationLoader.GetGameUpdateDataFromPartition(VirtualFileSystem, nsp, TitleId, 0);

                        if (controlNca != null && patchNca != null)
                        {
                            ApplicationControlProperty controlData = new ApplicationControlProperty();

                            using var nacpFile = new UniqueRef<IFile>();

                            controlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None)
                                .OpenFile(ref nacpFile.Ref(), "/control.nacp".ToU8Span(), OpenMode.Read)
                                .ThrowIfFailure();
                            nacpFile.Get.Read(out _, 0, SpanHelpers.AsByteSpan(ref controlData), ReadOption.None)
                                .ThrowIfFailure();

                            TitleUpdates.Add(new TitleUpdateModel(controlData, path));
                        }
                        else
                        {
                            Dispatcher.UIThread.Post(async () =>
                            {
                                await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance["DialogUpdateAddUpdateErrorMessage"]);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance["DialogDlcLoadNcaErrorMessage"], ex.Message, path));
                        });
                    }
                }
            }
        }

        private void RemoveUpdates(bool removeSelectedOnly = false)
        {
            if (removeSelectedOnly)
            {
                TitleUpdates.RemoveAll(TitleUpdates.Where(x => x.IsEnabled && !x.IsNoUpdate).ToList());
            }
            else
            {
                TitleUpdates.RemoveAll(TitleUpdates.Where(x => !x.IsNoUpdate).ToList());
            }

            TitleUpdates.FirstOrDefault(x => x.IsNoUpdate).IsEnabled = true;

            SortUpdates();
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
            OpenFileDialog dialog = new OpenFileDialog() { Title = LocaleManager.Instance["SelectUpdateDialogTitle"], AllowMultiple = true };

            dialog.Filters.Add(new FileDialogFilter { Name = "NSP", Extensions = { "nsp" } });

            string[] files = await dialog.ShowAsync(this);

            if (files != null)
            {
                foreach (string file in files)
                {
                    AddUpdate(file);
                }
            }

            SortUpdates();
        }

        private void SortUpdates()
        {
            var list = TitleUpdates.ToList();

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

                return Version.Parse(first.Control.DisplayVersionString.ToString())
                    .CompareTo(Version.Parse(second.Control.DisplayVersionString.ToString())) * -1;
            });

            TitleUpdates.Clear();

            TitleUpdates.AddRange(list);
        }

        public void Save()
        {
            _titleUpdateWindowData.Paths.Clear();

            _titleUpdateWindowData.Selected = "";

            foreach (TitleUpdateModel update in TitleUpdates)
            {
                _titleUpdateWindowData.Paths.Add(update.Path);

                if (update.IsEnabled)
                {
                    _titleUpdateWindowData.Selected = update.Path;
                }
            }

            using (FileStream dlcJsonStream = File.Create(_updateJsonPath, 4096, FileOptions.WriteThrough))
            {
                dlcJsonStream.Write(Encoding.UTF8.GetBytes(JsonHelper.Serialize(_titleUpdateWindowData, true)));
            }

            if (Owner is MainWindow window)
            {
                window.ViewModel.LoadApplications();
            }

            Close();
        }
    }
}