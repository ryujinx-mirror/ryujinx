using System.Collections.Generic;

namespace Ryujinx.Common.Configuration
{
    public struct DlcContainer
    {
        public string Path { get; set; }
        public List<DlcNca> DlcNcaList { get; set; }
    }
}