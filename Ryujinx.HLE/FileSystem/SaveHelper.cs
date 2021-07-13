using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using Ryujinx.HLE.HOS;
using System.IO;

namespace Ryujinx.HLE.FileSystem
{
    static class SaveHelper
    {
        public static IFileSystem OpenSystemSaveData(ServiceCtx context, ulong saveId)
        {
            SaveInfo saveInfo = new SaveInfo(0, (long)saveId, SaveDataType.SystemSaveData, SaveSpaceId.NandSystem);
            string   savePath = context.Device.FileSystem.GetSavePath(context, saveInfo, false);

            if (File.Exists(savePath))
            {
                string tempDirectoryPath = $"{savePath}_temp";

                Directory.CreateDirectory(tempDirectoryPath);

                IFileSystem outputFolder = new LocalFileSystem(tempDirectoryPath);

                using (LocalStorage systemSaveData = new LocalStorage(savePath, FileAccess.Read, FileMode.Open))
                {
                    IFileSystem saveFs = new LibHac.FsSystem.Save.SaveDataFileSystem(context.Device.System.KeySet, systemSaveData, IntegrityCheckLevel.None, false);

                    saveFs.CopyDirectory(outputFolder, "/", "/");
                }

                File.Delete(savePath);

                Directory.Move(tempDirectoryPath, savePath);
            }
            else
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
            }

            return new LocalFileSystem(savePath);
        }
    }
}