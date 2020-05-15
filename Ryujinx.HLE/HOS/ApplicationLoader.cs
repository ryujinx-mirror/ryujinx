using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Spl;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using static LibHac.Fs.ApplicationSaveDataManagement;

namespace Ryujinx.HLE.HOS
{
    using JsonHelper = Common.Utilities.JsonHelper;

    public class ApplicationLoader
    {
        private readonly Switch _device;
        private readonly ContentManager _contentManager;
        private readonly VirtualFileSystem _fileSystem;

        public IntegrityCheckLevel FsIntegrityCheckLevel => _device.System.FsIntegrityCheckLevel;

        public ulong TitleId { get; private set; }
        public string TitleIdText => TitleId.ToString("x16");
        public string TitleName { get; private set; }

        public string TitleVersionString { get; private set; }

        public bool TitleIs64Bit { get; private set; }

        public BlitStruct<ApplicationControlProperty> ControlData { get; set; }

        public ApplicationLoader(Switch device, VirtualFileSystem fileSystem, ContentManager contentManager)
        {
            _device = device;
            _contentManager = contentManager;
            _fileSystem = fileSystem;

            ControlData = new BlitStruct<ApplicationControlProperty>(1);
        }

        public void LoadCart(string exeFsDir, string romFsFile = null)
        {
            if (romFsFile != null)
            {
                _fileSystem.LoadRomFs(romFsFile);
            }

            LocalFileSystem codeFs = new LocalFileSystem(exeFsDir);

            LoadExeFs(codeFs, out _);

            if (TitleId != 0)
            {
                EnsureSaveData(new TitleId(TitleId));
            }
        }

