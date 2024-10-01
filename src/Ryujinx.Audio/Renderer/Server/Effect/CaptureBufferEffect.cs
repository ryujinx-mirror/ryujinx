using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DspAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Server state for an capture buffer effect.
    /// </summary>
    public class CaptureBufferEffect : BaseEffect
    {
        /// <summary>
        /// The capture buffer parameter.
        /// </summary>
        public AuxiliaryBufferParameter Parameter;

        /// <summary>
        /// Capture buffer state.
        /// </summary>
        public AuxiliaryBufferAddresses State;

        public override EffectType TargetEffectType => EffectType.CaptureBuffer;

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

                bool sendBufferUnmapped = !mapper.TryAttachBuffer(out updateErrorInfo, ref WorkBuffers[0], Parameter.SendBufferInfoAddress, bufferSize);

                BufferUnmapped = sendBufferUnmapped;

                if (!BufferUnmapped)
                {
                    DspAddress sendDspAddress = WorkBuffers[0].GetReference(false);

                    // NOTE: Nintendo directly interact with the CPU side structure in the processing of the DSP command.
                    State.SendBufferInfo = sendDspAddress;
                    State.SendBufferInfoBase = sendDspAddress + (ulong)Unsafe.SizeOf<AuxiliaryBufferHeader>();
                    State.ReturnBufferInfo = 0;
                    State.ReturnBufferInfoBase = 0;
                }
            }
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();
        }
    }
}
