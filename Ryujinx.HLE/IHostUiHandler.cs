using Ryujinx.HLE.HOS.Applets;

namespace Ryujinx.HLE
{
    public interface IHostUiHandler
    {
        /// <summary>
        /// Displays an Input Dialog box to the user and blocks until text is entered.
        /// </summary>
        /// <param name="userText">Text that the user entered. Set to `null` on internal errors</param>
        /// <returns>True when OK is pressed, False otherwise. Also returns True on internal errors</returns>
        bool DisplayInputDialog(SoftwareKeyboardUiArgs args, out string userText);
    }
}