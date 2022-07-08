using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Path = System.IO.Path;

namespace Ryujinx.Ava.Ui.Windows
{
    public class DlcManagerWindow : StyleableWindow
    {
        private readonly List<DlcContainer> _dlcContainerList;
        private readonly string _dlcJsonPath;

        public VirtualFileSystem VirtualFileSystem { get; }

        public AvaloniaList<DlcModel> Dlcs { get; set; }
        public Grid DlcGrid { get; private set; }
        public ulong TitleId { get; }
        public string TitleName { get; }

        public string Heading => string.Format(LocaleManager.Instance["DlcWindowHeading"], TitleName, TitleId.ToString("X16"));

        public DlcManagerWindow()
        {
            DataContext = this;

            InitializeComponent();
            AttachDebugDevTools();
            
            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["DlcWindowTitle"];
        }

        public DlcManagerWindow(VirtualFileSystem virtualFileSystem, ulong titleId, string titleName)
        {
            VirtualFileSystem = virtualFileSystem;
            TitleId = titleId;
            TitleName = titleName;

            _dlcJsonPath = Path.Combine(AppDataManager.GamesDirPath, titleId.ToString("x16"), "dlc.json");

            try
            {
                _dlcContainerList = JsonHelper.DeserializeFromFile<List<DlcContainer>>(_dlcJsonPath);
            }
            catch
            {
                _dlcContainerList = new List<DlcContainer>();
            }

            DataContext = this;

            InitializeComponent();
            AttachDebugDevTools();
            
            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["DlcWindowTitle"];

            LoadDlcs();
        }

        [Conditional("DEBUG")]
        private void AttachDebugDevTools()
        {
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            Dlcs = new AvaloniaList<DlcModel>();

            AvaloniaXamlLoader.Load(this);

            DlcGrid = this.FindControl<Grid>("DlcGrid");
        }

        private void LoadDlcs()
        {
            foreach (DlcContainer dlcContainer in _dlcContainerList)
            {
                using FileStream containerFile = File.OpenRead(dlcContainer.Path);

                PartitionFileSystem pfs = new PartitionFileSystem(containerFile.AsStorage());

                VirtualFileSystem.ImportTickets(pfs);

                foreach (DlcNca dlcNca in dlcContainer.DlcNcaList)
                {
                    using var ncaFile = new UniqueRef<IFile>();
                    pfs.OpenFile(ref ncaFile.Ref(), dlcNca.Path.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                    Nca nca = TryCreateNca(ncaFile.Get.AsStorage(), dlcContainer.Path);

                    if (nca != null)
                    {
                        Dlcs.Add(new DlcModel(nca.Header.TitleId.ToString("X16"), dlcContainer.Path, dlcNca.Path,
                            dlcNca.Enabled));
                    }
                }
            }
        }

        private Nca TryCreateNca(IStorage ncaStorage, string containerPath)
        {
            try
            {
                return new Nca(VirtualFileSystem.KeySet, ncaStorage);
            }
            catch (Exception ex)
            {
                ContentDialogHelper.CreateErrorDialog(this,
                    string.Format(LocaleManager.Instance[
                        "DialogDlcLoadNcaErrorMessage"], ex.Message, containerPath));
            }

            return null;
        }

        private void AddDlc(string path)
        {
            if (!File.Exists(path) || Dlcs.FirstOrDefault(x => x.ContainerPath == path) != null)
            {
                return;
            }

            using (FileStream containerFile = File.OpenRead(path))
            {
                PartitionFileSystem pfs = new PartitionFileSystem(containerFile.AsStorage());
                bool containsDlc = false;

                VirtualFileSystem.ImportTickets(pfs);

                foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
                {
                    using var ncaFile = new UniqueRef<IFile>();

                    pfs.OpenFile(ref ncaFile.Ref(), fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                    Nca nca = TryCreateNca(ncaFile.Get.AsStorage(), path);

                    if (nca == null)
                    {
                        continue;
                    }

                    if (nca.Header.ContentType == NcaContentType.PublicData)
                    {
                        if ((nca.Header.TitleId & 0xFFFFFFFFFFFFE000) != TitleId)
                        {
                            break;
                        }

                        Dlcs.Add(new DlcModel(nca.Header.TitleId.ToString("X16"), path, fileEntry.FullPath, true));

                        containsDlc = true;
                    }
                }

                if (!containsDlc)
                {
                    ContentDialogHelper.CreateErrorDialog(this, LocaleManager.Instance["DialogDlcNoDlcErrorMessage"]);
                }
            }
        }

        private void RemoveDlcs(bool removeSelectedOnly = false)
        {
            if (removeSelectedOnly)
            {
               Dlcs.RemoveAll(Dlcs.Where(x => x.IsEnabled).ToList());
            }
            else
            {
                Dlcs.Clear();
            }
        }

        public void RemoveSelected()
        {
            RemoveDlcs(true);
        }

        public void RemoveAll()
        {
            RemoveDlcs();
        }

        public async void Add()
        {
            OpenFileDialog dialog = new OpenFileDialog() { Title = LocaleManager.Instance["SelectDlcDialogTitle"], AllowMultiple = true };

            dialog.Filters.Add(new FileDialogFilter { Name = "NSP", Extensions = { "nsp" } });

            string[] files = await dialog.ShowAsync(this);

            if (files != null)
            {
                foreach (string file in files)
                {
                    AddDlc(file);
                }
            }
        }

        public void Save()
        {
            _dlcContainerList.Clear();

            DlcContainer container = default;

            foreach (DlcModel dlc in Dlcs)
            {
                if (container.Path != dlc.ContainerPath)
                {
                    if (!string.IsNullOrWhiteSpace(container.Path))
                    {
                        _dlcContainerList.Add(container);
                    }

                    container = new DlcContainer { Path = dlc.ContainerPath, DlcNcaList = new List<DlcNca>() };
                }

                container.DlcNcaList.Add(new DlcNca
                {
                    Enabled = dlc.IsEnabled,
                    TitleId = Convert.ToUInt64(dlc.TitleId, 16),
                    Path = dlc.FullPath
                });
            }

            if (!string.IsNullOrWhiteSpace(container.Path))
            {
                _dlcContainerList.Add(container);
            }

            using (FileStream dlcJsonStream = File.Create(_dlcJsonPath, 4096, FileOptions.WriteThrough))
            {
                dlcJsonStream.Write(Encoding.UTF8.GetBytes(JsonHelper.Serialize(_dlcContainerList, true)));
            }

            Close();
        }
    }
}