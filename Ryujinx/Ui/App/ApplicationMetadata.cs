namespace Ryujinx.Ui.App
{
    public class ApplicationMetadata
    {
        public bool   Favorite   { get; set; }
        public double TimePlayed { get; set; }
        public string LastPlayed { get; set; } = "Never";
    }
}