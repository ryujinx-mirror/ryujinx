using LibHac;

namespace Ryujinx.HLE.FileSystem.Content
{
    public struct LocationEntry
    {
        public string      ContentPath { get; }
        public int         Flag        { get; private set; }
        public long        TitleId     { get; }
        public ContentType ContentType { get; }

        public LocationEntry(string contentPath, int flag, long titleId, ContentType contentType)
        {
            ContentPath = contentPath;
            Flag        = flag;
            TitleId     = titleId;
            ContentType = contentType;
        }

        public void SetFlag(int flag)
        {
            Flag = flag;
        }
    }
}
