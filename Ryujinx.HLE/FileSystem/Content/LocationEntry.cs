using System;
using System.Collections.Generic;
using System.Text;
using LibHac;

namespace Ryujinx.HLE.FileSystem.Content
{
    public struct LocationEntry
    {
        public string      ContentPath { get; private set; }
        public int         Flag        { get; private set; }
        public long        TitleId     { get; private set; }
        public ContentType ContentType { get; private set; }

        public LocationEntry(string ContentPath, int Flag, long TitleId, ContentType ContentType)
        {
            this.ContentPath = ContentPath;
            this.Flag        = Flag;
            this.TitleId     = TitleId;
            this.ContentType = ContentType;
        }

        public void SetFlag(int Flag)
        {
            this.Flag = Flag;
        }
    }
}
