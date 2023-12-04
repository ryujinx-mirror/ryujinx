using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    internal class InlineResponses
    {
        private const uint MaxStrLenUTF8 = 0x7D4;
        private const uint MaxStrLenUTF16 = 0x3EC;

        private static void BeginResponse(InlineKeyboardState state, InlineKeyboardResponse resCode, BinaryWriter writer)
        {
            writer.Write((uint)state);
            writer.Write((uint)resCode);
        }

        private static uint WriteString(string text, BinaryWriter writer, uint maxSize, Encoding encoding)
        {
            // Ensure the text fits in the buffer, but do not straight cut the bytes because
            // this may corrupt the encoding. Search for a cut in the source string that fits.

            byte[] bytes = null;

            for (int maxStr = text.Length; maxStr >= 0; maxStr--)
            {
                // This loop will probably will run only once.
                bytes = encoding.GetBytes(text, 0, maxStr);
                if (bytes.Length <= maxSize)
                {
                    break;
                }
            }

            writer.Write(bytes);
            writer.Seek((int)maxSize - bytes.Length, SeekOrigin.Current);
            writer.Write((uint)text.Length); // String size

            return (uint)text.Length; // Return the cursor position at the end of the text
        }

        private static void WriteStringWithCursor(string text, uint cursor, BinaryWriter writer, uint maxSize, Encoding encoding, bool padMiddle)
        {
            uint length = WriteString(text, writer, maxSize, encoding);

            if (cursor > length)
            {
                cursor = length;
            }

            if (padMiddle)
            {
                writer.Write((int)-1); // ?
                writer.Write((int)-1); // ?
            }

            writer.Write(cursor); // Cursor position
        }

        public static byte[] FinishedInitialize(InlineKeyboardState state)
        {
            uint resSize = 2 * sizeof(uint) + 0x1;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.FinishedInitialize, writer);
            writer.Write((byte)1); // Data (ignored by the program)

            return stream.ToArray();
        }

        public static byte[] Default(InlineKeyboardState state)
        {
            uint resSize = 2 * sizeof(uint);

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.Default, writer);

            return stream.ToArray();
        }

        public static byte[] ChangedString(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 6 * sizeof(uint) + MaxStrLenUTF16;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.ChangedString, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF16, Encoding.Unicode, true);

            return stream.ToArray();
        }

        public static byte[] MovedCursor(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 4 * sizeof(uint) + MaxStrLenUTF16;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.MovedCursor, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF16, Encoding.Unicode, false);

            return stream.ToArray();
        }

        public static byte[] MovedTab(string text, uint cursor, InlineKeyboardState state)
        {
            // Should be the same as MovedCursor.

            uint resSize = 4 * sizeof(uint) + MaxStrLenUTF16;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.MovedTab, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF16, Encoding.Unicode, false);

            return stream.ToArray();
        }

        public static byte[] DecidedEnter(string text, InlineKeyboardState state)
        {
            uint resSize = 3 * sizeof(uint) + MaxStrLenUTF16;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.DecidedEnter, writer);
            WriteString(text, writer, MaxStrLenUTF16, Encoding.Unicode);

            return stream.ToArray();
        }

        public static byte[] DecidedCancel(InlineKeyboardState state)
        {
            uint resSize = 2 * sizeof(uint);

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.DecidedCancel, writer);

            return stream.ToArray();
        }

        public static byte[] ChangedStringUtf8(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 6 * sizeof(uint) + MaxStrLenUTF8;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.ChangedStringUtf8, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF8, Encoding.UTF8, true);

            return stream.ToArray();
        }

        public static byte[] MovedCursorUtf8(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 4 * sizeof(uint) + MaxStrLenUTF8;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.MovedCursorUtf8, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF8, Encoding.UTF8, false);

            return stream.ToArray();
        }

        public static byte[] DecidedEnterUtf8(string text, InlineKeyboardState state)
        {
            uint resSize = 3 * sizeof(uint) + MaxStrLenUTF8;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.DecidedEnterUtf8, writer);
            WriteString(text, writer, MaxStrLenUTF8, Encoding.UTF8);

            return stream.ToArray();
        }

        public static byte[] UnsetCustomizeDic(InlineKeyboardState state)
        {
            uint resSize = 2 * sizeof(uint);

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.UnsetCustomizeDic, writer);

            return stream.ToArray();
        }

        public static byte[] ReleasedUserWordInfo(InlineKeyboardState state)
        {
            uint resSize = 2 * sizeof(uint);

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.ReleasedUserWordInfo, writer);

            return stream.ToArray();
        }

        public static byte[] UnsetCustomizedDictionaries(InlineKeyboardState state)
        {
            uint resSize = 2 * sizeof(uint);

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.UnsetCustomizedDictionaries, writer);

            return stream.ToArray();
        }

        public static byte[] ChangedStringV2(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 6 * sizeof(uint) + MaxStrLenUTF16 + 0x1;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.ChangedStringV2, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF16, Encoding.Unicode, true);
            writer.Write((byte)0); // Flag == 0

            return stream.ToArray();
        }

        public static byte[] MovedCursorV2(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 4 * sizeof(uint) + MaxStrLenUTF16 + 0x1;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.MovedCursorV2, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF16, Encoding.Unicode, false);
            writer.Write((byte)0); // Flag == 0

            return stream.ToArray();
        }

        public static byte[] ChangedStringUtf8V2(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 6 * sizeof(uint) + MaxStrLenUTF8 + 0x1;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.ChangedStringUtf8V2, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF8, Encoding.UTF8, true);
            writer.Write((byte)0); // Flag == 0

            return stream.ToArray();
        }

        public static byte[] MovedCursorUtf8V2(string text, uint cursor, InlineKeyboardState state)
        {
            uint resSize = 4 * sizeof(uint) + MaxStrLenUTF8 + 0x1;

            using MemoryStream stream = new(new byte[resSize]);
            using BinaryWriter writer = new(stream);

            BeginResponse(state, InlineKeyboardResponse.MovedCursorUtf8V2, writer);
            WriteStringWithCursor(text, cursor, writer, MaxStrLenUTF8, Encoding.UTF8, false);
            writer.Write((byte)0); // Flag == 0

            return stream.ToArray();
        }
    }
}
