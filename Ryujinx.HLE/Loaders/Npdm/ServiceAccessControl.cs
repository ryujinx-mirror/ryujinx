using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class ServiceAccessControl
    {
        public IReadOnlyDictionary<string, bool> Services { get; private set; }

        public ServiceAccessControl(Stream Stream, int Offset, int Size)
        {
            Stream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(Stream);

            int ByteReaded = 0;

            Dictionary<string, bool> Services = new Dictionary<string, bool>();

            while (ByteReaded != Size)
            {
                byte ControlByte = Reader.ReadByte();

                if (ControlByte == 0)
                {
                    break;
                }

                int  Length          = (ControlByte & 0x07) + 1;
                bool RegisterAllowed = (ControlByte & 0x80) != 0;

                Services.Add(Encoding.ASCII.GetString(Reader.ReadBytes(Length), 0, Length), RegisterAllowed);

                ByteReaded += Length + 1;
            }

            this.Services = new ReadOnlyDictionary<string, bool>(Services);
        }
    }
}
