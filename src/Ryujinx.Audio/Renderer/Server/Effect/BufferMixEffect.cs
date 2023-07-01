using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Server state for a buffer mix effect.
    /// </summary>
    public class BufferMixEffect : BaseEffect
    {
        /// <summary>
        /// The buffer mix parameter.
        /// </summary>
        public BufferMixParameter Parameter;

        public override EffectType TargetEffectType => EffectType.BufferMix;

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, ref EffectInParameterVersion1 parameter, PoolMapper mapper)
        {
            Update(out updateErrorInfo, ref parameter, mapper);
        }

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, ref EffectInParameterVersion2 parameter, PoolMapper mapper)
        {
            Update(out updateErrorInfo, ref parameter, mapper);
        }

        public void Update<T>(out BehaviourParameter.ErrorInfo updateErrorInfo, ref T parameter, PoolMapper mapper) where T : unmanaged, IEffectInParameter
        {
            Debug.Assert(IsTypeValid(ref parameter));

            UpdateParameterBase(ref parameter);

            Parameter = MemoryMarshal.Cast<byte, BufferMixParameter>(parameter.SpecificData)[0];
            IsEnabled = parameter.IsEnabled;

            updateErrorInfo = new BehaviourParameter.ErrorInfo();
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();
        }
    }
}
