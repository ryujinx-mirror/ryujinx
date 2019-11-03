using System;
using System.IO;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    static class Parcel
    {
        public static byte[] GetParcelData(byte[] parcel)
        {
            if (parcel == null)
            {
                throw new ArgumentNullException(nameof(parcel));
            }

            using (MemoryStream ms = new MemoryStream(parcel))
            {
                BinaryReader reader = new BinaryReader(ms);

                int dataSize   = reader.ReadInt32();
                int dataOffset = reader.ReadInt32();
                int objsSize   = reader.ReadInt32();
                int objsOffset = reader.ReadInt32();

                ms.Seek(dataOffset - 0x10, SeekOrigin.Current);

                return reader.ReadBytes(dataSize);
            }
        }

        public static byte[] MakeParcel(byte[] data, byte[] objs)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (objs == null)
            {
                throw new ArgumentNullException(nameof(objs));
            }

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                writer.Write(data.Length);
                writer.Write(0x10);
                writer.Write(objs.Length);
                writer.Write(data.Length + 0x10);

                writer.Write(data);
                writer.Write(objs);

                return ms.ToArray();
            }
        }
    }
}