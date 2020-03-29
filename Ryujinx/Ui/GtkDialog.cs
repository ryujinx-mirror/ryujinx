using Gtk;
using System.Reflection;

namespace Ryujinx.Ui
{
    internal class GtkDialog
    {
        internal static bool _isExitDialogOpen = false;

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

        internal static bool CreateExitDialog()
        {
            if (_isExitDialogOpen)
            {
                return false;
            }

            _isExitDialogOpen = true;

            MessageDialog messageDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.OkCancel, null)
            {
                Title = "Ryujinx - Exit",
                Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                Text = "Are you sure you want to stop emulation?",
                SecondaryText = "All unsaved data will be lost",
                WindowPosition = WindowPosition.Center
            };

            messageDialog.SetSizeRequest(100, 20);
            ResponseType res = (ResponseType)messageDialog.Run();
            messageDialog.Dispose();
            _isExitDialogOpen = false;
            
            if (res == ResponseType.Ok)
            {
                return true;
            } 
            
            return false;
        }
    }
}
