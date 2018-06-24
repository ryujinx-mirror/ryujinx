using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class ServiceAccessControl
    {
        public List<(string, bool)> Services = new List<(string, bool)>();

        public ServiceAccessControl(Stream ServiceAccessControlStream, int Offset, int Size)
        {
            ServiceAccessControlStream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(ServiceAccessControlStream);

            int ByteReaded = 0;

            while (ByteReaded != Size)
            {
                byte ControlByte = Reader.ReadByte();

                if (ControlByte == 0x00) break;

                int Length             = ((ControlByte & 0x07)) + 1;
                bool RegisterAllowed   = ((ControlByte & 0x80) != 0);

                Services.Add((Encoding.ASCII.GetString(Reader.ReadBytes(Length), 0, Length), RegisterAllowed));

                ByteReaded += Length + 1;
            }
        }
    }
}
