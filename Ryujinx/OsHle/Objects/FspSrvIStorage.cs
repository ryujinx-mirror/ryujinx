using ChocolArm64.Memory;
using Ryujinx.OsHle.Ipc;
using System.IO;

namespace Ryujinx.OsHle.Objects
{
    class FspSrvIStorage
    {
        public Stream BaseStream { get; private set; }

        public FspSrvIStorage(Stream BaseStream)
        {
            this.BaseStream = BaseStream;
        }

        public static long Read(ServiceCtx Context)
        {
            FspSrvIStorage Storage = Context.GetObject<FspSrvIStorage>();

            long Offset = Context.RequestData.ReadInt64();
            long Size   = Context.RequestData.ReadInt64();

            if (Context.Request.ReceiveBuff.Count > 0)
            {
                IpcBuffDesc BuffDesc = Context.Request.ReceiveBuff[0];

                //Use smaller length to avoid overflows.
                if (Size > BuffDesc.Size)
                {
                    Size = BuffDesc.Size;
                }

                byte[] Data = new byte[Size];

                Storage.BaseStream.Seek(Offset, SeekOrigin.Begin);
                Storage.BaseStream.Read(Data, 0, Data.Length);

                AMemoryHelper.WriteBytes(Context.Memory, BuffDesc.Position, Data);
            }

            return 0;
        }
    }
}