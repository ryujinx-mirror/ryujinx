using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Objects
{
    class AmIStorage
    {
        public byte[] Data { get; private set; }

        public AmIStorage(byte[] Data)
        {
            this.Data = Data;
        }

        public static long Open(ServiceCtx Context)
        {
            AmIStorage Storage = Context.GetObject<AmIStorage>();

            MakeObject(Context, new AmIStorageAccessor(Storage));

            return 0;
        }
    }
}