        private (Nca Main, Nca Patch, Nca Control) GetGameData(PartitionFileSystem pfs)
        {
            Nca mainNca = null;
            Nca patchNca = null;
            Nca controlNca = null;

            _fileSystem.ImportTickets(pfs);

            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
            {
                pfs.OpenFile(out IFile ncaFile, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(_fileSystem.KeySet, ncaFile.AsStorage());

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
                else if (nca.Header.ContentType == NcaContentType.Control)
                {
                    controlNca = nca;
                }
            }

            return (mainNca, patchNca, controlNca);
        }

        public void LoadXci(string xciFile)
        {
            FileStream file = new FileStream(xciFile, FileMode.Open, FileAccess.Read);

            Xci xci = new Xci(_fileSystem.KeySet, file.AsStorage());

            if (!xci.HasPartition(XciPartitionType.Secure))
            {
                Logger.PrintError(LogClass.Loader, "Unable to load XCI: Could not find XCI secure partition");

                return;
            }

            PartitionFileSystem securePartition = xci.OpenPartition(XciPartitionType.Secure);

            Nca mainNca = null;
            Nca patchNca = null;
            Nca controlNca = null;

            try
            {
                (mainNca, patchNca, controlNca) = GetGameData(securePartition);
            }
            catch (Exception e)
            {
                Logger.PrintError(LogClass.Loader, $"Unable to load XCI: {e.Message}");

                return;
            }

            if (mainNca == null)
            {
                Logger.PrintError(LogClass.Loader, "Unable to load XCI: Could not find Main NCA");

                return;
            }

            _contentManager.LoadEntries(_device);

            LoadNca(mainNca, patchNca, controlNca);
        }

        public void LoadNsp(string nspFile)
        {
            FileStream file = new FileStream(nspFile, FileMode.Open, FileAccess.Read);

            PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

            Nca mainNca = null;
            Nca patchNca = null;
            Nca controlNca = null;

            try
            {
                (mainNca, patchNca, controlNca) = GetGameData(nsp);
            }
            catch (Exception e)
            {
                Logger.PrintError(LogClass.Loader, $"Unable to load NSP: {e.Message}");

                return;
            }

            if (mainNca == null)
            {
                Logger.PrintError(LogClass.Loader, "Unable to load NSP: Could not find Main NCA");

                return;
            }

            if (mainNca != null)
            {
                LoadNca(mainNca, patchNca, controlNca);

                return;
            }

            // This is not a normal NSP, it's actually a ExeFS as a NSP
            LoadExeFs(nsp, out _);
        }

        public void LoadNca(string ncaFile)
        {
            FileStream file = new FileStream(ncaFile, FileMode.Open, FileAccess.Read);

            Nca nca = new Nca(_fileSystem.KeySet, file.AsStorage(false));

            LoadNca(nca, null, null);
        }

        private void LoadNca(Nca mainNca, Nca patchNca, Nca controlNca)
        {
            if (mainNca.Header.ContentType != NcaContentType.Program)
            {
                Logger.PrintError(LogClass.Loader, "Selected NCA is not a \"Program\" NCA");

                return;
            }

            IStorage dataStorage = null;
            IFileSystem codeFs = null;

            string titleUpdateMetadataPath = System.IO.Path.Combine(_fileSystem.GetBasePath(), "games", mainNca.Header.TitleId.ToString("x16"), "updates.json");

            if (File.Exists(titleUpdateMetadataPath))
            {
                string updatePath = JsonHelper.DeserializeFromFile<TitleUpdateMetadata>(titleUpdateMetadataPath).Selected;

                if (File.Exists(updatePath))
                {
                    FileStream file = new FileStream(updatePath, FileMode.Open, FileAccess.Read);
                    PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

                    _fileSystem.ImportTickets(nsp);

                    foreach (DirectoryEntryEx fileEntry in nsp.EnumerateEntries("/", "*.nca"))
                    {
                        nsp.OpenFile(out IFile ncaFile, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                        Nca nca = new Nca(_fileSystem.KeySet, ncaFile.AsStorage());

                        if ($"{nca.Header.TitleId.ToString("x16")[..^3]}000" != mainNca.Header.TitleId.ToString("x16"))
                        {
                            break;
                        }

                        if (nca.Header.ContentType == NcaContentType.Program)
                        {
                            patchNca = nca;
                        }
                        else if (nca.Header.ContentType == NcaContentType.Control)
                        {
                            controlNca = nca;
                        }
                    }
                }
            }

            if (patchNca == null)
            {
                if (mainNca.CanOpenSection(NcaSectionType.Data))
                {
                    dataStorage = mainNca.OpenStorage(NcaSectionType.Data, FsIntegrityCheckLevel);
                }

                if (mainNca.CanOpenSection(NcaSectionType.Code))
                {
                    codeFs = mainNca.OpenFileSystem(NcaSectionType.Code, FsIntegrityCheckLevel);
                }
            }
            else
            {
                if (patchNca.CanOpenSection(NcaSectionType.Data))
                {
                    dataStorage = mainNca.OpenStorageWithPatch(patchNca, NcaSectionType.Data, FsIntegrityCheckLevel);
                }

                if (patchNca.CanOpenSection(NcaSectionType.Code))
                {
                    codeFs = mainNca.OpenFileSystemWithPatch(patchNca, NcaSectionType.Code, FsIntegrityCheckLevel);
                }
            }

            if (codeFs == null)
            {
                Logger.PrintError(LogClass.Loader, "No ExeFS found in NCA");

                return;
            }

            if (dataStorage == null)
            {
                Logger.PrintWarning(LogClass.Loader, "No RomFS found in NCA");
            }
            else
            {
                _fileSystem.SetRomFs(dataStorage.AsStream(FileAccess.Read));
            }

            LoadExeFs(codeFs, out Npdm metaData);

            TitleId = metaData.Aci0.TitleId;
            TitleIs64Bit = metaData.Is64Bit;

            if (controlNca != null)
            {
                ReadControlData(controlNca);
            }
            else
            {
                ControlData.ByteSpan.Clear();
            }

            if (TitleId != 0)
            {
                EnsureSaveData(new TitleId(TitleId));
            }

            Logger.PrintInfo(LogClass.Loader, $"Application Loaded: {TitleName} v{TitleVersionString} [{TitleIdText}] [{(TitleIs64Bit ? "64-bit" : "32-bit")}]");
        }

        public void ReadControlData(Nca controlNca)
        {
            IFileSystem controlFs = controlNca.OpenFileSystem(NcaSectionType.Data, FsIntegrityCheckLevel);

            Result result = controlFs.OpenFile(out IFile controlFile, "/control.nacp".ToU8Span(), OpenMode.Read);

            if (result.IsSuccess())
            {
                result = controlFile.Read(out long bytesRead, 0, ControlData.ByteSpan, ReadOption.None);

                if (result.IsSuccess() && bytesRead == ControlData.ByteSpan.Length)
                {
                    TitleName = ControlData.Value
                        .Titles[(int)_device.System.State.DesiredTitleLanguage].Name.ToString();

                    if (string.IsNullOrWhiteSpace(TitleName))
                    {
                        TitleName = ControlData.Value.Titles.ToArray()
                            .FirstOrDefault(x => x.Name[0] != 0).Name.ToString();
                    }

                    TitleVersionString = ControlData.Value.DisplayVersion.ToString();
                }
            }
            else
            {
                ControlData.ByteSpan.Clear();
            }
        }

        private void LoadExeFs(IFileSystem codeFs, out Npdm metaData)
        {
            Result result = codeFs.OpenFile(out IFile npdmFile, "/main.npdm".ToU8Span(), OpenMode.Read);

            if (ResultFs.PathNotFound.Includes(result))
            {
                Logger.PrintWarning(LogClass.Loader, "NPDM file not found, using default values!");

                metaData = GetDefaultNpdm();
            }
            else
            {
                metaData = new Npdm(npdmFile.AsStream());
            }

            List<IExecutable> nsos = new List<IExecutable>();

            void LoadNso(string filename)
            {
                foreach (DirectoryEntryEx file in codeFs.EnumerateEntries("/", $"{filename}*"))
                {
                    if (Path.GetExtension(file.Name) != string.Empty)
                    {
                        continue;
                    }

                    Logger.PrintInfo(LogClass.Loader, $"Loading {file.Name}...");

                    codeFs.OpenFile(out IFile nsoFile, file.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                    NsoExecutable nso = new NsoExecutable(nsoFile.AsStorage());

                    nsos.Add(nso);
                }
            }

            TitleId = metaData.Aci0.TitleId;
            TitleIs64Bit = metaData.Is64Bit;

            LoadNso("rtld");
            LoadNso("main");
            LoadNso("subsdk");
            LoadNso("sdk");

            _contentManager.LoadEntries(_device);

            ProgramLoader.LoadNsos(_device.System.KernelContext, metaData, executables: nsos.ToArray());
        }

        public void LoadProgram(string filePath)
        {
            Npdm metaData = GetDefaultNpdm();

            bool isNro = Path.GetExtension(filePath).ToLower() == ".nro";

            IExecutable nro;

            if (isNro)
            {
                FileStream input = new FileStream(filePath, FileMode.Open);
                NroExecutable obj = new NroExecutable(input);
                nro = obj;

                // homebrew NRO can actually have some data after the actual NRO
                if (input.Length > obj.FileSize)
                {
                    input.Position = obj.FileSize;

                    BinaryReader reader = new BinaryReader(input);

                    uint asetMagic = reader.ReadUInt32();

                    if (asetMagic == 0x54455341)
                    {
                        uint asetVersion = reader.ReadUInt32();
                        if (asetVersion == 0)
                        {
                            ulong iconOffset = reader.ReadUInt64();
                            ulong iconSize = reader.ReadUInt64();

                            ulong nacpOffset = reader.ReadUInt64();
                            ulong nacpSize = reader.ReadUInt64();

                            ulong romfsOffset = reader.ReadUInt64();
                            ulong romfsSize = reader.ReadUInt64();

                            if (romfsSize != 0)
                            {
                                _fileSystem.SetRomFs(new HomebrewRomFsStream(input, obj.FileSize + (long)romfsOffset));
                            }

                            if (nacpSize != 0)
                            {
                                input.Seek(obj.FileSize + (long)nacpOffset, SeekOrigin.Begin);

                                reader.Read(ControlData.ByteSpan);

                                ref ApplicationControlProperty nacp = ref ControlData.Value;

                                metaData.TitleName = nacp.Titles[(int)_device.System.State.DesiredTitleLanguage].Name.ToString();

                                if (string.IsNullOrWhiteSpace(metaData.TitleName))
                                {
                                    metaData.TitleName = nacp.Titles.ToArray().FirstOrDefault(x => x.Name[0] != 0).Name.ToString();
                                }

                                if (nacp.PresenceGroupId != 0)
                                {
                                    metaData.Aci0.TitleId = nacp.PresenceGroupId;
                                }
                                else if (nacp.SaveDataOwnerId.Value != 0)
                                {
                                    metaData.Aci0.TitleId = nacp.SaveDataOwnerId.Value;
                                }
                                else if (nacp.AddOnContentBaseId != 0)
                                {
                                    metaData.Aci0.TitleId = nacp.AddOnContentBaseId - 0x1000;
                                }
                                else
                                {
                                    metaData.Aci0.TitleId = 0000000000000000;
                                }
                            }
                        }
                        else
                        {
                            Logger.PrintWarning(LogClass.Loader, $"Unsupported ASET header version found \"{asetVersion}\"");
                        }
                    }
                }
            }
            else
            {
                nro = new NsoExecutable(new LocalStorage(filePath, FileAccess.Read));
            }

            _contentManager.LoadEntries(_device);

            TitleName = metaData.TitleName;
            TitleId = metaData.Aci0.TitleId;
            TitleIs64Bit = metaData.Is64Bit;

            ProgramLoader.LoadNsos(_device.System.KernelContext, metaData, executables: nro);
        }

        private Npdm GetDefaultNpdm()
        {
            Assembly asm = Assembly.GetCallingAssembly();

            using (Stream npdmStream = asm.GetManifestResourceStream("Ryujinx.HLE.Homebrew.npdm"))
            {
                return new Npdm(npdmStream);
            }
        }

        private Result EnsureSaveData(TitleId titleId)
        {
            Logger.PrintInfo(LogClass.Application, "Ensuring required savedata exists.");

            Uid user = _device.System.State.Account.LastOpenedUser.UserId.ToLibHacUid();

            ref ApplicationControlProperty control = ref ControlData.Value;

            if (Util.IsEmpty(ControlData.ByteSpan))
            {
                // If the current application doesn't have a loaded control property, create a dummy one
                // and set the savedata sizes so a user savedata will be created.
                control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                control.UserAccountSaveDataSize = 0x4000;
                control.UserAccountSaveDataJournalSize = 0x4000;

                Logger.PrintWarning(LogClass.Application,
                    "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.");
            }

            FileSystemClient fs = _fileSystem.FsClient;

            Result rc = fs.EnsureApplicationCacheStorage(out _, titleId, ref control);

            if (rc.IsFailure())
            {
                Logger.PrintError(LogClass.Application, $"Error calling EnsureApplicationCacheStorage. Result code {rc.ToStringWithName()}");

                return rc;
            }

            rc = EnsureApplicationSaveData(fs, out _, titleId, ref control, ref user);

            if (rc.IsFailure())
            {
                Logger.PrintError(LogClass.Application, $"Error calling EnsureApplicationSaveData. Result code {rc.ToStringWithName()}");
            }

            return rc;
        }
    }
}