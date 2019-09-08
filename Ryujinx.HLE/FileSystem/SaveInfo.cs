using Ryujinx.HLE.Utilities;

namespace Ryujinx.HLE.FileSystem
{
    struct SaveInfo
    {
        public ulong        TitleId      { get; private set; }
        public long         SaveId       { get; private set; }
        public SaveDataType SaveDataType { get; private set; }
        public SaveSpaceId  SaveSpaceId  { get; private set; }
        public UInt128      UserId       { get; private set; }

        public SaveInfo(
            ulong        titleId,
            long         saveId,
            SaveDataType saveDataType,
            SaveSpaceId  saveSpaceId,
            UInt128      userId = new UInt128())
        {
            TitleId      = titleId;
            SaveId       = saveId;
            SaveDataType = saveDataType;
            SaveSpaceId  = saveSpaceId;
            UserId       = userId;
        }
    }
}