using ChocolArm64.Memory;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        private const int LangCodesCount = 13;

        public static long SetGetAvailableLanguageCodes(ServiceCtx Context)
        {
            int PtrBuffSize = Context.RequestData.ReadInt32();

            if (Context.Request.RecvListBuff.Count > 0)
            {
                long  Position = Context.Request.RecvListBuff[0].Position;
                short Size     = Context.Request.RecvListBuff[0].Size;

                //This should return an array of ints with values matching the LanguageCode enum.
                byte[] Data = new byte[Size];

                Data[0] = 0;
                Data[1] = 1;

                AMemoryHelper.WriteBytes(Context.Memory, Position, Data);
            }

            Context.ResponseData.Write(LangCodesCount);

            return 0;
        }
    }
}