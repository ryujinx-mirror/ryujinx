using ARMeilleure.Translation.PTC;
using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ns;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using static LibHac.Fs.ApplicationSaveDataManagement;
using static Ryujinx.HLE.HOS.ModLoader;
using ApplicationId = LibHac.Ncm.ApplicationId;

namespace Ryujinx.HLE.HOS
{
    using JsonHelper = Common.Utilities.JsonHelper;

    public class ApplicationLoader
    {
        // Binaries from exefs are loaded into mem in this order. Do not change.
        internal static readonly string[] ExeFsPrefixes =
        {
            "rtld",
            "main",
            "subsdk0",
            "subsdk1",
            "subsdk2",
            "subsdk3",
            "subsdk4",
            "subsdk5",
            "subsdk6",
            "subsdk7",
            "subsdk8",
            "subsdk9",
            "sdk"
        };

        private readonly Switch            _device;
        private readonly ContentManager    _contentManager;
        private readonly VirtualFileSystem _fileSystem;

        private string _titleName;
        private string _displayVersion;
        private BlitStruct<ApplicationControlProperty> _controlData;

        public BlitStruct<ApplicationControlProperty> ControlData => _controlData;
        public string TitleName => _titleName;
        public string DisplayVersion => _displayVersion;

        public ulong  TitleId      { get; private set; }
        public bool   TitleIs64Bit { get; private set; }

        public string TitleIdText => TitleId.ToString("x16");

        public ApplicationLoader(Switch device, VirtualFileSystem fileSystem, ContentManager contentManager)
        {
            _device         = device;
            _contentManager = contentManager;
            _fileSystem     = fileSystem;

            _controlData = new BlitStruct<ApplicationControlProperty>(1);
        }

        public void LoadCart(string exeFsDir, string romFsFile = null)
        {
            if (romFsFile != null)
            {
                _fileSystem.LoadRomFs(romFsFile);
            }

            LocalFileSystem codeFs = new LocalFileSystem(exeFsDir);

            Npdm metaData = ReadNpdm(codeFs);

            _fileSystem.ModLoader.CollectMods(new[] { TitleId }, _fileSystem.ModLoader.GetModsBasePath());

            if (TitleId != 0)
            {
                EnsureSaveData(new ApplicationId(TitleId));
            }

            LoadExeFs(codeFs, metaData);
        }

