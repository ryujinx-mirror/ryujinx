using Ryujinx.HLE.Utilities;

namespace Ryujinx.HLE.FileSystem
{
    struct SaveInfo
    {
        public long   TitleId { get; private set; }
        public long   SaveId  { get; private set; }
        public UInt128 UserId  { get; private set; }

        public SaveDataType SaveDataType { get; private set; }
        public SaveSpaceId  SaveSpaceId  { get; private set; }

        public SaveInfo(
            long         TitleId,
            long         SaveId,
            SaveDataType SaveDataType,
            UInt128       UserId,
            SaveSpaceId  SaveSpaceId)
        {
            this.TitleId      = TitleId;
            this.UserId       = UserId;
            this.SaveId       = SaveId;
            this.SaveDataType = SaveDataType;
            this.SaveSpaceId  = SaveSpaceId;
        }
    }
}
