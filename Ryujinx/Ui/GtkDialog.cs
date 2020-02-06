using Gtk;
using System.Reflection;

namespace Ryujinx.Ui
{
    internal class GtkDialog
    {
        internal static void CreateDialog(string title, string text, string secondaryText)
        {
            MessageDialog errorDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, null)
            {
                Title          = title,
                Icon           = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                Text           = text,
                SecondaryText  = secondaryText,
                WindowPosition = WindowPosition.Center
            };
            errorDialog.SetSizeRequest(100, 20);
            errorDialog.Run();
            errorDialog.Dispose();
        }

        internal static void CreateWarningDialog(string text, string secondaryText)
        {
            CreateDialog("Ryujinx - Warning", text, secondaryText);
        }

        internal static void CreateErrorDialog(string errorMessage)
        {
            CreateDialog("Ryujinx - Error", "Ryujinx has encountered an error", errorMessage);
        }
    }
}
