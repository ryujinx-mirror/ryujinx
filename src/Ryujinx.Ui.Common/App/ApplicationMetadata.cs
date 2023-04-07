namespace Ryujinx.Ui.App.Common
{
    public class ApplicationMetadata
    {
        public string Title { get; set; }
        public bool   Favorite   { get; set; }
        public double TimePlayed { get; set; }
        public string LastPlayed { get; set; } = "Never";
    }
}