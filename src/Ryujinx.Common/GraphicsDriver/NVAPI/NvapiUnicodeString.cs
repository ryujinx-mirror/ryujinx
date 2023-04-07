using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Common.GraphicsDriver.NVAPI
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct NvapiUnicodeString
    {
        private fixed byte _data[4096];

        public NvapiUnicodeString(string text)
        {
            Set(text);
        }

        public string Get()
        {
            fixed (byte* data = _data)
            {
                string text = Encoding.Unicode.GetString(data, 4096);

                int index = text.IndexOf('\0');
                if (index > -1)
                {
                    text = text.Remove(index);
                }

                return text;
            }
        }

        public void Set(string text)
        {
            text += '\0';
            fixed (char* textPtr = text)
            fixed (byte* data = _data)
            {
                int written = Encoding.Unicode.GetBytes(textPtr, text.Length, data, 4096);
            }
        }
    }
}
