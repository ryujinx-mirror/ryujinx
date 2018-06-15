using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.OsHle.Services.Aud
{
    class IAudioOutManager : IpcService
    {
        private const string DefaultAudioOutput = "DeviceOut";

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAudioOutManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ListAudioOuts     },
                { 1, OpenAudioOut      },
                { 2, ListAudioOutsAuto },
                { 3, OpenAudioOutAuto  }
            };
        }

        public long ListAudioOuts(ServiceCtx Context)
        {   
            ListAudioOutsMethod(Context, Context.Request.ReceiveBuff[0].Position, Context.Request.ReceiveBuff[0].Size);

            return 0;
        }

        public long OpenAudioOut(ServiceCtx Context)
        {
            OpenAudioOutMethod(Context, Context.Request.SendBuff[0].Position, Context.Request.SendBuff[0].Size,
                Context.Request.ReceiveBuff[0].Position, Context.Request.ReceiveBuff[0].Size);

            return 0;
        }
        
        public long ListAudioOutsAuto(ServiceCtx Context)
        { 
            ListAudioOutsMethod(Context, Context.Request.GetBufferType0x22().Position, Context.Request.GetBufferType0x22().Size);

            return 0;
        }
		
        public long OpenAudioOutAuto(ServiceCtx Context)
        {
            OpenAudioOutMethod(Context, Context.Request.GetBufferType0x21().Position, Context.Request.GetBufferType0x21().Size,
                Context.Request.GetBufferType0x22().Position, Context.Request.GetBufferType0x22().Size);

            return 0;
        }
        
        public void ListAudioOutsMethod(ServiceCtx Context, long Position, long Size)
        {
            int NameCount = 0;

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(DefaultAudioOutput + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);

                NameCount++;
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            Context.ResponseData.Write(NameCount);
        }
        
        public void OpenAudioOutMethod(ServiceCtx Context, long SendPosition, long SendSize, long ReceivePosition, long ReceiveSize)
        {
            IAalOutput AudioOut = Context.Ns.AudioOut;
                
            string DeviceName = AMemoryHelper.ReadAsciiString(
                Context.Memory,
                SendPosition,
                SendSize
            );
            
            if (DeviceName == string.Empty)
            {
                DeviceName = DefaultAudioOutput;
            }

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(DeviceName + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)ReceiveSize)
            {
                Context.Memory.WriteBytes(ReceivePosition, DeviceNameBuffer);
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {ReceiveSize} too small!");
            }       

            int SampleRate = Context.RequestData.ReadInt32();
            int Channels   = Context.RequestData.ReadInt32();

            Channels = (ushort)(Channels >> 16);

            if (SampleRate == 0)
            {
                SampleRate = 48000;
            }

            if (Channels < 1 || Channels > 2)
            {
                Channels = 2;
            }

            KEvent ReleaseEvent = new KEvent();

            ReleaseCallback Callback = () =>
            {
                ReleaseEvent.WaitEvent.Set();
            };

            int Track = AudioOut.OpenTrack(SampleRate, Channels, Callback, out AudioFormat Format);

            MakeObject(Context, new IAudioOut(AudioOut, ReleaseEvent, Track));

            Context.ResponseData.Write(SampleRate);
            Context.ResponseData.Write(Channels);
            Context.ResponseData.Write((int)Format);
            Context.ResponseData.Write((int)PlaybackState.Stopped);
        }
    }
}
