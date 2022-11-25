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
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class DownloadableContentManagerWindow : StyleableWindow
    {
        private readonly List<DownloadableContentContainer> _downloadableContentContainerList;
        private readonly string                             _downloadableContentJsonPath;

        private VirtualFileSystem                      _virtualFileSystem    { get; }
        private AvaloniaList<DownloadableContentModel> _downloadableContents { get; set; }

        private ulong  _titleId   { get; }
        private string _titleName { get; }

        public DownloadableContentManagerWindow()
        {
            DataContext = this;

            InitializeComponent();

            Title = $"Ryujinx {Program.Version} - {LocaleManager.Instance["DlcWindowTitle"]} - {_titleName} ({_titleId:X16})";
        }

        public DownloadableContentManagerWindow(VirtualFileSystem virtualFileSystem, ulong titleId, string titleName)
        {
            _virtualFileSystem    = virtualFileSystem;
            _downloadableContents = new AvaloniaList<DownloadableContentModel>();

            _titleId   = titleId;
            _titleName = titleName;

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

            RemoveButton.IsEnabled = false;

            DlcDataGrid.SelectionChanged += DlcDataGrid_SelectionChanged;

            Title = $"Ryujinx {Program.Version} - {LocaleManager.Instance["DlcWindowTitle"]} - {_titleName} ({_titleId:X16})";

            LoadDownloadableContents();
            PrintHeading();
        }

        private void DlcDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveButton.IsEnabled = (DlcDataGrid.SelectedItems.Count > 0);
        }

        private void PrintHeading()
        {
            Heading.Text = string.Format(LocaleManager.Instance["DlcWindowHeading"], _downloadableContents.Count, _titleName, _titleId.ToString("X16"));
        }

        private void LoadDownloadableContents()
        {
            foreach (DownloadableContentContainer downloadableContentContainer in _downloadableContentContainerList)
            {
                if (File.Exists(downloadableContentContainer.ContainerPath))
                {
                    using FileStream containerFile = File.OpenRead(downloadableContentContainer.ContainerPath);

                    PartitionFileSystem partitionFileSystem = new(containerFile.AsStorage());

                    _virtualFileSystem.ImportTickets(partitionFileSystem);

                    foreach (DownloadableContentNca downloadableContentNca in downloadableContentContainer.DownloadableContentNcaList)
                    {
                        using UniqueRef<IFile> ncaFile = new();

                        partitionFileSystem.OpenFile(ref ncaFile.Ref(), downloadableContentNca.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                        Nca nca = TryOpenNca(ncaFile.Get.AsStorage(), downloadableContentContainer.ContainerPath);
                        if (nca != null)
                        {
                            _downloadableContents.Add(new DownloadableContentModel(nca.Header.TitleId.ToString("X16"),
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

        private Nca TryOpenNca(IStorage ncaStorage, string containerPath)
        {
            try
            {
                return new Nca(_virtualFileSystem.KeySet, ncaStorage);
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
            if (!File.Exists(path) || _downloadableContents.FirstOrDefault(x => x.ContainerPath == path) != null)
            {
                return;
            }

            using FileStream containerFile = File.OpenRead(path);

            PartitionFileSystem partitionFileSystem         = new(containerFile.AsStorage());
            bool                containsDownloadableContent = false;

            _virtualFileSystem.ImportTickets(partitionFileSystem);

            foreach (DirectoryEntryEx fileEntry in partitionFileSystem.EnumerateEntries("/", "*.nca"))
            {
                using var ncaFile = new UniqueRef<IFile>();

                partitionFileSystem.OpenFile(ref ncaFile.Ref(), fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = TryOpenNca(ncaFile.Get.AsStorage(), path);
                if (nca == null)
                {
                    continue;
                }

                if (nca.Header.ContentType == NcaContentType.PublicData)
                {
                    if ((nca.Header.TitleId & 0xFFFFFFFFFFFFE000) != _titleId)
                    {
                        break;
                    }

                    _downloadableContents.Add(new DownloadableContentModel(nca.Header.TitleId.ToString("X16"), path, fileEntry.FullPath, true));

                    containsDownloadableContent = true;
                }
            }

            if (!containsDownloadableContent)
            {
                await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance["DialogDlcNoDlcErrorMessage"]);
            }
        }

        private void RemoveDownloadableContents(bool removeSelectedOnly = false)
        {
            if (removeSelectedOnly)
            {
                AvaloniaList<DownloadableContentModel> removedItems = new();

                foreach (var item in DlcDataGrid.SelectedItems)
                {
                    removedItems.Add(item as DownloadableContentModel);
                }

                DlcDataGrid.SelectedItems.Clear();

                foreach (var item in removedItems)
                {
                    _downloadableContents.RemoveAll(_downloadableContents.Where(x => x.TitleId == item.TitleId).ToList());
                }
            }
            else
            {
                _downloadableContents.Clear();
            }

            PrintHeading();
        }

        public void RemoveSelected()
        {
            RemoveDownloadableContents(true);
        }

        public void RemoveAll()
        {
            RemoveDownloadableContents();
        }

        public void EnableAll()
        {
            foreach(var item in _downloadableContents)
            {
                item.Enabled = true;
            }
        }

        public void DisableAll()
        {
            foreach (var item in _downloadableContents)
            {
                item.Enabled = false;
            }
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

            PrintHeading();
        }

        public void Save()
        {
            _downloadableContentContainerList.Clear();

            DownloadableContentContainer container = default;

            foreach (DownloadableContentModel downloadableContent in _downloadableContents)
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