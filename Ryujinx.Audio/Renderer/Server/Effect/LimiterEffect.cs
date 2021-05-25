//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

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
    /// Server state for a limiter effect.
    /// </summary>
    public class LimiterEffect : BaseEffect
    {
        /// <summary>
        /// The limiter parameter.
        /// </summary>
        public LimiterParameter Parameter;

        /// <summary>
        /// The limiter state.
        /// </summary>
        public Memory<LimiterState> State { get; }

        /// <summary>
        /// Create a new <see cref="LimiterEffect"/>.
        /// </summary>
        public LimiterEffect()
        {
            State = new LimiterState[1];
        }

        public override EffectType TargetEffectType => EffectType.Limiter;

        public override ulong GetWorkBuffer(int index)
        {
            return GetSingleBuffer();
        }

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

            ref LimiterParameter limiterParameter = ref MemoryMarshal.Cast<byte, LimiterParameter>(parameter.SpecificData)[0];

            updateErrorInfo = new BehaviourParameter.ErrorInfo();

            UpdateParameterBase(ref parameter);

            Parameter = limiterParameter;

            IsEnabled = parameter.IsEnabled;

            if (BufferUnmapped || parameter.IsNew)
            {
                UsageState = UsageState.New;
                Parameter.Status = UsageState.Invalid;

                BufferUnmapped = !mapper.TryAttachBuffer(out updateErrorInfo, ref WorkBuffers[0], parameter.BufferBase, parameter.BufferSize);
            }
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();

            Parameter.Status = UsageState.Enabled;
            Parameter.StatisticsReset = false;
        }

        public override void InitializeResultState(ref EffectResultState state)
        {
            ref LimiterStatistics statistics = ref MemoryMarshal.Cast<byte, LimiterStatistics>(state.SpecificData)[0];

            statistics.Reset();
        }

        public override void UpdateResultState(ref EffectResultState destState, ref EffectResultState srcState)
        {
            destState = srcState;
        }
    }
}
