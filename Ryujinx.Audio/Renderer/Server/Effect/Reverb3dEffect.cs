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

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, ref EffectInParameter parameter, PoolMapper mapper)
        {
            Debug.Assert(IsTypeValid(ref parameter));

            ref Reverb3dParameter reverbParameter = ref MemoryMarshal.Cast<byte, Reverb3dParameter>(parameter.SpecificData)[0];

            updateErrorInfo = new BehaviourParameter.ErrorInfo();

            if (reverbParameter.IsChannelCountMaxValid())
            {
                UpdateParameterBase(ref parameter);

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
