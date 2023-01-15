using LibHac.Ns;
using Ryujinx.Ava.Common.Locale;

namespace Ryujinx.Ava.UI.Models
{
    public class TitleUpdateModel
    {
        public ApplicationControlProperty Control { get; }
        public string Path { get; }

        public string Label => string.Format(LocaleManager.Instance[LocaleKeys.TitleUpdateVersionLabel], Control.DisplayVersionString.ToString());

        public TitleUpdateModel(ApplicationControlProperty control, string path)
        {
            Control = control;
            Path = path;
        }
    }
}