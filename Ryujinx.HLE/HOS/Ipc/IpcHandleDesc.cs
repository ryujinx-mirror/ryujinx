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

        public IpcHandleDesc(BinaryReader Reader)
        {
            int Word = Reader.ReadInt32();

            HasPId = (Word & 1) != 0;

            ToCopy = new int[(Word >> 1) & 0xf];
            ToMove = new int[(Word >> 5) & 0xf];

            PId = HasPId ? Reader.ReadInt64() : 0;

            for (int Index = 0; Index < ToCopy.Length; Index++)
            {
                ToCopy[Index] = Reader.ReadInt32();
            }

            for (int Index = 0; Index < ToMove.Length; Index++)
            {
                ToMove[Index] = Reader.ReadInt32();
            }
        }

        public IpcHandleDesc(int[] Copy, int[] Move)
        {
            ToCopy = Copy ?? throw new ArgumentNullException(nameof(Copy));
            ToMove = Move ?? throw new ArgumentNullException(nameof(Move));
        }

        public IpcHandleDesc(int[] Copy, int[] Move, long PId) : this(Copy, Move)
        {
            this.PId = PId;

            HasPId = true;
        }

        public static IpcHandleDesc MakeCopy(params int[] Handles)
        {
            return new IpcHandleDesc(Handles, new int[0]);
        }

        public static IpcHandleDesc MakeMove(params int[] Handles)
        {
            return new IpcHandleDesc(new int[0], Handles);
        }

        public byte[] GetBytes()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                int Word = HasPId ? 1 : 0;

                Word |= (ToCopy.Length & 0xf) << 1;
                Word |= (ToMove.Length & 0xf) << 5;

                Writer.Write(Word);

                if (HasPId)
                {
                    Writer.Write((long)PId);
                }

                foreach (int Handle in ToCopy)
                {
                    Writer.Write(Handle);
                }

                foreach (int Handle in ToMove)
                {
                    Writer.Write(Handle);
                }

                return MS.ToArray();
            }
        }
    }
}