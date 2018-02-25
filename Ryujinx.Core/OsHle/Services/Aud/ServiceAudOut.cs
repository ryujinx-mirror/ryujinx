using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

using static Ryujinx.Core.OsHle.IpcServices.ObjHelper;

namespace Ryujinx.Core.OsHle.IpcServices.Aud
{
    class ServiceAudOut : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceAudOut()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ListAudioOuts },
                { 1, OpenAudioOut  },
            };
        }

        public long ListAudioOuts(ServiceCtx Context)
        {
            long Position = Context.Request.ReceiveBuff[0].Position;

            AMemoryHelper.WriteBytes(Context.Memory, Position, Encoding.ASCII.GetBytes("iface"));

            Context.ResponseData.Write(1);

            return 0;
        }

        public long OpenAudioOut(ServiceCtx Context)
        {
            MakeObject(Context, new IAudioOut());

            Context.ResponseData.Write(48000); //Sample Rate
            Context.ResponseData.Write(2); //Channel Count
            Context.ResponseData.Write(2); //PCM Format
            /*  
                0 - Invalid
                1 - INT8
                2 - INT16
                3 - INT24
                4 - INT32
                5 - PCM Float
                6 - ADPCM
            */
            Context.ResponseData.Write(0); //Unknown

            return 0;
        }
    }
}