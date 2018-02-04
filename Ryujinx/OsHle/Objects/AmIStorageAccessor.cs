using ChocolArm64.Memory;
using System;

namespace Ryujinx.OsHle.Objects
{
    class AmIStorageAccessor
    {
        public AmIStorage Storage { get; private set; }

        public AmIStorageAccessor(AmIStorage Storage)
        {
            this.Storage = Storage;
        }

        public static long GetSize(ServiceCtx Context)
        {
            AmIStorageAccessor Accessor = Context.GetObject<AmIStorageAccessor>();

            Context.ResponseData.Write((long)Accessor.Storage.Data.Length);

            return 0;
        }

        public static long Read(ServiceCtx Context)
        {
            AmIStorageAccessor Accessor = Context.GetObject<AmIStorageAccessor>();

            AmIStorage Storage = Accessor.Storage;

            long ReadPosition = Context.RequestData.ReadInt64();

            if (Context.Request.RecvListBuff.Count > 0)
            {
                long  Position = Context.Request.RecvListBuff[0].Position;
                short Size     = Context.Request.RecvListBuff[0].Size;

                byte[] Data;

                if (Storage.Data.Length > Size)
                {
                    Data = new byte[Size];
                    
                    Buffer.BlockCopy(Storage.Data, 0, Data, 0, Size);
                }
                else
                {
                    Data = Storage.Data;
                }

                AMemoryHelper.WriteBytes(Context.Memory, Position, Data);
            }

            return 0;
        }
    }
}