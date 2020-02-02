using Ryujinx.HLE.HOS.Services.Account.Acc;

namespace Ryujinx.HLE.FileSystem
{
    struct SaveInfo
    {
        public ulong        TitleId      { get; private set; }
        public long         SaveId       { get; private set; }
        public SaveDataType SaveDataType { get; private set; }
        public SaveSpaceId  SaveSpaceId  { get; private set; }
        public UserId       UserId       { get; private set; }

        public SaveInfo(
            ulong        titleId,
            long         saveId,
            SaveDataType saveDataType,
            SaveSpaceId  saveSpaceId,
            UserId       userId = new UserId())
        {
            TitleId      = titleId;
            SaveId       = saveId;
            SaveDataType = saveDataType;
            SaveSpaceId  = saveSpaceId;
            UserId       = userId;
        }
    }
}