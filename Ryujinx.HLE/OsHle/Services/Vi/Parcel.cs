using System;
using System.IO;

namespace Ryujinx.HLE.OsHle.Services.Android
{
    static class Parcel
    {
        public static byte[] GetParcelData(byte[] Parcel)
        {
            if (Parcel == null)
            {
                throw new ArgumentNullException(nameof(Parcel));
            }

            using (MemoryStream MS = new MemoryStream(Parcel))
            {
                BinaryReader Reader = new BinaryReader(MS);

                int DataSize   = Reader.ReadInt32();
                int DataOffset = Reader.ReadInt32();
                int ObjsSize   = Reader.ReadInt32();
                int ObjsOffset = Reader.ReadInt32();

                MS.Seek(DataOffset - 0x10, SeekOrigin.Current);

                return Reader.ReadBytes(DataSize);
            }
        }

        public static byte[] MakeParcel(byte[] Data, byte[] Objs)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }

            if (Objs == null)
            {
                throw new ArgumentNullException(nameof(Objs));
            }

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write(Data.Length);
                Writer.Write(0x10);
                Writer.Write(Objs.Length);
                Writer.Write(Data.Length + 0x10);

                Writer.Write(Data);
                Writer.Write(Objs);

                return MS.ToArray();
            }
        }
    }
}