        public static (Nca main, Nca patch, Nca control) GetGameData(VirtualFileSystem fileSystem, PartitionFileSystem pfs, int programIndex)
        {
            Nca mainNca    = null;
            Nca patchNca   = null;
            Nca controlNca = null;

            fileSystem.ImportTickets(pfs);

            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
            {
                pfs.OpenFile(out IFile ncaFile, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(fileSystem.KeySet, ncaFile.AsStorage());

                int ncaProgramIndex = (int)(nca.Header.TitleId & 0xF);

                if (ncaProgramIndex != programIndex)
                {
                    continue;
                }

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

        public static (Nca patch, Nca control) GetGameUpdateDataFromPartition(VirtualFileSystem fileSystem, PartitionFileSystem pfs, string titleId, int programIndex)
        {
            Nca patchNca = null;
            Nca controlNca = null;

            fileSystem.ImportTickets(pfs);

            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
            {
                pfs.OpenFile(out IFile ncaFile, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(fileSystem.KeySet, ncaFile.AsStorage());

                int ncaProgramIndex = (int)(nca.Header.TitleId & 0xF);

                if (ncaProgramIndex != programIndex)
                {
                    continue;
                }

                if ($"{nca.Header.TitleId.ToString("x16")[..^3]}000" != titleId)
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

            return (patchNca, controlNca);
        }

        public static (Nca patch, Nca control) GetGameUpdateData(VirtualFileSystem fileSystem, string titleId, int programIndex, out string updatePath)
        {
            updatePath = null;

            if (ulong.TryParse(titleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdBase))
            {
                // Clear the program index part.
                titleIdBase &= 0xFFFFFFFFFFFFFFF0;

                // Load update informations if existing.
                string titleUpdateMetadataPath = Path.Combine(AppDataManager.GamesDirPath, titleIdBase.ToString("x16"), "updates.json");

                if (File.Exists(titleUpdateMetadataPath))
                {
                    updatePath = JsonHelper.DeserializeFromFile<TitleUpdateMetadata>(titleUpdateMetadataPath).Selected;

                    if (File.Exists(updatePath))
                    {
                        FileStream file = new FileStream(updatePath, FileMode.Open, FileAccess.Read);
                        PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

                        return GetGameUpdateDataFromPartition(fileSystem, nsp, titleIdBase.ToString("x16"), programIndex);
                    }
                }
            }

            return (null, null);
        }

        public void LoadXci(string xciFile)
        {
            FileStream file = new FileStream(xciFile, FileMode.Open, FileAccess.Read);
            Xci        xci  = new Xci(_fileSystem.KeySet, file.AsStorage());

            if (!xci.HasPartition(XciPartitionType.Secure))
            {
                Logger.Error?.Print(LogClass.Loader, "Unable to load XCI: Could not find XCI secure partition");

                return;
            }

            PartitionFileSystem securePartition = xci.OpenPartition(XciPartitionType.Secure);

            Nca mainNca;
            Nca patchNca;
            Nca controlNca;

            try
            {
                (mainNca, patchNca, controlNca) = GetGameData(_fileSystem, securePartition, _device.UserChannelPersistence.Index);
            }
            catch (Exception e)
            {
                Logger.Error?.Print(LogClass.Loader, $"Unable to load XCI: {e.Message}");

                return;
            }

            if (mainNca == null)
            {
                Logger.Error?.Print(LogClass.Loader, "Unable to load XCI: Could not find Main NCA");

                return;
            }

            _contentManager.LoadEntries(_device);
            _contentManager.ClearAocData();
            _contentManager.AddAocData(securePartition, xciFile, mainNca.Header.TitleId);

            LoadNca(mainNca, patchNca, controlNca);
        }

        public void LoadNsp(string nspFile)
        {
            FileStream          file = new FileStream(nspFile, FileMode.Open, FileAccess.Read);
            PartitionFileSystem nsp  = new PartitionFileSystem(file.AsStorage());

            Nca mainNca;
            Nca patchNca;
            Nca controlNca;

            try
            {
                (mainNca, patchNca, controlNca) = GetGameData(_fileSystem, nsp, _device.UserChannelPersistence.Index);
            }
            catch (Exception e)
            {
                Logger.Error?.Print(LogClass.Loader, $"Unable to load NSP: {e.Message}");

                return;
            }

            if (mainNca == null)
            {
                Logger.Error?.Print(LogClass.Loader, "Unable to load NSP: Could not find Main NCA");

                return;
            }

            if (mainNca != null)
            {
                _contentManager.ClearAocData();
                _contentManager.AddAocData(nsp, nspFile, mainNca.Header.TitleId);

                LoadNca(mainNca, patchNca, controlNca);

                return;
            }

            // This is not a normal NSP, it's actually a ExeFS as a NSP
            LoadExeFs(nsp);
        }

        public void LoadNca(string ncaFile)
        {
            FileStream file = new FileStream(ncaFile, FileMode.Open, FileAccess.Read);
            Nca        nca  = new Nca(_fileSystem.KeySet, file.AsStorage(false));

            LoadNca(nca, null, null);
        }

        private void LoadNca(Nca mainNca, Nca patchNca, Nca controlNca)
        {
            if (mainNca.Header.ContentType != NcaContentType.Program)
            {
                Logger.Error?.Print(LogClass.Loader, "Selected NCA is not a \"Program\" NCA");

                return;
            }

            IStorage    dataStorage = null;
            IFileSystem codeFs      = null;

            (Nca updatePatchNca, Nca updateControlNca) = GetGameUpdateData(_fileSystem, mainNca.Header.TitleId.ToString("x16"), _device.UserChannelPersistence.Index, out _);

            if (updatePatchNca != null)
            {
                patchNca = updatePatchNca;
            }

            if (updateControlNca != null)
            {
                controlNca = updateControlNca;
            }

            // Load program 0 control NCA as we are going to need it for display version.
            (_, Nca updateProgram0ControlNca) = GetGameUpdateData(_fileSystem, mainNca.Header.TitleId.ToString("x16"), 0, out _);

            // Load Aoc
            string titleAocMetadataPath = Path.Combine(AppDataManager.GamesDirPath, mainNca.Header.TitleId.ToString("x16"), "dlc.json");

            if (File.Exists(titleAocMetadataPath))
            {
                List<DlcContainer> dlcContainerList = JsonHelper.DeserializeFromFile<List<DlcContainer>>(titleAocMetadataPath);

                foreach (DlcContainer dlcContainer in dlcContainerList)
                {
                    foreach (DlcNca dlcNca in dlcContainer.DlcNcaList)
                    {
                        _contentManager.AddAocItem(dlcNca.TitleId, dlcContainer.Path, dlcNca.Path, dlcNca.Enabled);
                    }
                }
            }

            if (patchNca == null)
            {
                if (mainNca.CanOpenSection(NcaSectionType.Data))
                {
                    dataStorage = mainNca.OpenStorage(NcaSectionType.Data, _device.System.FsIntegrityCheckLevel);
                }

                if (mainNca.CanOpenSection(NcaSectionType.Code))
                {
                    codeFs = mainNca.OpenFileSystem(NcaSectionType.Code, _device.System.FsIntegrityCheckLevel);
                }
            }
            else
            {
                if (patchNca.CanOpenSection(NcaSectionType.Data))
                {
                    dataStorage = mainNca.OpenStorageWithPatch(patchNca, NcaSectionType.Data, _device.System.FsIntegrityCheckLevel);
                }

                if (patchNca.CanOpenSection(NcaSectionType.Code))
                {
                    codeFs = mainNca.OpenFileSystemWithPatch(patchNca, NcaSectionType.Code, _device.System.FsIntegrityCheckLevel);
                }
            }

            if (codeFs == null)
            {
                Logger.Error?.Print(LogClass.Loader, "No ExeFS found in NCA");

                return;
            }

            Npdm metaData = ReadNpdm(codeFs);

            _fileSystem.ModLoader.CollectMods(_contentManager.GetAocTitleIds().Prepend(TitleId), _fileSystem.ModLoader.GetModsBasePath());

            if (controlNca != null)
            {
                ReadControlData(_device, controlNca, ref _controlData, ref _titleName, ref _displayVersion);
            }
            else
            {
                ControlData.ByteSpan.Clear();
            }

            // NOTE: Nintendo doesn't guarantee that the display version will be updated on sub programs when updating a multi program application.
            // BODY: As such, to avoid PTC cache confusion, we only trust the the program 0 display version when launching a sub program.
            if (updateProgram0ControlNca != null && _device.UserChannelPersistence.Index != 0)
            {
                string dummyTitleName = "";
                BlitStruct<ApplicationControlProperty> dummyControl = new BlitStruct<ApplicationControlProperty>(1);

                ReadControlData(_device, updateProgram0ControlNca, ref dummyControl, ref dummyTitleName, ref _displayVersion);
            }

            if (dataStorage == null)
            {
                Logger.Warning?.Print(LogClass.Loader, "No RomFS found in NCA");
            }
            else
            {
                IStorage newStorage = _fileSystem.ModLoader.ApplyRomFsMods(TitleId, dataStorage);

                _fileSystem.SetRomFs(newStorage.AsStream(FileAccess.Read));
            }

            if (TitleId != 0)
            {
                EnsureSaveData(new ApplicationId(TitleId));
            }

            LoadExeFs(codeFs, metaData);

            Logger.Info?.Print(LogClass.Loader, $"Application Loaded: {TitleName} v{DisplayVersion} [{TitleIdText}] [{(TitleIs64Bit ? "64-bit" : "32-bit")}]");
        }

        // Sets TitleId, so be sure to call before using it
        private Npdm ReadNpdm(IFileSystem fs)
        {
            Result result = fs.OpenFile(out IFile npdmFile, "/main.npdm".ToU8Span(), OpenMode.Read);

            Npdm metaData;

            if (ResultFs.PathNotFound.Includes(result))
            {
                Logger.Warning?.Print(LogClass.Loader, "NPDM file not found, using default values!");

                metaData = GetDefaultNpdm();
            }
            else
            {
                metaData = new Npdm(npdmFile.AsStream());
            }

            TitleId      = metaData.Aci0.TitleId;
            TitleIs64Bit = metaData.Is64Bit;

            return metaData;
        }

        private static void ReadControlData(Switch device, Nca controlNca, ref BlitStruct<ApplicationControlProperty> controlData, ref string titleName, ref string displayVersion)
        {
            IFileSystem controlFs = controlNca.OpenFileSystem(NcaSectionType.Data, device.System.FsIntegrityCheckLevel);
            Result      result    = controlFs.OpenFile(out IFile controlFile, "/control.nacp".ToU8Span(), OpenMode.Read);

            if (result.IsSuccess())
            {
                result = controlFile.Read(out long bytesRead, 0, controlData.ByteSpan, ReadOption.None);

                if (result.IsSuccess() && bytesRead == controlData.ByteSpan.Length)
                {
                    titleName = controlData.Value.Titles[(int)device.System.State.DesiredTitleLanguage].Name.ToString();

                    if (string.IsNullOrWhiteSpace(titleName))
                    {
                        titleName = controlData.Value.Titles.ToArray().FirstOrDefault(x => x.Name[0] != 0).Name.ToString();
                    }

                    displayVersion = controlData.Value.DisplayVersion.ToString();
                }
            }
            else
            {
                controlData.ByteSpan.Clear();
            }
        }

        private void LoadExeFs(IFileSystem codeFs, Npdm metaData = null)
        {
            if (_fileSystem.ModLoader.ReplaceExefsPartition(TitleId, ref codeFs))
            {
                metaData = null; //TODO: Check if we should retain old npdm
            }

            metaData ??= ReadNpdm(codeFs);

            NsoExecutable[] nsos = new NsoExecutable[ExeFsPrefixes.Length];

            for (int i = 0; i < nsos.Length; i++)
            {
                string name = ExeFsPrefixes[i];

                if (!codeFs.FileExists($"/{name}"))
                {
                    continue; // file doesn't exist, skip
                }

                Logger.Info?.Print(LogClass.Loader, $"Loading {name}...");

                codeFs.OpenFile(out IFile nsoFile, $"/{name}".ToU8Span(), OpenMode.Read).ThrowIfFailure();

                nsos[i] = new NsoExecutable(nsoFile.AsStorage(), name);
            }

            // ExeFs file replacements
            ModLoadResult modLoadResult = _fileSystem.ModLoader.ApplyExefsMods(TitleId, nsos);

            // collect the nsos, ignoring ones that aren't used
            NsoExecutable[] programs = nsos.Where(x => x != null).ToArray();

            // take the npdm from mods if present
            if (modLoadResult.Npdm != null)
            {
                metaData = modLoadResult.Npdm;
            }

            _fileSystem.ModLoader.ApplyNsoPatches(TitleId, programs);

            _contentManager.LoadEntries(_device);

            bool usePtc = _device.System.EnablePtc;

            // Don't use PPTC if ExeFs files have been replaced.
            usePtc &= !modLoadResult.Modified;

            if (_device.System.EnablePtc && !usePtc)
            {
                Logger.Warning?.Print(LogClass.Ptc, $"Detected unsupported ExeFs modifications. PPTC disabled.");
            }

            Graphics.Gpu.GraphicsConfig.TitleId = TitleIdText;
            _device.Gpu.HostInitalized.Set();

            Ptc.Initialize(TitleIdText, DisplayVersion, usePtc);

            ProgramLoader.LoadNsos(_device.System.KernelContext, out ProcessTamperInfo tamperInfo, metaData, executables: programs);

            _fileSystem.ModLoader.LoadCheats(TitleId, tamperInfo, _device.TamperMachine);
        }

        public void LoadProgram(string filePath)
        {
            Npdm metaData = GetDefaultNpdm();
            bool isNro    = Path.GetExtension(filePath).ToLower() == ".nro";

            IExecutable executable;

            if (isNro)
            {
                FileStream    input = new FileStream(filePath, FileMode.Open);
                NroExecutable obj   = new NroExecutable(input.AsStorage());

                executable = obj;

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
                            ulong iconSize   = reader.ReadUInt64();

                            ulong nacpOffset = reader.ReadUInt64();
                            ulong nacpSize   = reader.ReadUInt64();

                            ulong romfsOffset = reader.ReadUInt64();
                            ulong romfsSize   = reader.ReadUInt64();

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
                            Logger.Warning?.Print(LogClass.Loader, $"Unsupported ASET header version found \"{asetVersion}\"");
                        }
                    }
                }
            }
            else
            {
                executable = new NsoExecutable(new LocalStorage(filePath, FileAccess.Read), Path.GetFileNameWithoutExtension(filePath));
            }

            _contentManager.LoadEntries(_device);

            _titleName   = metaData.TitleName;
            TitleId      = metaData.Aci0.TitleId;
            TitleIs64Bit = metaData.Is64Bit;

            // Explicitly null titleid to disable the shader cache
            Graphics.Gpu.GraphicsConfig.TitleId = null;
            _device.Gpu.HostInitalized.Set();

            ProgramLoader.LoadNsos(_device.System.KernelContext, out ProcessTamperInfo tamperInfo, metaData, executables: executable);

            _fileSystem.ModLoader.LoadCheats(TitleId, tamperInfo, _device.TamperMachine);
        }

        private Npdm GetDefaultNpdm()
        {
            Assembly asm = Assembly.GetCallingAssembly();

            using (Stream npdmStream = asm.GetManifestResourceStream("Ryujinx.HLE.Homebrew.npdm"))
            {
                return new Npdm(npdmStream);
            }
        }

        private Result EnsureSaveData(ApplicationId applicationId)
        {
            Logger.Info?.Print(LogClass.Application, "Ensuring required savedata exists.");

            Uid user = _device.System.AccountManager.LastOpenedUser.UserId.ToLibHacUid();

            ref ApplicationControlProperty control = ref ControlData.Value;

            if (LibHac.Utilities.IsEmpty(ControlData.ByteSpan))
            {
                // If the current application doesn't have a loaded control property, create a dummy one
                // and set the savedata sizes so a user savedata will be created.
                control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                control.UserAccountSaveDataSize = 0x4000;
                control.UserAccountSaveDataJournalSize = 0x4000;

                Logger.Warning?.Print(LogClass.Application,
                    "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.");
            }

            FileSystemClient fileSystem = _fileSystem.FsClient;
            Result           resultCode = fileSystem.EnsureApplicationCacheStorage(out _, applicationId, ref control);

            if (resultCode.IsFailure())
            {
                Logger.Error?.Print(LogClass.Application, $"Error calling EnsureApplicationCacheStorage. Result code {resultCode.ToStringWithName()}");

                return resultCode;
            }

            resultCode = EnsureApplicationSaveData(fileSystem, out _, applicationId, ref control, ref user);

            if (resultCode.IsFailure())
            {
                Logger.Error?.Print(LogClass.Application, $"Error calling EnsureApplicationSaveData. Result code {resultCode.ToStringWithName()}");
            }

            return resultCode;
        }
    }
}
