using System;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Identifies prohibited character sets.
    /// </summary>
    [Flags]
    enum InvalidCharFlags : uint
    {
        /// <summary>
        /// No characters are prohibited.
        /// </summary>
        None = 0 << 1,

        /// <summary>
        /// Prohibits spaces.
        /// </summary>
        Space = 1 << 1,

        /// <summary>
        /// Prohibits the at (@) symbol.
        /// </summary>
        AtSymbol = 1 << 2,

        /// <summary>
        /// Prohibits the percent (%) symbol.
        /// </summary>
        Percent = 1 << 3,

        /// <summary>
        /// Prohibits the forward slash (/) symbol.
        /// </summary>
        ForwardSlash = 1 << 4,

        /// <summary>
        /// Prohibits the backward slash (\) symbol.
        /// </summary>
        BackSlash = 1 << 5,

        /// <summary>
        /// Prohibits numbers.
        /// </summary>
        Numbers = 1 << 6,

        /// <summary>
        /// Prohibits characters outside of those allowed in download codes.
        /// </summary>
        DownloadCode = 1 << 7,

        /// <summary>
        /// Prohibits characters outside of those allowed in Mii Nicknames.
        /// </summary>
        Username = 1 << 8,
    }
}
