using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Renderer.Dsp.State.AuxiliaryBufferHeader;
using DspAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Server state for an auxiliary buffer effect.
    /// </summary>
    public class AuxiliaryBufferEffect : BaseEffect
    {
        /// <summary>
        /// The auxiliary buffer parameter.
        /// </summary>
        public AuxiliaryBufferParameter Parameter;

        /// <summary>
        /// Auxiliary buffer state.
        /// </summary>
        public AuxiliaryBufferAddresses State;

        public override EffectType TargetEffectType => EffectType.AuxiliaryBuffer;

        public override DspAddress GetWorkBuffer(int index)
        {
            return WorkBuffers[index].GetReference(true);
        }

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, in EffectInParameterVersion1 parameter, PoolMapper mapper)
        {
            Update(out updateErrorInfo, in parameter, mapper);
        }

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, in EffectInParameterVersion2 parameter, PoolMapper mapper)
        {
            Update(out updateErrorInfo, in parameter, mapper);
        }

        public void Update<T>(out BehaviourParameter.ErrorInfo updateErrorInfo, in T parameter, PoolMapper mapper) where T : unmanaged, IEffectInParameter
        {
            Debug.Assert(IsTypeValid(in parameter));

            UpdateParameterBase(in parameter);

            Parameter = MemoryMarshal.Cast<byte, AuxiliaryBufferParameter>(parameter.SpecificData)[0];
            IsEnabled = parameter.IsEnabled;

            updateErrorInfo = new BehaviourParameter.ErrorInfo();

            if (BufferUnmapped || parameter.IsNew)
            {
                ulong bufferSize = (ulong)Unsafe.SizeOf<int>() * Parameter.BufferStorageSize + (ulong)Unsafe.SizeOf<AuxiliaryBufferHeader>();

                bool sendBufferUnmapped = !mapper.TryAttachBuffer(out _, ref WorkBuffers[0], Parameter.SendBufferInfoAddress, bufferSize);
                bool returnBufferUnmapped = !mapper.TryAttachBuffer(out updateErrorInfo, ref WorkBuffers[1], Parameter.ReturnBufferInfoAddress, bufferSize);

                BufferUnmapped = sendBufferUnmapped && returnBufferUnmapped;

                if (!BufferUnmapped)
                {
                    DspAddress sendDspAddress = WorkBuffers[0].GetReference(false);
                    DspAddress returnDspAddress = WorkBuffers[1].GetReference(false);

                    State.SendBufferInfo = sendDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferInfo>();
                    State.SendBufferInfoBase = sendDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferHeader>();

                    State.ReturnBufferInfo = returnDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferInfo>();
                    State.ReturnBufferInfoBase = returnDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferHeader>();
                }
            }
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();
        }
    }
}
