using Gtk;

namespace Ryujinx.Ui.Widgets
{
    public class GtkInputDialog : MessageDialog
    {
        public Entry InputEntry { get; }

        public GtkInputDialog(Window parent, string title, string mainText, uint inputMax) : base(parent, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.OkCancel, null)
        {
            SetDefaultSize(300, 0);

            Title = title;

            Label mainTextLabel = new Label
            {
                Text = mainText
            };

            InputEntry = new Entry
            {
                MaxLength = (int)inputMax
            };

            Label inputMaxTextLabel = new Label
            {
                Text = $"(Max length: {inputMax})"
            };

            ((Box)MessageArea).PackStart(mainTextLabel,     true, true, 0);
            ((Box)MessageArea).PackStart(InputEntry,        true, true, 5);
            ((Box)MessageArea).PackStart(inputMaxTextLabel, true, true, 0);

            ShowAll();
        }
    }
}