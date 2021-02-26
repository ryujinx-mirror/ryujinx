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
using System.Diagnostics;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;

namespace Ryujinx.Audio.Renderer.Server.Sink
{
    /// <summary>
    /// Base class used for server information of a sink.
    /// </summary>
    public class BaseSink
    {
        /// <summary>
        /// The type of this <see cref="BaseSink"/>.
        /// </summary>
        public SinkType Type;

        /// <summary>
        /// Set to true if the sink is used.
        /// </summary>
        public bool IsUsed;

        /// <summary>
        /// Set to true if the sink need to be skipped because of invalid state.
        /// </summary>
        public bool ShouldSkip;

        /// <summary>
        /// The node id of the sink.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// Create a new <see cref="BaseSink"/>.
        /// </summary>
        public BaseSink()
        {
            CleanUp();
        }

        /// <summary>
        /// Clean up the internal state of the <see cref="BaseSink"/>.
        /// </summary>
        public virtual void CleanUp()
        {
            Type = TargetSinkType;
            IsUsed = false;
            ShouldSkip = false;
        }

        /// <summary>
        /// The target <see cref="SinkType"/> handled by this <see cref="BaseSink"/>.
        /// </summary>
        public virtual SinkType TargetSinkType => SinkType.Invalid;

        /// <summary>
        /// Check if the <see cref="SinkType"/> sent by the user match the internal <see cref="SinkType"/>.
        /// </summary>
        /// <param name="parameter">The user parameter.</param>
        /// <returns>Return true, if the <see cref="SinkType"/> sent by the user match the internal <see cref="SinkType"/>.</returns>
        public bool IsTypeValid(ref SinkInParameter parameter)
        {
            return parameter.Type == TargetSinkType;
        }

        /// <summary>
        /// Update the <see cref="BaseSink"/> state during command generation.
        /// </summary>
        public virtual void UpdateForCommandGeneration()
        {
            Debug.Assert(Type == TargetSinkType);
        }

        /// <summary>
        /// Update the internal common parameters from user parameter.
        /// </summary>
        /// <param name="parameter">The user parameter.</param>
        protected void UpdateStandardParameter(ref SinkInParameter parameter)
        {
            if (IsUsed != parameter.IsUsed)
            {
                IsUsed = parameter.IsUsed;
                NodeId = parameter.NodeId;
            }
        }

        /// <summary>
        /// Update the internal state from user parameter.
        /// </summary>
        /// <param name="errorInfo">The possible <see cref="ErrorInfo"/> that was generated.</param>
        /// <param name="parameter">The user parameter.</param>
        /// <param name="outStatus">The user output status.</param>
        /// <param name="mapper">The mapper to use.</param>
        public virtual void Update(out ErrorInfo errorInfo, ref SinkInParameter parameter, ref SinkOutStatus outStatus, PoolMapper mapper)
        {
            Debug.Assert(IsTypeValid(ref parameter));

            errorInfo = new ErrorInfo();
        }
    }
}
