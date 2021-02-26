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
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using Ryujinx.Audio.Renderer.Utils;
using System;
using System.Diagnostics;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;

using DspAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Base class used as a server state for an effect.
    /// </summary>
    public class BaseEffect
    {
        /// <summary>
        /// The <see cref="EffectType"/> of the effect.
        /// </summary>
        public EffectType Type;

        /// <summary>
        /// Set to true if the effect must be active.
        /// </summary>
        public bool IsEnabled;

        /// <summary>
        /// Set to true if the internal effect work buffers used wasn't mapped.
        /// </summary>
        public bool BufferUnmapped;

        /// <summary>
        /// The current state of the effect.
        /// </summary>
        public UsageState UsageState;

        /// <summary>
        /// The target mix id of the effect.
        /// </summary>
        public int MixId;

        /// <summary>
        /// Position of the effect while processing effects.
        /// </summary>
        public uint ProcessingOrder;

        /// <summary>
        /// Array of all the work buffer used by the effect.
        /// </summary>
        protected AddressInfo[] WorkBuffers;

        /// <summary>
        /// Create a new <see cref="BaseEffect"/>.
        /// </summary>
        public BaseEffect()
        {
            Type = TargetEffectType;
            UsageState = UsageState.Invalid;

            IsEnabled = false;
            BufferUnmapped = false;
            MixId = Constants.UnusedMixId;
            ProcessingOrder = uint.MaxValue;

            WorkBuffers = new AddressInfo[2];

            foreach (ref AddressInfo info in WorkBuffers.AsSpan())
            {
                info = AddressInfo.Create();
            }
        }

        /// <summary>
        /// The target <see cref="EffectType"/> handled by this <see cref="BaseEffect"/>.
        /// </summary>
        public virtual EffectType TargetEffectType => EffectType.Invalid;

        /// <summary>
        /// Check if the <see cref="EffectType"/> sent by the user match the internal <see cref="EffectType"/>.
        /// </summary>
        /// <param name="parameter">The user parameter.</param>
        /// <returns>Returns true if the <see cref="EffectType"/> sent by the user matches the internal <see cref="EffectType"/>.</returns>
        public bool IsTypeValid(ref EffectInParameter parameter)
        {
            return parameter.Type == TargetEffectType;
        }

        /// <summary>
        /// Update the usage state during command generation.
        /// </summary>
        protected void UpdateUsageStateForCommandGeneration()
        {
            UsageState = IsEnabled ? UsageState.Enabled : UsageState.Disabled;
        }

        /// <summary>
        /// Update the internal common parameters from a user parameter.
        /// </summary>
        /// <param name="parameter">The user parameter.</param>
        protected void UpdateParameterBase(ref EffectInParameter parameter)
        {
            MixId = parameter.MixId;
            ProcessingOrder = parameter.ProcessingOrder;
        }

        /// <summary>
        /// Force unmap all the work buffers.
        /// </summary>
        /// <param name="mapper">The mapper to use.</param>
        public void ForceUnmapBuffers(PoolMapper mapper)
        {
            foreach (ref AddressInfo info in WorkBuffers.AsSpan())
            {
                if (info.GetReference(false) != 0)
                {
                    mapper.ForceUnmap(ref info);
                }
            }
        }

        /// <summary>
        /// Check if the effect needs to be skipped.
        /// </summary>
        /// <returns>Returns true if the effect needs to be skipped.</returns>
        public bool ShouldSkip()
        {
            return BufferUnmapped;
        }

        /// <summary>
        /// Update the <see cref="BaseEffect"/> state during command generation.
        /// </summary>
        public virtual void UpdateForCommandGeneration()
        {
            Debug.Assert(Type == TargetEffectType);
        }

        /// <summary>
        /// Update the internal state from a user parameter.
        /// </summary>
        /// <param name="updateErrorInfo">The possible <see cref="ErrorInfo"/> that was generated.</param>
        /// <param name="parameter">The user parameter.</param>
        /// <param name="mapper">The mapper to use.</param>
        public virtual void Update(out ErrorInfo updateErrorInfo, ref EffectInParameter parameter, PoolMapper mapper)
        {
            Debug.Assert(IsTypeValid(ref parameter));

            updateErrorInfo = new ErrorInfo();
        }

        /// <summary>
        /// Get the work buffer DSP address at the given index.
        /// </summary>
        /// <param name="index">The index of the work buffer</param>
        /// <returns>The work buffer DSP address at the given index.</returns>
        public virtual DspAddress GetWorkBuffer(int index)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the first work buffer DSP address.
        /// </summary>
        /// <returns>The first work buffer DSP address.</returns>
        protected DspAddress GetSingleBuffer()
        {
            if (IsEnabled)
            {
                return WorkBuffers[0].GetReference(true);
            }

            if (UsageState != UsageState.Disabled)
            {
                DspAddress address = WorkBuffers[0].GetReference(false);
                ulong size = WorkBuffers[0].Size;

                if (address != 0 && size != 0)
                {
                    AudioProcessorMemoryManager.InvalidateDataCache(address, size);
                }
            }

            return 0;
        }

        /// <summary>
        /// Store the output status to the given user output.
        /// </summary>
        /// <param name="outStatus">The given user output.</param>
        /// <param name="isAudioRendererActive">If set to true, the <see cref="AudioRenderSystem"/> is active.</param>
        public void StoreStatus(ref EffectOutStatus outStatus, bool isAudioRendererActive)
        {
            if (isAudioRendererActive)
            {
                if (UsageState == UsageState.Disabled)
                {
                    outStatus.State = EffectOutStatus.EffectState.Disabled;
                }
                else
                {
                    outStatus.State = EffectOutStatus.EffectState.Enabled;
                }
            }
            else if (UsageState == UsageState.New)
            {
                outStatus.State = EffectOutStatus.EffectState.Enabled;
            }
            else
            {
                outStatus.State = EffectOutStatus.EffectState.Disabled;
            }
        }

        /// <summary>
        /// Get the <see cref="PerformanceDetailType"/> associated to the <see cref="Type"/> of this effect.
        /// </summary>
        /// <returns>The <see cref="PerformanceDetailType"/> associated to the <see cref="Type"/> of this effect.</returns>
        public PerformanceDetailType GetPerformanceDetailType()
        {
            switch (Type)
            {
                case EffectType.BiquadFilter:
                    return PerformanceDetailType.BiquadFilter;
                case EffectType.AuxiliaryBuffer:
                    return PerformanceDetailType.Aux;
                case EffectType.Delay:
                    return PerformanceDetailType.Delay;
                case EffectType.Reverb:
                    return PerformanceDetailType.Reverb;
                case EffectType.Reverb3d:
                    return PerformanceDetailType.Reverb3d;
                case EffectType.BufferMix:
                    return PerformanceDetailType.Mix;
                default:
                    throw new NotImplementedException($"{Type}");
            }
        }
    }
}
