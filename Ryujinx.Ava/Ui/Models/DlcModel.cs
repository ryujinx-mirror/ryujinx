namespace Ryujinx.Ava.Ui.Models
{
    public class DlcModel
    {
        public bool IsEnabled { get; set; }
        public string TitleId { get; }
        public string ContainerPath { get; }
        public string FullPath { get; }

        public DlcModel(string titleId, string containerPath, string fullPath, bool isEnabled)
        {
            TitleId = titleId;
            ContainerPath = containerPath;
            FullPath = fullPath;
            IsEnabled = isEnabled;
        }
    }
}