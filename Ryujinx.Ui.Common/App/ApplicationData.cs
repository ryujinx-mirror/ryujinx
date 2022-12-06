using LibHac.Common;
using LibHac.Ns;

namespace Ryujinx.Ui.App.Common
{
    public class ApplicationData
    {
        public bool   Favorite      { get; set; }
        public byte[] Icon          { get; set; }
        public string TitleName     { get; set; }
        public string TitleId       { get; set; }
        public string Developer     { get; set; }
        public string Version       { get; set; }
        public string TimePlayed    { get; set; }
        public double TimePlayedNum { get; set; }
        public string LastPlayed    { get; set; }
        public string FileExtension { get; set; }
        public string FileSize      { get; set; }
        public double FileSizeBytes { get; set; }
        public string Path          { get; set; }
        public BlitStruct<ApplicationControlProperty> ControlHolder { get; set; }
    }
}