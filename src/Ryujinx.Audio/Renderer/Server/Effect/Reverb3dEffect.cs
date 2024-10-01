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
    /// Server state for a 3D reverberation effect.
    /// </summary>
    public class Reverb3dEffect : BaseEffect
    {
        /// <summary>
        /// The 3D reverberation parameter.
        /// </summary>
        public Reverb3dParameter Parameter;

        /// <summary>
        /// The 3D reverberation state.
        /// </summary>
        public Memory<Reverb3dState> State { get; }

        public Reverb3dEffect()
        {
            State = new Reverb3dState[1];
        }

        public override EffectType TargetEffectType => EffectType.Reverb3d;

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

            ref Reverb3dParameter reverbParameter = ref MemoryMarshal.Cast<byte, Reverb3dParameter>(parameter.SpecificData)[0];

            updateErrorInfo = new BehaviourParameter.ErrorInfo();

            if (reverbParameter.IsChannelCountMaxValid())
            {
                UpdateParameterBase(in parameter);

                UsageState oldParameterStatus = Parameter.ParameterStatus;

                Parameter = reverbParameter;

                if (reverbParameter.IsChannelCountValid())
                {
                    IsEnabled = parameter.IsEnabled;

                    if (oldParameterStatus != UsageState.Enabled)
                    {
                        Parameter.ParameterStatus = oldParameterStatus;
                    }

                    if (BufferUnmapped || parameter.IsNew)
                    {
                        UsageState = UsageState.New;
                        Parameter.ParameterStatus = UsageState.Invalid;

                        BufferUnmapped = !mapper.TryAttachBuffer(out updateErrorInfo, ref WorkBuffers[0], parameter.BufferBase, parameter.BufferSize);
                    }
                }
            }
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();

            Parameter.ParameterStatus = UsageState.Enabled;
        }
    }
}
