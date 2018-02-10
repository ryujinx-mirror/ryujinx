using ChocolArm64.Memory;
using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.OsHle.Objects.FspSrv
{
    class IStorage : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public Stream BaseStream { get; private set; }

        public IStorage(Stream BaseStream)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Read }
            };

            this.BaseStream = BaseStream;
        }

        public static long Read(ServiceCtx Context)
        {
            IStorage Storage = Context.GetObject<IStorage>();

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