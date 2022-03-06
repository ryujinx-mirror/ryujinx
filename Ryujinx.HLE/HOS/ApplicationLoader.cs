using ARMeilleure.Translation.PTC;
using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Shim;
using LibHac.FsSystem;
using LibHac.Loader;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using static LibHac.Fs.ApplicationSaveDataManagement;
using static Ryujinx.HLE.HOS.ModLoader;
using ApplicationId = LibHac.Ncm.ApplicationId;
using Path = System.IO.Path;

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

        private readonly Switch _device;
        private string _titleName;
        private string _displayVersion;
        private BlitStruct<ApplicationControlProperty> _controlData;

        public BlitStruct<ApplicationControlProperty> ControlData => _controlData;
        public string TitleName => _titleName;
        public string DisplayVersion => _displayVersion;

        public ulong TitleId { get; private set; }
        public bool TitleIs64Bit { get; private set; }

        public string TitleIdText => TitleId.ToString("x16");

        public ApplicationLoader(Switch device)
        {
            _device = device;
            _controlData = new BlitStruct<ApplicationControlProperty>(1);
        }

        public void LoadCart(string exeFsDir, string romFsFile = null)
        {
            if (romFsFile != null)
            {
                _device.Configuration.VirtualFileSystem.LoadRomFs(romFsFile);
            }

            LocalFileSystem codeFs = new LocalFileSystem(exeFsDir);

            MetaLoader metaData = ReadNpdm(codeFs);

            _device.Configuration.VirtualFileSystem.ModLoader.CollectMods(
                new[] { TitleId }, 
                _device.Configuration.VirtualFileSystem.ModLoader.GetModsBasePath(), 
                _device.Configuration.VirtualFileSystem.ModLoader.GetSdModsBasePath());

            if (TitleId != 0)
            {
                EnsureSaveData(new ApplicationId(TitleId));
            }

            LoadExeFs(codeFs, metaData);
        }

        public static (Nca main, Nca patch, Nca control) GetGameData(VirtualFileSystem fileSystem, PartitionFileSystem pfs, int programIndex)
        {
            Nca mainNca = null;
            Nca patchNca = null;
            Nca controlNca = null;

            fileSystem.ImportTickets(pfs);

            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
            {
                using var ncaFile = new UniqueRef<IFile>();

                pfs.OpenFile(ref ncaFile.Ref(), fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(fileSystem.KeySet, ncaFile.Release().AsStorage());

                int ncaProgramIndex = (int)(nca.Header.TitleId & 0xF);

                if (ncaProgramIndex != programIndex)
                {
                    continue;
                }

                if (nca.Header.ContentType == NcaContentType.Program)
                {
                    int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                    if (nca.SectionExists(NcaSectionType.Data) && nca.Header.GetFsHeader(dataIndex).IsPatchSection())
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
                using var ncaFile = new UniqueRef<IFile>();

                pfs.OpenFile(ref ncaFile.Ref(), fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(fileSystem.KeySet, ncaFile.Release().AsStorage());

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
            Xci xci = new Xci(_device.Configuration.VirtualFileSystem.KeySet, file.AsStorage());

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
                (mainNca, patchNca, controlNca) = GetGameData(_device.Configuration.VirtualFileSystem, securePartition, _device.Configuration.UserChannelPersistence.Index);

                RegisterProgramMapInfo(securePartition).ThrowIfFailure();
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

            _device.Configuration.ContentManager.LoadEntries(_device);
            _device.Configuration.ContentManager.ClearAocData();
            _device.Configuration.ContentManager.AddAocData(securePartition, xciFile, mainNca.Header.TitleId, _device.Configuration.FsIntegrityCheckLevel);

            LoadNca(mainNca, patchNca, controlNca);
        }

        public void LoadNsp(string nspFile)
        {
            FileStream file = new FileStream(nspFile, FileMode.Open, FileAccess.Read);
            PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

            Nca mainNca;
            Nca patchNca;
            Nca controlNca;

            try
            {
                (mainNca, patchNca, controlNca) = GetGameData(_device.Configuration.VirtualFileSystem, nsp, _device.Configuration.UserChannelPersistence.Index);

                RegisterProgramMapInfo(nsp).ThrowIfFailure();
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
                _device.Configuration.ContentManager.ClearAocData();
                _device.Configuration.ContentManager.AddAocData(nsp, nspFile, mainNca.Header.TitleId, _device.Configuration.FsIntegrityCheckLevel);

                LoadNca(mainNca, patchNca, controlNca);

                return;
            }

            // This is not a normal NSP, it's actually a ExeFS as a NSP
            LoadExeFs(nsp);
        }

        public void LoadNca(string ncaFile)
        {
            FileStream file = new FileStream(ncaFile, FileMode.Open, FileAccess.Read);
            Nca nca = new Nca(_device.Configuration.VirtualFileSystem.KeySet, file.AsStorage(false));

            LoadNca(nca, null, null);
        }

        private void LoadNca(Nca mainNca, Nca patchNca, Nca controlNca)
        {
            if (mainNca.Header.ContentType != NcaContentType.Program)
            {
                Logger.Error?.Print(LogClass.Loader, "Selected NCA is not a \"Program\" NCA");

                return;
            }

            IStorage dataStorage = null;
            IFileSystem codeFs = null;

            (Nca updatePatchNca, Nca updateControlNca) = GetGameUpdateData(_device.Configuration.VirtualFileSystem, mainNca.Header.TitleId.ToString("x16"), _device.Configuration.UserChannelPersistence.Index, out _);

            if (updatePatchNca != null)
            {
                patchNca = updatePatchNca;
            }

            if (updateControlNca != null)
            {
                controlNca = updateControlNca;
            }

            // Load program 0 control NCA as we are going to need it for display version.
            (_, Nca updateProgram0ControlNca) = GetGameUpdateData(_device.Configuration.VirtualFileSystem, mainNca.Header.TitleId.ToString("x16"), 0, out _);

            // Load Aoc
            string titleAocMetadataPath = Path.Combine(AppDataManager.GamesDirPath, mainNca.Header.TitleId.ToString("x16"), "dlc.json");

            if (File.Exists(titleAocMetadataPath))
            {
                List<DlcContainer> dlcContainerList = JsonHelper.DeserializeFromFile<List<DlcContainer>>(titleAocMetadataPath);

                foreach (DlcContainer dlcContainer in dlcContainerList)
                {
                    foreach (DlcNca dlcNca in dlcContainer.DlcNcaList)
                    {
                        if (File.Exists(dlcContainer.Path))
                        {
                            _device.Configuration.ContentManager.AddAocItem(dlcNca.TitleId, dlcContainer.Path, dlcNca.Path, dlcNca.Enabled);
                        }
                        else
                        {
                            Logger.Warning?.Print(LogClass.Application, $"Cannot find AddOnContent file {dlcContainer.Path}. It may have been moved or renamed.");
                        }
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

            MetaLoader metaData = ReadNpdm(codeFs);

            _device.Configuration.VirtualFileSystem.ModLoader.CollectMods(
                _device.Configuration.ContentManager.GetAocTitleIds().Prepend(TitleId), 
                _device.Configuration.VirtualFileSystem.ModLoader.GetModsBasePath(), 
                _device.Configuration.VirtualFileSystem.ModLoader.GetSdModsBasePath());

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
            if (updateProgram0ControlNca != null && _device.Configuration.UserChannelPersistence.Index != 0)
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
                IStorage newStorage = _device.Configuration.VirtualFileSystem.ModLoader.ApplyRomFsMods(TitleId, dataStorage);

                _device.Configuration.VirtualFileSystem.SetRomFs(newStorage.AsStream(FileAccess.Read));
            }

            // Don't create save data for system programs.
            if (TitleId != 0 && (TitleId < SystemProgramId.Start.Value || TitleId > SystemAppletId.End.Value))
            {
                // Multi-program applications can technically use any program ID for the main program, but in practice they always use 0 in the low nibble.
                // We'll know if this changes in the future because stuff will get errors when trying to mount the correct save.
                EnsureSaveData(new ApplicationId(TitleId & ~0xFul));
            }

            LoadExeFs(codeFs, metaData);

            Logger.Info?.Print(LogClass.Loader, $"Application Loaded: {TitleName} v{DisplayVersion} [{TitleIdText}] [{(TitleIs64Bit ? "64-bit" : "32-bit")}]");
        }

        // Sets TitleId, so be sure to call before using it
        private MetaLoader ReadNpdm(IFileSystem fs)
        {
            using var npdmFile = new UniqueRef<IFile>();

            Result result = fs.OpenFile(ref npdmFile.Ref(), "/main.npdm".ToU8Span(), OpenMode.Read);

            MetaLoader metaData;

            if (ResultFs.PathNotFound.Includes(result))
            {
                Logger.Warning?.Print(LogClass.Loader, "NPDM file not found, using default values!");

                metaData = GetDefaultNpdm();
            }
            else
            {
                npdmFile.Get.GetSize(out long fileSize).ThrowIfFailure();

                var npdmBuffer = new byte[fileSize];
                npdmFile.Get.Read(out _, 0, npdmBuffer).ThrowIfFailure();

                metaData = new MetaLoader();
                metaData.Load(npdmBuffer).ThrowIfFailure();
            }

            metaData.GetNpdm(out var npdm).ThrowIfFailure();

            TitleId = npdm.Aci.Value.ProgramId.Value;
            TitleIs64Bit = (npdm.Meta.Value.Flags & 1) != 0;
            _device.System.LibHacHorizonManager.ArpIReader.ApplicationId = new LibHac.ApplicationId(TitleId);

            return metaData;
        }

        private static void ReadControlData(Switch device, Nca controlNca, ref BlitStruct<ApplicationControlProperty> controlData, ref string titleName, ref string displayVersion)
        {
            using var controlFile = new UniqueRef<IFile>();

            IFileSystem controlFs = controlNca.OpenFileSystem(NcaSectionType.Data, device.System.FsIntegrityCheckLevel);
            Result result = controlFs.OpenFile(ref controlFile.Ref(), "/control.nacp".ToU8Span(), OpenMode.Read);

            if (result.IsSuccess())
            {
                result = controlFile.Get.Read(out long bytesRead, 0, controlData.ByteSpan, ReadOption.None);

                if (result.IsSuccess() && bytesRead == controlData.ByteSpan.Length)
                {
                    titleName = controlData.Value.Title[(int)device.System.State.DesiredTitleLanguage].NameString.ToString();

                    if (string.IsNullOrWhiteSpace(titleName))
                    {
                        titleName = controlData.Value.Title.ItemsRo.ToArray().FirstOrDefault(x => x.Name[0] != 0).NameString.ToString();
                    }

                    displayVersion = controlData.Value.DisplayVersionString.ToString();
                }
            }
            else
            {
                controlData.ByteSpan.Clear();
            }
        }

        private void LoadExeFs(IFileSystem codeFs, MetaLoader metaData = null)
        {
            if (_device.Configuration.VirtualFileSystem.ModLoader.ReplaceExefsPartition(TitleId, ref codeFs))
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

                using var nsoFile = new UniqueRef<IFile>();

                codeFs.OpenFile(ref nsoFile.Ref(), $"/{name}".ToU8Span(), OpenMode.Read).ThrowIfFailure();

                nsos[i] = new NsoExecutable(nsoFile.Release().AsStorage(), name);
            }

            // ExeFs file replacements
            ModLoadResult modLoadResult = _device.Configuration.VirtualFileSystem.ModLoader.ApplyExefsMods(TitleId, nsos);

            // collect the nsos, ignoring ones that aren't used
            NsoExecutable[] programs = nsos.Where(x => x != null).ToArray();

            // take the npdm from mods if present
            if (modLoadResult.Npdm != null)
            {
                metaData = modLoadResult.Npdm;
            }

            _device.Configuration.VirtualFileSystem.ModLoader.ApplyNsoPatches(TitleId, programs);

            _device.Configuration.ContentManager.LoadEntries(_device);

            bool usePtc = _device.System.EnablePtc;

            // Don't use PPTC if ExeFs files have been replaced.
            usePtc &= !modLoadResult.Modified;

            if (_device.System.EnablePtc && !usePtc)
            {
                Logger.Warning?.Print(LogClass.Ptc, $"Detected unsupported ExeFs modifications. PPTC disabled.");
            }

            Graphics.Gpu.GraphicsConfig.TitleId = TitleIdText;
            _device.Gpu.HostInitalized.Set();

            Ptc.Initialize(TitleIdText, DisplayVersion, usePtc, _device.Configuration.MemoryManagerMode);

            metaData.GetNpdm(out Npdm npdm).ThrowIfFailure();
            ProgramLoader.LoadNsos(_device.System.KernelContext, out ProcessTamperInfo tamperInfo, metaData, new ProgramInfo(in npdm), executables: programs);

            _device.Configuration.VirtualFileSystem.ModLoader.LoadCheats(TitleId, tamperInfo, _device.TamperMachine);
        }

        public void LoadProgram(string filePath)
        {
            MetaLoader metaData = GetDefaultNpdm();
            metaData.GetNpdm(out Npdm npdm).ThrowIfFailure();
            ProgramInfo programInfo = new ProgramInfo(in npdm);

            bool isNro = Path.GetExtension(filePath).ToLower() == ".nro";

            IExecutable executable;

            if (isNro)
            {
                FileStream input = new FileStream(filePath, FileMode.Open);
                NroExecutable obj = new NroExecutable(input.AsStorage());

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
                            ulong iconSize = reader.ReadUInt64();

                            ulong nacpOffset = reader.ReadUInt64();
                            ulong nacpSize = reader.ReadUInt64();

                            ulong romfsOffset = reader.ReadUInt64();
                            ulong romfsSize = reader.ReadUInt64();

                            if (romfsSize != 0)
                            {
                                _device.Configuration.VirtualFileSystem.SetRomFs(new HomebrewRomFsStream(input, obj.FileSize + (long)romfsOffset));
                            }

                            if (nacpSize != 0)
                            {
                                input.Seek(obj.FileSize + (long)nacpOffset, SeekOrigin.Begin);

                                reader.Read(ControlData.ByteSpan);

                                ref ApplicationControlProperty nacp = ref ControlData.Value;

                                programInfo.Name = nacp.Title[(int)_device.System.State.DesiredTitleLanguage].NameString.ToString();

                                if (string.IsNullOrWhiteSpace(programInfo.Name))
                                {
                                    programInfo.Name = nacp.Title.ItemsRo.ToArray().FirstOrDefault(x => x.Name[0] != 0).NameString.ToString();
                                }

                                if (nacp.PresenceGroupId != 0)
                                {
                                    programInfo.ProgramId = nacp.PresenceGroupId;
                                }
                                else if (nacp.SaveDataOwnerId != 0)
                                {
                                    programInfo.ProgramId = nacp.SaveDataOwnerId;
                                }
                                else if (nacp.AddOnContentBaseId != 0)
                                {
                                    programInfo.ProgramId = nacp.AddOnContentBaseId - 0x1000;
                                }
                                else
                                {
                                    programInfo.ProgramId = 0000000000000000;
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

            _device.Configuration.ContentManager.LoadEntries(_device);

            _titleName = programInfo.Name;
            TitleId = programInfo.ProgramId;
            TitleIs64Bit = (npdm.Meta.Value.Flags & 1) != 0;
            _device.System.LibHacHorizonManager.ArpIReader.ApplicationId = new LibHac.ApplicationId(TitleId);

            // Explicitly null titleid to disable the shader cache
            Graphics.Gpu.GraphicsConfig.TitleId = null;
            _device.Gpu.HostInitalized.Set();

            ProgramLoader.LoadNsos(_device.System.KernelContext, out ProcessTamperInfo tamperInfo, metaData, programInfo, executables: executable);

            _device.Configuration.VirtualFileSystem.ModLoader.LoadCheats(TitleId, tamperInfo, _device.TamperMachine);
        }

        private MetaLoader GetDefaultNpdm()
        {
            Assembly asm = Assembly.GetCallingAssembly();

            using (Stream npdmStream = asm.GetManifestResourceStream("Ryujinx.HLE.Homebrew.npdm"))
            {
                var npdmBuffer = new byte[npdmStream.Length];
                npdmStream.Read(npdmBuffer);

                var metaLoader = new MetaLoader();
                metaLoader.Load(npdmBuffer).ThrowIfFailure();

                return metaLoader;
            }
        }

        private static (ulong applicationId, int programCount) GetMultiProgramInfo(VirtualFileSystem fileSystem, PartitionFileSystem pfs)
        {
            ulong mainProgramId = 0;
            Span<bool> hasIndex = stackalloc bool[0x10];

            fileSystem.ImportTickets(pfs);

            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
            {
                using var ncaFile = new UniqueRef<IFile>();

                pfs.OpenFile(ref ncaFile.Ref(), fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(fileSystem.KeySet, ncaFile.Release().AsStorage());

                if (nca.Header.ContentType != NcaContentType.Program)
                {
                    continue;
                }

                int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                if (nca.SectionExists(NcaSectionType.Data) && nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                {
                    continue;
                }

                ulong currentProgramId = nca.Header.TitleId;
                ulong currentMainProgramId = currentProgramId & ~0xFFFul;

                if (mainProgramId == 0 && currentMainProgramId != 0)
                {
                    mainProgramId = currentMainProgramId;
                }

                if (mainProgramId != currentMainProgramId)
                {
                    // As far as I know there aren't any multi-application game cards containing multi-program applications,
                    // so because multi-application game cards are the only way we should run into multiple applications
                    // we'll just return that there's a single program.
                    return (mainProgramId, 1);
                }

                hasIndex[(int)(currentProgramId & 0xF)] = true;
            }

            int programCount = 0;

            for (int i = 0; i < hasIndex.Length && hasIndex[i]; i++)
            {
                programCount++;
            }

            return (mainProgramId, programCount);
        }

        private Result RegisterProgramMapInfo(PartitionFileSystem pfs)
        {
            (ulong applicationId, int programCount) = GetMultiProgramInfo(_device.Configuration.VirtualFileSystem, pfs);

            if (programCount <= 0)
                return Result.Success;

            Span<ProgramIndexMapInfo> mapInfo = stackalloc ProgramIndexMapInfo[0x10];

            for (int i = 0; i < programCount; i++)
            {
                mapInfo[i].ProgramId = new ProgramId(applicationId + (uint)i);
                mapInfo[i].MainProgramId = new ProgramId(applicationId);
                mapInfo[i].ProgramIndex = (byte)i;
            }

            return _device.System.LibHacHorizonManager.NsClient.Fs.RegisterProgramIndexMapInfo(mapInfo.Slice(0, programCount));
        }

        private Result EnsureSaveData(ApplicationId applicationId)
        {
            Logger.Info?.Print(LogClass.Application, "Ensuring required savedata exists.");

            Uid user = _device.System.AccountManager.LastOpenedUser.UserId.ToLibHacUid();

            ref ApplicationControlProperty control = ref ControlData.Value;

            if (LibHac.Common.Utilities.IsZeros(ControlData.ByteSpan))
            {
                // If the current application doesn't have a loaded control property, create a dummy one
                // and set the savedata sizes so a user savedata will be created.
                control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                control.UserAccountSaveDataSize = 0x4000;
                control.UserAccountSaveDataJournalSize = 0x4000;
                control.SaveDataOwnerId = applicationId.Value;

                Logger.Warning?.Print(LogClass.Application,
                    "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.");
            }

            HorizonClient hos = _device.System.LibHacHorizonManager.RyujinxClient;
            Result resultCode = hos.Fs.EnsureApplicationCacheStorage(out _, out _, applicationId, in control);

            if (resultCode.IsFailure())
            {
                Logger.Error?.Print(LogClass.Application, $"Error calling EnsureApplicationCacheStorage. Result code {resultCode.ToStringWithName()}");

                return resultCode;
            }

            resultCode = hos.Fs.EnsureApplicationSaveData(out _, applicationId, in control, in user);

            if (resultCode.IsFailure())
            {
                Logger.Error?.Print(LogClass.Application, $"Error calling EnsureApplicationSaveData. Result code {resultCode.ToStringWithName()}");
            }

            return resultCode;
        }
    }
}
