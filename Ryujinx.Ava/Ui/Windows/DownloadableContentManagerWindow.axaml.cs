using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class DownloadableContentManagerWindow : StyleableWindow
    {
        private readonly List<DownloadableContentContainer> _downloadableContentContainerList;
        private readonly string _downloadableContentJsonPath;

        public VirtualFileSystem VirtualFileSystem { get; }
        public AvaloniaList<DownloadableContentModel> DownloadableContents { get; set; } = new AvaloniaList<DownloadableContentModel>();
        public ulong TitleId { get; }
        public string TitleName { get; }

        public string Heading => string.Format(LocaleManager.Instance["DlcWindowHeading"], TitleName, TitleId.ToString("X16"));

        public DownloadableContentManagerWindow()
        {
            DataContext = this;

            InitializeComponent();

            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["DlcWindowTitle"];
        }

        public DownloadableContentManagerWindow(VirtualFileSystem virtualFileSystem, ulong titleId, string titleName)
        {
            VirtualFileSystem = virtualFileSystem;
            TitleId           = titleId;
            TitleName         = titleName;

            _downloadableContentJsonPath = Path.Combine(AppDataManager.GamesDirPath, titleId.ToString("x16"), "dlc.json");

            try
            {
                _downloadableContentContainerList = JsonHelper.DeserializeFromFile<List<DownloadableContentContainer>>(_downloadableContentJsonPath);
            }
            catch
            {
                _downloadableContentContainerList = new List<DownloadableContentContainer>();
            }

            DataContext = this;

            InitializeComponent();

            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["DlcWindowTitle"];

            LoadDownloadableContents();
        }

        private void LoadDownloadableContents()
        {
            foreach (DownloadableContentContainer downloadableContentContainer in _downloadableContentContainerList)
            {
                if (File.Exists(downloadableContentContainer.ContainerPath))
                {
                    using FileStream containerFile = File.OpenRead(downloadableContentContainer.ContainerPath);

                    PartitionFileSystem pfs = new PartitionFileSystem(containerFile.AsStorage());

                    VirtualFileSystem.ImportTickets(pfs);

                    foreach (DownloadableContentNca downloadableContentNca in downloadableContentContainer.DownloadableContentNcaList)
                    {
                        using var ncaFile = new UniqueRef<IFile>();

                        pfs.OpenFile(ref ncaFile.Ref(), downloadableContentNca.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                        Nca nca = TryCreateNca(ncaFile.Get.AsStorage(), downloadableContentContainer.ContainerPath);
                        if (nca != null)
                        {
                            DownloadableContents.Add(new DownloadableContentModel(nca.Header.TitleId.ToString("X16"),
                                                                                  downloadableContentContainer.ContainerPath,
                                                                                  downloadableContentNca.FullPath,
                                                                                  downloadableContentNca.Enabled));
                        }
                    }
                }
            }

            // NOTE: Save the list again to remove leftovers.
            Save();
        }

        private Nca TryCreateNca(IStorage ncaStorage, string containerPath)
        {
            try
            {
                return new Nca(VirtualFileSystem.KeySet, ncaStorage);
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance["DialogDlcLoadNcaErrorMessage"], ex.Message, containerPath));
                });
            }

            return null;
        }

        private async Task AddDownloadableContent(string path)
        {
            if (!File.Exists(path) || DownloadableContents.FirstOrDefault(x => x.ContainerPath == path) != null)
            {
                return;
            }

            using (FileStream containerFile = File.OpenRead(path))
            {
                PartitionFileSystem pfs = new PartitionFileSystem(containerFile.AsStorage());
                bool containsDownloadableContent = false;

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

                        DownloadableContents.Add(new DownloadableContentModel(nca.Header.TitleId.ToString("X16"), path, fileEntry.FullPath, true));

                        containsDownloadableContent = true;
                    }
                }

                if (!containsDownloadableContent)
                {
                    await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance["DialogDlcNoDlcErrorMessage"]);
                }
            }
        }

        private void RemoveDownloadableContents(bool removeSelectedOnly = false)
        {
            if (removeSelectedOnly)
            {
                DownloadableContents.RemoveAll(DownloadableContents.Where(x => x.Enabled).ToList());
            }
            else
            {
                DownloadableContents.Clear();
            }
        }

        public void RemoveSelected()
        {
            RemoveDownloadableContents(true);
        }

        public void RemoveAll()
        {
            RemoveDownloadableContents();
        }

        public async void Add()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            { 
                Title         = LocaleManager.Instance["SelectDlcDialogTitle"],
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
                   await AddDownloadableContent(file);
                }
            }
        }

        public void Save()
        {
            _downloadableContentContainerList.Clear();

            DownloadableContentContainer container = default;

            foreach (DownloadableContentModel downloadableContent in DownloadableContents)
            {
                if (container.ContainerPath != downloadableContent.ContainerPath)
                {
                    if (!string.IsNullOrWhiteSpace(container.ContainerPath))
                    {
                        _downloadableContentContainerList.Add(container);
                    }

                    container = new DownloadableContentContainer
                    {
                        ContainerPath              = downloadableContent.ContainerPath,
                        DownloadableContentNcaList = new List<DownloadableContentNca>()
                    };
                }

                container.DownloadableContentNcaList.Add(new DownloadableContentNca
                {
                    Enabled  = downloadableContent.Enabled,
                    TitleId  = Convert.ToUInt64(downloadableContent.TitleId, 16),
                    FullPath = downloadableContent.FullPath
                });
            }

            if (!string.IsNullOrWhiteSpace(container.ContainerPath))
            {
                _downloadableContentContainerList.Add(container);
            }

            using (FileStream downloadableContentJsonStream = File.Create(_downloadableContentJsonPath, 4096, FileOptions.WriteThrough))
            {
                downloadableContentJsonStream.Write(Encoding.UTF8.GetBytes(JsonHelper.Serialize(_downloadableContentContainerList, true)));
            }
        }

        public void SaveAndClose()
        {
            Save();
            Close();
        }
    }
}