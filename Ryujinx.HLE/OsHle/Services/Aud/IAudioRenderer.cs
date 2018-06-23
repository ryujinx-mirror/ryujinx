using ChocolArm64.Memory;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.OsHle.Services.Aud
{
    class IAudioRenderer : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent UpdateEvent;

        private AudioRendererParameter Params;

        public IAudioRenderer(AudioRendererParameter Params)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 4, RequestUpdateAudioRenderer },
                { 5, StartAudioRenderer         },
                { 6, StopAudioRenderer          },
                { 7, QuerySystemEvent           }
            };

            UpdateEvent = new KEvent();

            this.Params = Params;
        }

        public long RequestUpdateAudioRenderer(ServiceCtx Context)
        {
            long OutputPosition = Context.Request.ReceiveBuff[0].Position;

            long InputPosition = Context.Request.SendBuff[0].Position;

            UpdateDataHeader InputDataHeader = AMemoryHelper.Read<UpdateDataHeader>(Context.Memory, InputPosition);

            int MemoryPoolOffset = Marshal.SizeOf(InputDataHeader) + InputDataHeader.BehaviorSize;

            UpdateDataHeader OutputDataHeader = new UpdateDataHeader();

            OutputDataHeader.Revision               = Params.Revision;
            OutputDataHeader.BehaviorSize           = 0xb0;
            OutputDataHeader.MemoryPoolsSize        = (Params.EffectCount + (Params.VoiceCount * 4)) * 0x10;
            OutputDataHeader.VoicesSize             = Params.VoiceCount  * 0x10;
            OutputDataHeader.EffectsSize            = Params.EffectCount * 0x10;
            OutputDataHeader.SinksSize              = Params.SinkCount   * 0x20;
            OutputDataHeader.PerformanceManagerSize = 0x10;
            OutputDataHeader.TotalSize              = Marshal.SizeOf(OutputDataHeader) + OutputDataHeader.BehaviorSize + OutputDataHeader.MemoryPoolsSize +
                OutputDataHeader.VoicesSize + OutputDataHeader.EffectsSize + OutputDataHeader.SinksSize + OutputDataHeader.PerformanceManagerSize;

            AMemoryHelper.Write(Context.Memory, OutputPosition, OutputDataHeader);

            for (int Offset = 0x40; Offset < 0x40 + OutputDataHeader.MemoryPoolsSize; Offset += 0x10, MemoryPoolOffset += 0x20)
            {
                MemoryPoolStates PoolState = (MemoryPoolStates) Context.Memory.ReadInt32(InputPosition + MemoryPoolOffset + 0x10);

                if (PoolState == MemoryPoolStates.RequestAttach)
                {
                    Context.Memory.WriteInt32(OutputPosition + Offset, (int)MemoryPoolStates.Attached);
                }
                else if (PoolState == MemoryPoolStates.RequestDetach)
                {
                    Context.Memory.WriteInt32(OutputPosition + Offset, (int)MemoryPoolStates.Detached);
                }
                else
                {
                    Context.Memory.WriteInt32(OutputPosition + Offset, (int)PoolState);
                }
            }

            //TODO: We shouldn't be signaling this here.
            UpdateEvent.WaitEvent.Set();

            return 0;
        }

        public long StartAudioRenderer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long StopAudioRenderer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long QuerySystemEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(UpdateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                UpdateEvent.Dispose();
            }
        }
    }
}
 