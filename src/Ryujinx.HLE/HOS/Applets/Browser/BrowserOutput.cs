using Ryujinx.Common;
using System;
using System.IO;

namespace Ryujinx.HLE.HOS.Applets.Browser
{
    class BrowserOutput
    {
        public BrowserOutputType Type { get; }
        public byte[] Value { get; }

        public BrowserOutput(BrowserOutputType type, byte[] value)
        {
            Type  = type;
            Value = value;
        }

        public BrowserOutput(BrowserOutputType type, uint value)
        {
            Type  = type;
            Value = BitConverter.GetBytes(value); 
        }

        public BrowserOutput(BrowserOutputType type, ulong value)
        {
            Type  = type;
            Value = BitConverter.GetBytes(value);
        }

        public BrowserOutput(BrowserOutputType type, bool value)
        {
            Type  = type;
            Value = BitConverter.GetBytes(value);
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteStruct(new WebArgTLV
            {
                Type = (ushort)Type,
                Size = (ushort)Value.Length
            });

            writer.Write(Value);
        }
    }
}
