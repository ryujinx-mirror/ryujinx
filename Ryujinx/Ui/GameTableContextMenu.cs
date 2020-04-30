using Gtk;
using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Spl;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using static LibHac.Fs.ApplicationSaveDataManagement;
using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class GameTableContextMenu : Menu
    {
        private ListStore         _gameTableStore;
        private TreeIter          _rowIter;
        private VirtualFileSystem _virtualFileSystem;
        private MessageDialog     _dialog;
        private bool              _cancel;

        private BlitStruct<ApplicationControlProperty> _controlData;

#pragma warning disable CS0649
#pragma warning disable IDE0044
        [GUI] MenuItem _openSaveUserDir;
        [GUI] MenuItem _openSaveDeviceDir;
        [GUI] MenuItem _openSaveBcatDir;
        [GUI] MenuItem _manageTitleUpdates;
        [GUI] MenuItem _extractRomFs;
        [GUI] MenuItem _extractExeFs;
        [GUI] MenuItem _extractLogo;
#pragma warning restore CS0649
#pragma warning restore IDE0044

        public GameTableContextMenu(ListStore gameTableStore, BlitStruct<ApplicationControlProperty> controlData, TreeIter rowIter, VirtualFileSystem virtualFileSystem)
            : this(new Builder("Ryujinx.Ui.GameTableContextMenu.glade"), gameTableStore, controlData, rowIter, virtualFileSystem) { }

        private GameTableContextMenu(Builder builder, ListStore gameTableStore, BlitStruct<ApplicationControlProperty> controlData, TreeIter rowIter, VirtualFileSystem virtualFileSystem) : base(builder.GetObject("_contextMenu").Handle)
        {
            builder.Autoconnect(this);

            _gameTableStore    = gameTableStore;
            _rowIter           = rowIter;
            _virtualFileSystem = virtualFileSystem;
            _controlData       = controlData;

            _openSaveUserDir.Activated    += OpenSaveUserDir_Clicked;
            _openSaveDeviceDir.Activated  += OpenSaveDeviceDir_Clicked;
            _openSaveBcatDir.Activated    += OpenSaveBcatDir_Clicked;
            _manageTitleUpdates.Activated += ManageTitleUpdates_Clicked;
            _extractRomFs.Activated       += ExtractRomFs_Clicked;
            _extractExeFs.Activated       += ExtractExeFs_Clicked;
            _extractLogo.Activated        += ExtractLogo_Clicked;

            _openSaveUserDir.Sensitive   = !Util.IsEmpty(controlData.ByteSpan) && controlData.Value.UserAccountSaveDataSize > 0;
            _openSaveDeviceDir.Sensitive = !Util.IsEmpty(controlData.ByteSpan) && controlData.Value.DeviceSaveDataSize > 0;
            _openSaveBcatDir.Sensitive   = !Util.IsEmpty(controlData.ByteSpan) && controlData.Value.BcatDeliveryCacheStorageSize > 0;

            string ext = System.IO.Path.GetExtension(_gameTableStore.GetValue(_rowIter, 9).ToString()).ToLower();
            if (ext != ".nca" && ext != ".nsp" && ext != ".pfs0" && ext != ".xci")
            {
                _extractRomFs.Sensitive = false;
                _extractExeFs.Sensitive = false;
                _extractLogo.Sensitive  = false;
            }
        }

        private bool TryFindSaveData(string titleName, ulong titleId, BlitStruct<ApplicationControlProperty> controlHolder, SaveDataFilter filter, out ulong saveDataId)
        {
            saveDataId = default;

            Result result = _virtualFileSystem.FsClient.FindSaveDataWithFilter(out SaveDataInfo saveDataInfo, SaveDataSpaceId.User, ref filter);

            if (ResultFs.TargetNotFound.Includes(result))
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

                ref ApplicationControlProperty control = ref controlHolder.Value;

                if (LibHac.Util.IsEmpty(controlHolder.ByteSpan))
                {
                    // If the current application doesn't have a loaded control property, create a dummy one
                    // and set the savedata sizes so a user savedata will be created.
                    control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                    // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                    control.UserAccountSaveDataSize        = 0x4000;
                    control.UserAccountSaveDataJournalSize = 0x4000;

                    Logger.PrintWarning(LogClass.Application,
                        "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.");
                }

                Uid user = new Uid(1, 0);

                result = EnsureApplicationSaveData(_virtualFileSystem.FsClient, out _, new TitleId(titleId), ref control, ref user);

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
            string workingPath   = System.IO.Path.Combine(saveRootPath, "1");

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

        private void ExtractSection(NcaSectionType ncaSectionType)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the folder to extract into", null, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Extract", ResponseType.Accept);
            fileChooser.SetPosition(WindowPosition.Center);

            int    response    = fileChooser.Run();
            string destination = fileChooser.Filename;
            
            fileChooser.Dispose();

            if (response == (int)ResponseType.Accept)
            {
                Thread extractorThread = new Thread(() =>
                {
                    string sourceFile = _gameTableStore.GetValue(_rowIter, 9).ToString();

                    Gtk.Application.Invoke(delegate
                    {
                        _dialog = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Cancel, null)
                        {
                            Title          = "Ryujinx - NCA Section Extractor",
                            Icon           = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                            SecondaryText  = $"Extracting {ncaSectionType} section from {System.IO.Path.GetFileName(sourceFile)}...",
                            WindowPosition = WindowPosition.Center
                        };

                        int dialogResponse = _dialog.Run();
                        if (dialogResponse == (int)ResponseType.Cancel || dialogResponse == (int)ResponseType.DeleteEvent)
                        {
                            _cancel = true;
                            _dialog.Dispose();
                        }
                    });

                    using (FileStream file = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                    {
                        Nca mainNca  = null;
                        Nca patchNca = null;

                        if ((System.IO.Path.GetExtension(sourceFile).ToLower() == ".nsp")  ||
                            (System.IO.Path.GetExtension(sourceFile).ToLower() == ".pfs0") ||
                            (System.IO.Path.GetExtension(sourceFile).ToLower() == ".xci"))
                        {
                            PartitionFileSystem pfs;

                            if (System.IO.Path.GetExtension(sourceFile) == ".xci")
                            {
                                Xci xci = new Xci(_virtualFileSystem.KeySet, file.AsStorage());

                                pfs = xci.OpenPartition(XciPartitionType.Secure);
                            }
                            else
                            {
                                pfs = new PartitionFileSystem(file.AsStorage());
                            }

                            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
                            {
                                pfs.OpenFile(out IFile ncaFile, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                                Nca nca = new Nca(_virtualFileSystem.KeySet, ncaFile.AsStorage());

                                if (nca.Header.ContentType == NcaContentType.Program)
                                {
                                    int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                                    if (nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                                    {
                                        patchNca = nca;
                                    }
                                    else
                                    {
                                        mainNca = nca;
                                    }
                                }
                            }
                        }
                        else if (System.IO.Path.GetExtension(sourceFile).ToLower() == ".nca")
                        {
                            mainNca = new Nca(_virtualFileSystem.KeySet, file.AsStorage());
                        }

                        if (mainNca == null)
                        {
                            Logger.PrintError(LogClass.Application, "Extraction failed. The main NCA was not present in the selected file.");

                            Gtk.Application.Invoke(delegate
                            {
                                GtkDialog.CreateErrorDialog("Extraction failed. The main NCA was not present in the selected file.");
                            });

                            return;
                        }

                        string titleUpdateMetadataPath = System.IO.Path.Combine(_virtualFileSystem.GetBasePath(), "games", mainNca.Header.TitleId.ToString("x16"), "updates.json");

                        if (File.Exists(titleUpdateMetadataPath))
                        {
                            string updatePath = JsonHelper.DeserializeFromFile<TitleUpdateMetadata>(titleUpdateMetadataPath).Selected;

                            if (File.Exists(updatePath))
                            {
                                FileStream updateFile = new FileStream(updatePath, FileMode.Open, FileAccess.Read);
                                PartitionFileSystem nsp = new PartitionFileSystem(updateFile.AsStorage());

                                foreach (DirectoryEntryEx ticketEntry in nsp.EnumerateEntries("/", "*.tik"))
                                {
                                    Result result = nsp.OpenFile(out IFile ticketFile, ticketEntry.FullPath.ToU8Span(), OpenMode.Read);

                                    if (result.IsSuccess())
                                    {
                                        Ticket ticket = new Ticket(ticketFile.AsStream());

                                        _virtualFileSystem.KeySet.ExternalKeySet.Add(new LibHac.Fs.RightsId(ticket.RightsId), new AccessKey(ticket.GetTitleKey(_virtualFileSystem.KeySet)));
                                    }
                                }

                                foreach (DirectoryEntryEx fileEntry in nsp.EnumerateEntries("/", "*.nca"))
                                {
                                    nsp.OpenFile(out IFile ncaFile, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                                    Nca nca = new Nca(_virtualFileSystem.KeySet, ncaFile.AsStorage());

                                    if ($"{nca.Header.TitleId.ToString("x16")[..^3]}000" != mainNca.Header.TitleId.ToString("x16"))
                                    {
                                        break;
                                    }

                                    if (nca.Header.ContentType == NcaContentType.Program)
                                    {
                                        patchNca = nca;
                                    }
                                }
                            }
                        }

                        int index = Nca.GetSectionIndexFromType(ncaSectionType, mainNca.Header.ContentType);

                        IFileSystem ncaFileSystem = patchNca != null ? mainNca.OpenFileSystemWithPatch(patchNca, index, IntegrityCheckLevel.ErrorOnInvalid)
                                                                     : mainNca.OpenFileSystem(index, IntegrityCheckLevel.ErrorOnInvalid);

                        FileSystemClient fsClient = _virtualFileSystem.FsClient;

                        string source = DateTime.Now.ToFileTime().ToString().Substring(10);
                        string output = DateTime.Now.ToFileTime().ToString().Substring(10);

                        fsClient.Register(source.ToU8Span(), ncaFileSystem);
                        fsClient.Register(output.ToU8Span(), new LocalFileSystem(destination));

                        (Result? resultCode, bool canceled) = CopyDirectory(fsClient, $"{source}:/", $"{output}:/");

                        if (!canceled)
                        {
                            if (resultCode.Value.IsFailure())
                            {
                                Logger.PrintError(LogClass.Application, $"LibHac returned error code: {resultCode.Value.ErrorCode}");

                                Gtk.Application.Invoke(delegate
                                {
                                    _dialog?.Dispose();

                                    GtkDialog.CreateErrorDialog("Extraction failed. Read the log file for further information.");
                                });
                            }
                            else if (resultCode.Value.IsSuccess())
                            {
                                Gtk.Application.Invoke(delegate
                                {
                                    _dialog?.Dispose();

                                    MessageDialog dialog = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, null)
                                    {
                                        Title          = "Ryujinx - NCA Section Extractor",
                                        Icon           = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                                        SecondaryText  = "Extraction has completed successfully.",
                                        WindowPosition = WindowPosition.Center
                                    };

                                    dialog.Run();
                                    dialog.Dispose();
                                });
                            }
                        }

                        fsClient.Unmount(source.ToU8Span());
                        fsClient.Unmount(output.ToU8Span());
                    }
                });

                extractorThread.Name         = "GUI.NcaSectionExtractorThread";
                extractorThread.IsBackground = true;
                extractorThread.Start();
            }
        }

        private (Result? result, bool canceled) CopyDirectory(FileSystemClient fs, string sourcePath, string destPath)
        {
            Result rc = fs.OpenDirectory(out DirectoryHandle sourceHandle, sourcePath.ToU8Span(), OpenDirectoryMode.All);
            if (rc.IsFailure()) return (rc, false);

            using (sourceHandle)
            {
                foreach (DirectoryEntryEx entry in fs.EnumerateEntries(sourcePath, "*", SearchOptions.Default))
                {
                    if (_cancel)
                    {
                        return (null, true);
                    }

                    string subSrcPath = PathTools.Normalize(PathTools.Combine(sourcePath, entry.Name));
                    string subDstPath = PathTools.Normalize(PathTools.Combine(destPath, entry.Name));

                    if (entry.Type == DirectoryEntryType.Directory)
                    {
                        fs.EnsureDirectoryExists(subDstPath);

                        (Result? result, bool canceled) = CopyDirectory(fs, subSrcPath, subDstPath);
                        if (canceled || result.Value.IsFailure())
                        {
                            return (result, canceled);
                        }
                    }

                    if (entry.Type == DirectoryEntryType.File)
                    {
                        fs.CreateOrOverwriteFile(subDstPath, entry.Size);

                        rc = CopyFile(fs, subSrcPath, subDstPath);
                        if (rc.IsFailure()) return (rc, false);
                    }
                }
            }

            return (Result.Success, false);
        }

        public Result CopyFile(FileSystemClient fs, string sourcePath, string destPath)
        {
            Result rc = fs.OpenFile(out FileHandle sourceHandle, sourcePath.ToU8Span(), OpenMode.Read);
            if (rc.IsFailure()) return rc;

            using (sourceHandle)
            {
                rc = fs.OpenFile(out FileHandle destHandle, destPath.ToU8Span(), OpenMode.Write | OpenMode.AllowAppend);
                if (rc.IsFailure()) return rc;

                using (destHandle)
                {
                    const int maxBufferSize = 1024 * 1024;

                    rc = fs.GetFileSize(out long fileSize, sourceHandle);
                    if (rc.IsFailure()) return rc;

                    int bufferSize = (int)Math.Min(maxBufferSize, fileSize);

                    byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                    try
                    {
                        for (long offset = 0; offset < fileSize; offset += bufferSize)
                        {
                            int toRead = (int)Math.Min(fileSize - offset, bufferSize);
                            Span<byte> buf = buffer.AsSpan(0, toRead);

                            rc = fs.ReadFile(out long _, sourceHandle, offset, buf);
                            if (rc.IsFailure()) return rc;

                            rc = fs.WriteFile(destHandle, offset, buf);
                            if (rc.IsFailure()) return rc;
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }

                    rc = fs.FlushFile(destHandle);
                    if (rc.IsFailure()) return rc;
                }
            }

            return Result.Success;
        }

        // Events
        private void OpenSaveUserDir_Clicked(object sender, EventArgs args)
        {
            string titleName = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[0];
            string titleId   = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();

            if (!ulong.TryParse(titleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdNumber))
            {
                GtkDialog.CreateErrorDialog("UI error: The selected game did not have a valid title ID");

                return;
            }

            SaveDataFilter filter = new SaveDataFilter();
            filter.SetUserId(new UserId(1, 0));

            OpenSaveDir(titleName, titleIdNumber, filter);
        }

        private void OpenSaveDir(string titleName, ulong titleId, SaveDataFilter filter)
        {
            filter.SetProgramId(new TitleId(titleId));

            if (!TryFindSaveData(titleName, titleId, _controlData, filter, out ulong saveDataId))
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

        private void OpenSaveDeviceDir_Clicked(object sender, EventArgs args)
        {
            string titleName = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[0];
            string titleId   = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();

            if (!ulong.TryParse(titleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdNumber))
            {
                GtkDialog.CreateErrorDialog("UI error: The selected game did not have a valid title ID");

                return;
            }

            SaveDataFilter filter = new SaveDataFilter();
            filter.SetSaveDataType(SaveDataType.Device);

            OpenSaveDir(titleName, titleIdNumber, filter);
        }

        private void OpenSaveBcatDir_Clicked(object sender, EventArgs args)
        {
            string titleName = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[0];
            string titleId   = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();

            if (!ulong.TryParse(titleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdNumber))
            {
                GtkDialog.CreateErrorDialog("UI error: The selected game did not have a valid title ID");

                return;
            }

            SaveDataFilter filter = new SaveDataFilter();
            filter.SetSaveDataType(SaveDataType.Bcat);

            OpenSaveDir(titleName, titleIdNumber, filter);
        }

        private void ManageTitleUpdates_Clicked(object sender, EventArgs args)
        {
            string titleName = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[0];
            string titleId   = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();

            TitleUpdateWindow titleUpdateWindow = new TitleUpdateWindow(titleId, titleName, _virtualFileSystem);
            titleUpdateWindow.Show();
        }

        private void ExtractRomFs_Clicked(object sender, EventArgs args)
        {
            ExtractSection(NcaSectionType.Data);
        }

        private void ExtractExeFs_Clicked(object sender, EventArgs args)
        {
            ExtractSection(NcaSectionType.Code);
        }

        private void ExtractLogo_Clicked(object sender, EventArgs args)
        {
            ExtractSection(NcaSectionType.Logo);
        }
    }
}
