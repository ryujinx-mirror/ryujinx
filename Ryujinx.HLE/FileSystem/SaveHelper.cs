using Ryujinx.HLE.HOS;
using System.IO;

using static Ryujinx.HLE.FileSystem.VirtualFileSystem;

namespace Ryujinx.HLE.FileSystem
{
    static class SaveHelper
    {
        public static string GetSavePath(SaveInfo saveMetaData, ServiceCtx context)
        {
            string baseSavePath   = NandPath;
            ulong  currentTitleId = saveMetaData.TitleId;

            switch (saveMetaData.SaveSpaceId)
            {
                case SaveSpaceId.NandUser:
                    baseSavePath = UserNandPath;
                    break;
                case SaveSpaceId.NandSystem:
                    baseSavePath = SystemNandPath;
                    break;
                case SaveSpaceId.SdCard:
                    baseSavePath = Path.Combine(SdCardPath, "Nintendo");
                    break;
            }

            baseSavePath = Path.Combine(baseSavePath, "save");

            if (saveMetaData.TitleId == 0 && saveMetaData.SaveDataType == SaveDataType.SaveData)
            {
                currentTitleId = context.Process.TitleId;
            }

            string saveAccount = saveMetaData.UserId.IsNull ? "savecommon" : saveMetaData.UserId.ToString();

            string savePath = Path.Combine(baseSavePath,
                saveMetaData.SaveId.ToString("x16"),
                saveAccount,
                saveMetaData.SaveDataType == SaveDataType.SaveData ? currentTitleId.ToString("x16") : string.Empty);

            return savePath;
        }
    }
}
