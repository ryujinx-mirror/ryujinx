using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class IApplicationFunctions : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IApplicationFunctions()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  1, PopLaunchParameter },
                { 20, EnsureSaveData     },
                { 21, GetDesiredLanguage },
                { 22, SetTerminateResult },
                { 40, NotifyRunning      }
            };
        }

        private const uint LaunchParamsMagic = 0xc79497ca;

        public long PopLaunchParameter(ServiceCtx Context)
        {
            //Only the first 0x18 bytes of the Data seems to be actually used.
            MakeObject(Context, new IStorage(MakeLaunchParams()));

            return 0;
        }

        public long EnsureSaveData(ServiceCtx Context)
        {
            long UIdLow  = Context.RequestData.ReadInt64();
            long UIdHigh = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(0L);

            return 0;
        }

        public long GetDesiredLanguage(ServiceCtx Context)
        {
            //This is an enumerator where each number is a differnet language.
            //0 is Japanese and 1 is English, need to figure out the other codes.
            Context.ResponseData.Write(1L);

            return 0;
        }

        public long SetTerminateResult(ServiceCtx Context)
        {
            int ErrorCode = Context.RequestData.ReadInt32();

            int Module = ErrorCode & 0xFF;
            int Description = (ErrorCode >> 9) & 0xFFF;

            Logging.Info($"({(ErrorModule)Module}){2000 + Module}-{Description}");

            return 0;
        }

        public long NotifyRunning(ServiceCtx Context)
        {
            Context.ResponseData.Write(1);

            return 0;
        }

        private byte[] MakeLaunchParams()
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