using Ryujinx.HLE.Utilities;

namespace Ryujinx.HLE.FileSystem
{
    struct SaveInfo
    {
        public long    TitleId { get; }
        public long    SaveId  { get; }
        public UInt128 UserId  { get; }

        public SaveDataType SaveDataType { get; }
        public SaveSpaceId  SaveSpaceId  { get; }

        public SaveInfo(
            long         titleId,
            long         saveId,
            SaveDataType saveDataType,
            UInt128      userId,
            SaveSpaceId  saveSpaceId)
        {
            TitleId      = titleId;
            UserId       = userId;
            SaveId       = saveId;
            SaveDataType = saveDataType;
            SaveSpaceId  = saveSpaceId;
        }
    }
}
