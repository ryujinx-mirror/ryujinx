using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Objects.Aud;
using System.Text;

using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Services
{
    static partial class Service
    {
        public static long AudOutListAudioOuts(ServiceCtx Context)
        {
            long Position = Context.Request.ReceiveBuff[0].Position;

            AMemoryHelper.WriteBytes(Context.Memory, Position, Encoding.ASCII.GetBytes("iface"));

            Context.ResponseData.Write(1);

            return 0;
        }

        public static long AudOutOpenAudioOut(ServiceCtx Context)
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

        public static long AudRenOpenAudioRenderer(ServiceCtx Context)
        {
            MakeObject(Context, new IAudioRenderer());

            return 0;
        }

        public static long AudRenGetAudioRendererWorkBufferSize(ServiceCtx Context)
        {
            int SampleRate = Context.RequestData.ReadInt32();
            int Unknown4   = Context.RequestData.ReadInt32();
            int Unknown8   = Context.RequestData.ReadInt32();
            int UnknownC   = Context.RequestData.ReadInt32();
            int Unknown10  = Context.RequestData.ReadInt32();
            int Unknown14  = Context.RequestData.ReadInt32();
            int Unknown18  = Context.RequestData.ReadInt32();
            int Unknown1c  = Context.RequestData.ReadInt32();
            int Unknown20  = Context.RequestData.ReadInt32();
            int Unknown24  = Context.RequestData.ReadInt32();
            int Unknown28  = Context.RequestData.ReadInt32();
            int Unknown2c  = Context.RequestData.ReadInt32();
            int Rev1Magic  = Context.RequestData.ReadInt32();

            Context.ResponseData.Write(0x400L);

            return 0;
        }
    }
}