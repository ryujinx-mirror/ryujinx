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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, ref EffectInParameter parameter, PoolMapper mapper)
        {
            Debug.Assert(IsTypeValid(ref parameter));

            UpdateParameterBase(ref parameter);

            Parameter = MemoryMarshal.Cast<byte, AuxiliaryBufferParameter>(parameter.SpecificData)[0];
            IsEnabled = parameter.IsEnabled;

            updateErrorInfo = new BehaviourParameter.ErrorInfo();

            if (BufferUnmapped || parameter.IsNew)
            {
                ulong bufferSize = (ulong)Unsafe.SizeOf<int>() * Parameter.BufferStorageSize + (ulong)Unsafe.SizeOf<AuxiliaryBufferHeader>() * 2;

                bool sendBufferUnmapped = !mapper.TryAttachBuffer(out updateErrorInfo, ref WorkBuffers[0], Parameter.SendBufferInfoAddress, bufferSize);
                bool returnBufferUnmapped = !mapper.TryAttachBuffer(out updateErrorInfo, ref WorkBuffers[1], Parameter.ReturnBufferInfoAddress, bufferSize);

                BufferUnmapped = sendBufferUnmapped && returnBufferUnmapped;

                if (!BufferUnmapped)
                {
                    DspAddress sendDspAddress = WorkBuffers[0].GetReference(false);
                    DspAddress returnDspAddress = WorkBuffers[1].GetReference(false);

                    State.SendBufferInfo = sendDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferHeader>();
                    State.SendBufferInfoBase = sendDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferHeader>() * 2;

                    State.ReturnBufferInfo = returnDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferHeader>();
                    State.ReturnBufferInfoBase = returnDspAddress + (uint)Unsafe.SizeOf<AuxiliaryBufferHeader>() * 2;
                }
            }
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();
        }
    }
}
