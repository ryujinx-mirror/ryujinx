namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// The intention of the user when they finish the interaction with the keyboard.
    /// </summary>
    enum KeyboardResult
    {
        NotSet = 0,
        Accept = 1,
        Cancel = 2,
    }
}
