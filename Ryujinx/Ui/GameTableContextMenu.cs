using Gtk;
using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Shim;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ncm;
using LibHac.Ns;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

using static LibHac.Fs.ApplicationSaveDataManagement;

namespace Ryujinx.Ui
{
    public class GameTableContextMenu : Menu
    {
        private readonly ListStore         _gameTableStore;
        private readonly TreeIter          _rowIter;
        private readonly VirtualFileSystem _virtualFileSystem;

        private readonly BlitStruct<ApplicationControlProperty> _controlData;

        private MessageDialog _dialog;
        private bool          _cancel;

        public GameTableContextMenu(ListStore gameTableStore, BlitStruct<ApplicationControlProperty> controlData, TreeIter rowIter, VirtualFileSystem virtualFileSystem)
        {
            _gameTableStore    = gameTableStore;
            _rowIter           = rowIter;
            _virtualFileSystem = virtualFileSystem;
            _controlData       = controlData;

            MenuItem openSaveUserDir = new MenuItem("Open User Save Directory")
            {
                Sensitive   = !Utilities.IsEmpty(controlData.ByteSpan) && controlData.Value.UserAccountSaveDataSize > 0,
                TooltipText = "Open the directory which contains Application's User Saves."
            };

            MenuItem openSaveDeviceDir = new MenuItem("Open Device Save Directory")
            {
                Sensitive   = !Utilities.IsEmpty(controlData.ByteSpan) && controlData.Value.DeviceSaveDataSize > 0,
                TooltipText = "Open the directory which contains Application's Device Saves."
            };

            MenuItem openSaveBcatDir = new MenuItem("Open BCAT Save Directory")
            {
                Sensitive   = !Utilities.IsEmpty(controlData.ByteSpan) && controlData.Value.BcatDeliveryCacheStorageSize > 0,
                TooltipText = "Open the directory which contains Application's BCAT Saves."
            };

            MenuItem manageTitleUpdates = new MenuItem("Manage Title Updates")
            {
                TooltipText = "Open the Title Update management window"
            };

            MenuItem manageDlc = new MenuItem("Manage DLC")
            {
                TooltipText = "Open the DLC management window"
            };

            MenuItem openTitleModDir = new MenuItem("Open Mods Directory")
            {
                TooltipText = "Open the directory which contains Application's Mods."
            };

            string ext    = System.IO.Path.GetExtension(_gameTableStore.GetValue(_rowIter, 9).ToString()).ToLower();
            bool   hasNca = ext == ".nca" || ext == ".nsp" || ext == ".pfs0" || ext == ".xci";

            MenuItem extractMenu = new MenuItem("Extract Data");

            MenuItem extractRomFs = new MenuItem("RomFS")
            {
                Sensitive   = hasNca,
                TooltipText = "Extract the RomFS section from Application's current config (including updates)."
            };

            MenuItem extractExeFs = new MenuItem("ExeFS")
            {
                Sensitive   = hasNca,
                TooltipText = "Extract the ExeFS section from Application's current config (including updates)."
            };

            MenuItem extractLogo = new MenuItem("Logo")
            {
                Sensitive   = hasNca,
                TooltipText = "Extract the Logo section from Application's current config (including updates)."
            };

            Menu extractSubMenu = new Menu();
            
            extractSubMenu.Append(extractExeFs);
            extractSubMenu.Append(extractRomFs);
            extractSubMenu.Append(extractLogo);

            extractMenu.Submenu = extractSubMenu;

            MenuItem managePtcMenu = new MenuItem("Cache Management");

            MenuItem purgePtcCache = new MenuItem("Purge PPTC cache")
            {
                TooltipText = "Delete the Application's PPTC cache."
            };
            
            MenuItem openPtcDir = new MenuItem("Open PPTC directory")
            {
                TooltipText = "Open the directory which contains Application's PPTC cache."
            };
            
            Menu managePtcSubMenu = new Menu();
            
            managePtcSubMenu.Append(purgePtcCache);
            managePtcSubMenu.Append(openPtcDir);
            
            managePtcMenu.Submenu = managePtcSubMenu;

            openSaveUserDir.Activated    += OpenSaveUserDir_Clicked;
            openSaveDeviceDir.Activated  += OpenSaveDeviceDir_Clicked;
            openSaveBcatDir.Activated    += OpenSaveBcatDir_Clicked;
            manageTitleUpdates.Activated += ManageTitleUpdates_Clicked;
            manageDlc.Activated          += ManageDlc_Clicked;
            openTitleModDir.Activated    += OpenTitleModDir_Clicked;
            extractRomFs.Activated       += ExtractRomFs_Clicked;
            extractExeFs.Activated       += ExtractExeFs_Clicked;
            extractLogo.Activated        += ExtractLogo_Clicked;
            purgePtcCache.Activated      += PurgePtcCache_Clicked;
            openPtcDir.Activated         += OpenPtcDir_Clicked;
            
            this.Add(openSaveUserDir);
            this.Add(openSaveDeviceDir);
            this.Add(openSaveBcatDir);
            this.Add(new SeparatorMenuItem());
            this.Add(manageTitleUpdates);
            this.Add(manageDlc);
            this.Add(openTitleModDir);
            this.Add(new SeparatorMenuItem());
            this.Add(managePtcMenu);
            this.Add(extractMenu);
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

                if (LibHac.Utilities.IsEmpty(controlHolder.ByteSpan))
                {
                    // If the current application doesn't have a loaded control property, create a dummy one
                    // and set the savedata sizes so a user savedata will be created.
                    control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                    // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                    control.UserAccountSaveDataSize        = 0x4000;
                    control.UserAccountSaveDataJournalSize = 0x4000;

                    Logger.Warning?.Print(LogClass.Application,
                        "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.");
                }

                Uid user = new Uid(1, 0);

                result = EnsureApplicationSaveData(_virtualFileSystem.FsClient, out _, new LibHac.Ncm.ApplicationId(titleId), ref control, ref user);

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

        private void ExtractSection(NcaSectionType ncaSectionType, int programIndex = 0)
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
                            Logger.Error?.Print(LogClass.Application, "Extraction failed. The main NCA was not present in the selected file.");

                            Gtk.Application.Invoke(delegate
                            {
                                GtkDialog.CreateErrorDialog("Extraction failed. The main NCA was not present in the selected file.");
                            });

                            return;
                        }


