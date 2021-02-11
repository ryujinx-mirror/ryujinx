using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure with custom dictionary words for the software keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct SoftwareKeyboardDictSet
    {
        /// <summary>
        /// A 0x1000-byte aligned buffer position.
        /// </summary>
        public ulong BufferPosition;

        /// <summary>
        /// A 0x1000-byte aligned buffer size.
        /// </summary>
        public uint BufferSize;

        /// <summary>
        /// Array of word entries in the buffer.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public ulong[] Entries;

        /// <summary>
        /// Number of used entries in the Entries field.
        /// </summary>
        public ushort TotalEntries;

        public ushort Padding1;
    }
}
