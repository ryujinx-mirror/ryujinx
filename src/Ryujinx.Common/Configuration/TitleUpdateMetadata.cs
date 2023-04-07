using System.Collections.Generic;

namespace Ryujinx.Common.Configuration
{
    public struct TitleUpdateMetadata
    {
        public string       Selected { get; set; }
        public List<string> Paths    { get; set; }
    }
}