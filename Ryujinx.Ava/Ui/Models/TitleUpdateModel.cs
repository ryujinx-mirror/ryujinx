using LibHac.Ns;
using Ryujinx.Ava.Common.Locale;

namespace Ryujinx.Ava.Ui.Models
{
    internal class TitleUpdateModel
    {
        public bool IsEnabled { get; set; }
        public bool IsNoUpdate { get; }
        public ApplicationControlProperty Control { get; }
        public string Path { get; }

        public string Label => IsNoUpdate
            ? LocaleManager.Instance["NoUpdate"]
            : string.Format(LocaleManager.Instance["TitleUpdateVersionLabel"], Control.DisplayVersionString.ToString(),
                Path);

        public TitleUpdateModel(ApplicationControlProperty control, string path, bool isNoUpdate = false)
        {
            Control = control;
            Path = path;
            IsNoUpdate = isNoUpdate;
        }
    }
}