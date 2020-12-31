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

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, ref EffectInParameter parameter, PoolMapper mapper)
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
