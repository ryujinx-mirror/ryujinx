using Gtk;
using System.Reflection;

namespace Ryujinx.Ui
{
    internal class ErrorAppletDialog : MessageDialog
    {
        internal static bool _isExitDialogOpen = false;

        public ErrorAppletDialog(Window parentWindow, DialogFlags dialogFlags, MessageType messageType, string[] buttons) : base(parentWindow, dialogFlags, messageType, ButtonsType.None, null)
        {
            Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");

            int responseId = 0;

            if (buttons != null)
            {
                foreach (string buttonText in buttons)
                {
                    AddButton(buttonText, responseId);
                    responseId++;
                }
            }
            else
            {
                AddButton("OK", 0);
            }
            
            ShowAll();
        }
    }
}