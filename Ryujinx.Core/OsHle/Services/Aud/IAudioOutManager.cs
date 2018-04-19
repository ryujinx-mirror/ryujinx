using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Core.OsHle.Services.Aud
{
    class IAudioOutManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAudioOutManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ListAudioOuts },
                { 1, OpenAudioOut  }
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
            IAalOutput AudioOut = Context.Ns.AudioOut;

            string DeviceName = AMemoryHelper.ReadAsciiString(
                Context.Memory,
                Context.Request.SendBuff[0].Position,
                Context.Request.SendBuff[0].Size);

            if (DeviceName == string.Empty)
            {
                DeviceName = "FIXME";
            }

            long DeviceNamePosition = Context.Request.ReceiveBuff[0].Position;
            long DeviceNameSize     = Context.Request.ReceiveBuff[0].Size;

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(DeviceName);

            if (DeviceName.Length <= DeviceNameSize)
            {
                AMemoryHelper.WriteBytes(Context.Memory, DeviceNamePosition, DeviceNameBuffer);
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

            return 0;
        }
    }
}