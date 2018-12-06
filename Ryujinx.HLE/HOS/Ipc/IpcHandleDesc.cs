using System;
using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    class IpcHandleDesc
    {
        public bool HasPId { get; private set; }

        public long PId { get; private set; }

        public int[] ToCopy { get; private set; }
        public int[] ToMove { get; private set; }

        public IpcHandleDesc(BinaryReader reader)
        {
            int word = reader.ReadInt32();

            HasPId = (word & 1) != 0;

            ToCopy = new int[(word >> 1) & 0xf];
            ToMove = new int[(word >> 5) & 0xf];

            PId = HasPId ? reader.ReadInt64() : 0;

            for (int index = 0; index < ToCopy.Length; index++)
            {
                ToCopy[index] = reader.ReadInt32();
            }

            for (int index = 0; index < ToMove.Length; index++)
            {
                ToMove[index] = reader.ReadInt32();
            }
        }

        public IpcHandleDesc(int[] copy, int[] move)
        {
            ToCopy = copy ?? throw new ArgumentNullException(nameof(copy));
            ToMove = move ?? throw new ArgumentNullException(nameof(move));
        }

        public IpcHandleDesc(int[] copy, int[] move, long pId) : this(copy, move)
        {
            PId = pId;

            HasPId = true;
        }

        public static IpcHandleDesc MakeCopy(params int[] handles)
        {
            return new IpcHandleDesc(handles, new int[0]);
        }

        public static IpcHandleDesc MakeMove(params int[] handles)
        {
            return new IpcHandleDesc(new int[0], handles);
        }

        public byte[] GetBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                int word = HasPId ? 1 : 0;

                word |= (ToCopy.Length & 0xf) << 1;
                word |= (ToMove.Length & 0xf) << 5;

                writer.Write(word);

                if (HasPId)
                {
                    writer.Write(PId);
                }

                foreach (int handle in ToCopy)
                {
                    writer.Write(handle);
                }

                foreach (int handle in ToMove)
                {
                    writer.Write(handle);
                }

                return ms.ToArray();
            }
        }
    }
}