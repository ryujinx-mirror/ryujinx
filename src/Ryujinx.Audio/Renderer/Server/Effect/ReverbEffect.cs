using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Server state for a reverberation effect.
    /// </summary>
    public class ReverbEffect : BaseEffect
    {
        /// <summary>
        /// The reverberation parameter.
        /// </summary>
        public ReverbParameter Parameter;

        /// <summary>
        /// The reverberation state.
        /// </summary>
        public Memory<ReverbState> State { get; }

        /// <summary>
        /// Create a new <see cref="ReverbEffect"/>.
        /// </summary>
        public ReverbEffect()
        {
            State = new ReverbState[1];
        }

        public override EffectType TargetEffectType => EffectType.Reverb;

        public override ulong GetWorkBuffer(int index)
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

            ref ReverbParameter reverbParameter = ref MemoryMarshal.Cast<byte, ReverbParameter>(parameter.SpecificData)[0];

            updateErrorInfo = new BehaviourParameter.ErrorInfo();

            if (reverbParameter.IsChannelCountMaxValid())
            {
                UpdateParameterBase(in parameter);

                UsageState oldParameterStatus = Parameter.Status;

                Parameter = reverbParameter;

                if (reverbParameter.IsChannelCountValid())
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
