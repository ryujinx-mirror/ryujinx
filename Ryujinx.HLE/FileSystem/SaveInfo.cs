using Ryujinx.HLE.Utilities;

namespace Ryujinx.HLE.FileSystem
{
    struct SaveInfo
    {
        public long    TitleId { get; private set; }
        public long    SaveId  { get; private set; }
        public UInt128 UserId  { get; private set; }

        public SaveDataType SaveDataType { get; private set; }
        public SaveSpaceId  SaveSpaceId  { get; private set; }

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
