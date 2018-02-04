using System.IO;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Objects
{
    class AmIApplicationFunctions
    {
        private const uint LaunchParamsMagic = 0xc79497ca;

        public static long PopLaunchParameter(ServiceCtx Context)
        {
            //Only the first 0x18 bytes of the Data seems to be actually used.
            MakeObject(Context, new AmIStorage(MakeLaunchParams()));

            return 0;
        }

        public static long EnsureSaveData(ServiceCtx Context)
        {
            long UIdLow  = Context.RequestData.ReadInt64();
            long UIdHigh = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(0L);

            return 0;
        }

        public static long GetDesiredLanguage(ServiceCtx Context)
        {
            //This is an enumerator where each number is a differnet language.
            //0 is Japanese and 1 is English, need to figure out the other codes.
            Context.ResponseData.Write(1L);

            return 0;
        }

        private static byte[] MakeLaunchParams()
        {
            //Size needs to be at least 0x88 bytes otherwise application errors.
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                MS.SetLength(0x88);

                Writer.Write(LaunchParamsMagic);
                Writer.Write(1);  //IsAccountSelected? Only lower 8 bits actually used.
                Writer.Write(1L); //User Id Low (note: User Id needs to be != 0)
                Writer.Write(0L); //User Id High

                return MS.ToArray();
            }
        }
    }
}