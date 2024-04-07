using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DspAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Server state for a delay effect.
    /// </summary>
    public class DelayEffect : BaseEffect
    {
        /// <summary>
        /// The delay parameter.
        /// </summary>
        public DelayParameter Parameter;

        /// <summary>
        /// The delay state.
        /// </summary>
        public Memory<DelayState> State { get; }

        public DelayEffect()
        {
            State = new DelayState[1];
        }

        public override EffectType TargetEffectType => EffectType.Delay;

        public override DspAddress GetWorkBuffer(int index)
        {
            return GetSingleBuffer();
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

            ref DelayParameter delayParameter = ref MemoryMarshal.Cast<byte, DelayParameter>(parameter.SpecificData)[0];

            updateErrorInfo = new BehaviourParameter.ErrorInfo();

            if (delayParameter.IsChannelCountMaxValid())
            {
                UpdateParameterBase(in parameter);

                UsageState oldParameterStatus = Parameter.Status;

                Parameter = delayParameter;

                if (delayParameter.IsChannelCountValid())
                {
                    IsEnabled = parameter.IsEnabled;

                    if (oldParameterStatus != UsageState.Enabled)
                    {
                        Parameter.Status = oldParameterStatus;
                    }

                    if (BufferUnmapped || parameter.IsNew)
                    {
                        UsageState = UsageState.New;
                        Parameter.Status = UsageState.Invalid;

                        BufferUnmapped = !mapper.TryAttachBuffer(out updateErrorInfo, ref WorkBuffers[0], parameter.BufferBase, parameter.BufferSize);
                    }
                }
            }
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();

            Parameter.Status = UsageState.Enabled;
        }
    }
}
