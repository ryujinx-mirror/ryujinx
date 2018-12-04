using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class ServiceAccessControl
    {
        public IReadOnlyDictionary<string, bool> Services { get; }

        public ServiceAccessControl(Stream stream, int offset, int size)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            BinaryReader reader = new BinaryReader(stream);

            int byteReaded = 0;

            Dictionary<string, bool> services = new Dictionary<string, bool>();

            while (byteReaded != size)
            {
                byte controlByte = reader.ReadByte();

                if (controlByte == 0)
                {
                    break;
                }

                int  length          = (controlByte & 0x07) + 1;
                bool registerAllowed = (controlByte & 0x80) != 0;

                services.Add(Encoding.ASCII.GetString(reader.ReadBytes(length), 0, length), registerAllowed);

                byteReaded += length + 1;
            }

            Services = new ReadOnlyDictionary<string, bool>(services);
        }
    }
}
