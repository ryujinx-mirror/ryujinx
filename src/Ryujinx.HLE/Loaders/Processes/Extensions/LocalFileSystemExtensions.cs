using LibHac.Common;
using LibHac.FsSystem;
using LibHac.Loader;
using LibHac.Ns;
using Ryujinx.HLE.Loaders.Processes.Extensions;
using ApplicationId = LibHac.Ncm.ApplicationId;

namespace Ryujinx.HLE.Loaders.Processes
{
    static class LocalFileSystemExtensions
    {
        public static ProcessResult Load(this LocalFileSystem exeFs, Switch device, string romFsPath = "")
        {
            MetaLoader metaLoader = exeFs.GetNpdm();
            var        nacpData   = new BlitStruct<ApplicationControlProperty>(1);
            ulong      programId  = metaLoader.GetProgramId();

            device.Configuration.VirtualFileSystem.ModLoader.CollectMods(
                new[] { programId },
                device.Configuration.VirtualFileSystem.ModLoader.GetModsBasePath(),
                device.Configuration.VirtualFileSystem.ModLoader.GetSdModsBasePath());

            if (programId != 0)
            {
                ProcessLoaderHelper.EnsureSaveData(device, new ApplicationId(programId), nacpData);
            }

            ProcessResult processResult = exeFs.Load(device, nacpData, metaLoader);

            // Load RomFS.
            if (!string.IsNullOrEmpty(romFsPath))
            {
                device.Configuration.VirtualFileSystem.LoadRomFs(processResult.ProcessId, romFsPath);
            }

            return processResult;
        }
    }
}