                        (Nca updatePatchNca, _) = ApplicationLoader.GetGameUpdateData(_virtualFileSystem, mainNca.Header.TitleId.ToString("x16"), programIndex, out _);

                        if (updatePatchNca != null)
                        {
                            patchNca = updatePatchNca;
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
                                Logger.Error?.Print(LogClass.Application, $"LibHac returned error code: {resultCode.Value.ErrorCode}");

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
            filter.SetProgramId(new ProgramId(titleId));

            if (!TryFindSaveData(titleName, titleId, _controlData, filter, out ulong saveDataId))
            {
                return;
            }

            string saveDir = GetSaveDataDirectory(saveDataId);

            Process.Start(new ProcessStartInfo
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

        private void ManageDlc_Clicked(object sender, EventArgs args)
        {
            string titleName = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[0];
            string titleId   = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();

            DlcWindow dlcWindow = new DlcWindow(titleId, titleName, _virtualFileSystem);
            dlcWindow.Show();
        }

        private void OpenTitleModDir_Clicked(object sender, EventArgs args)
        {
            string titleId = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();

            var modsBasePath = _virtualFileSystem.ModLoader.GetModsBasePath();
            var titleModsPath = _virtualFileSystem.ModLoader.GetTitleDir(modsBasePath, titleId);

            Process.Start(new ProcessStartInfo
            {
                FileName = titleModsPath,
                UseShellExecute = true,
                Verb = "open"
            });
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

        private void OpenPtcDir_Clicked(object sender, EventArgs args)
        {
            string titleId = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n")[1].ToLower();
            string ptcDir  = System.IO.Path.Combine(AppDataManager.GamesDirPath, titleId, "cache", "cpu");
            
            string mainPath   = System.IO.Path.Combine(ptcDir, "0");
            string backupPath = System.IO.Path.Combine(ptcDir, "1");

            if (!Directory.Exists(ptcDir))
            {
                Directory.CreateDirectory(ptcDir);
                Directory.CreateDirectory(mainPath);
                Directory.CreateDirectory(backupPath);
            }
            
            Process.Start(new ProcessStartInfo
            {
                FileName        = ptcDir,
                UseShellExecute = true,
                Verb            = "open"
            });
        }
        
        private void PurgePtcCache_Clicked(object sender, EventArgs args)
        {
            string[] tableEntry = _gameTableStore.GetValue(_rowIter, 2).ToString().Split("\n");
            string titleId = tableEntry[1].ToLower();
            
            DirectoryInfo mainDir   = new DirectoryInfo(System.IO.Path.Combine(AppDataManager.GamesDirPath, titleId, "cache", "cpu", "0"));
            DirectoryInfo backupDir = new DirectoryInfo(System.IO.Path.Combine(AppDataManager.GamesDirPath, titleId, "cache", "cpu", "1"));
            
            MessageDialog warningDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, null)
            {
                Title          = "Ryujinx - Warning",
                Text           = $"You are about to delete the PPTC cache for '{tableEntry[0]}'. Are you sure you want to proceed?",
                WindowPosition = WindowPosition.Center
            };

            List<FileInfo> cacheFiles = new List<FileInfo>();

            if (mainDir.Exists)   { cacheFiles.AddRange(mainDir.EnumerateFiles("*.cache")); }
            if (backupDir.Exists) { cacheFiles.AddRange(backupDir.EnumerateFiles("*.cache")); }

            if (cacheFiles.Count > 0 && warningDialog.Run() == (int)ResponseType.Yes)
            {
                foreach (FileInfo file in cacheFiles)
                {
                    try 
                    { 
                        file.Delete(); 
                    }
                    catch(Exception e)
                    {
                        Logger.Error?.Print(LogClass.Application, $"Error purging PPTC cache {file.Name}: {e}");
                    }
                }
            }

            warningDialog.Dispose();
        }
    }
}
