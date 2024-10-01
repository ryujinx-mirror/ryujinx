namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// The miniaturization mode used by the keyboard in inline mode.
    /// </summary>
    enum KeyboardMiniaturizationMode : byte
    {
        None = 0,
        Auto = 1,
        Forced = 2,
    }
}
