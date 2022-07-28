namespace Ryujinx.Ava.Ui.Models
{
    public class DownloadableContentModel
    {
        public bool   Enabled       { get; set; }
        public string TitleId       { get; }
        public string ContainerPath { get; }
        public string FullPath      { get; }

        public DownloadableContentModel(string titleId, string containerPath, string fullPath, bool enabled)
        {
            TitleId       = titleId;
            ContainerPath = containerPath;
            FullPath      = fullPath;
            Enabled       = enabled;
        }
    }
}