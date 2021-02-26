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
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    public abstract class PerformanceManager
    {
        /// <summary>
        /// Get the required size for a single performance frame.
        /// </summary>
        /// <param name="parameter">The audio renderer configuration.</param>
        /// <param name="behaviourContext">The behaviour context.</param>
        /// <returns>The required size for a single performance frame.</returns>
        public static ulong GetRequiredBufferSizeForPerformanceMetricsPerFrame(ref AudioRendererConfiguration parameter, ref BehaviourContext behaviourContext)
        {
            uint version = behaviourContext.GetPerformanceMetricsDataFormat();

            if (version == 2)
            {
                return (ulong)PerformanceManagerGeneric<PerformanceFrameHeaderVersion2,
                                                 PerformanceEntryVersion2,
                                                 PerformanceDetailVersion2>.GetRequiredBufferSizeForPerformanceMetricsPerFrame(ref parameter);
            }
            else if (version == 1)
            {
                return (ulong)PerformanceManagerGeneric<PerformanceFrameHeaderVersion1,
                                                 PerformanceEntryVersion1,
                                                 PerformanceDetailVersion1>.GetRequiredBufferSizeForPerformanceMetricsPerFrame(ref parameter);
            }

            throw new NotImplementedException($"Unknown Performance metrics data format version {version}");
        }

        /// <summary>
        /// Copy the performance frame history to the supplied user buffer and returns the size copied.
        /// </summary>
        /// <param name="performanceOutput">The supplied user buffer to store the performance frame into.</param>
        /// <returns>The size copied to the supplied buffer.</returns>
        public abstract uint CopyHistories(Span<byte> performanceOutput);

        /// <summary>
        /// Set the target node id to profile.
        /// </summary>
        /// <param name="target">The target node id to profile.</param>
        public abstract void SetTargetNodeId(int target);

        /// <summary>
        /// Check if the given target node id is profiled.
        /// </summary>
        /// <param name="target">The target node id to check.</param>
        /// <returns>Return true, if the given target node id is profiled.</returns>
        public abstract bool IsTargetNodeId(int target);

        /// <summary>
        /// Get the next buffer to store a performance entry.
        /// </summary>
        /// <param name="performanceEntry">The output <see cref="PerformanceEntryAddresses"/>.</param>
        /// <param name="entryType">The <see cref="PerformanceEntryType"/> info.</param>
        /// <param name="nodeId">The node id of the entry.</param>
        /// <returns>Return true, if a valid <see cref="PerformanceEntryAddresses"/> was returned.</returns>
        public abstract bool GetNextEntry(out PerformanceEntryAddresses performanceEntry, PerformanceEntryType entryType, int nodeId);

        /// <summary>
        /// Get the next buffer to store a performance detailed entry.
        /// </summary>
        /// <param name="performanceEntry">The output <see cref="PerformanceEntryAddresses"/>.</param>
        /// <param name="detailType">The <see cref="PerformanceDetailType"/> info.</param>
        /// <param name="entryType">The <see cref="PerformanceEntryType"/> info.</param>
        /// <param name="nodeId">The node id of the entry.</param>
        /// <returns>Return true, if a valid <see cref="PerformanceEntryAddresses"/> was returned.</returns>
        public abstract bool GetNextEntry(out PerformanceEntryAddresses performanceEntry, PerformanceDetailType detailType, PerformanceEntryType entryType, int nodeId);

        /// <summary>
        /// Finalize the current performance frame.
        /// </summary>
        /// <param name="dspRunningBehind">Indicate if the DSP is running behind.</param>
        /// <param name="voiceDropCount">The count of voices that were dropped.</param>
        /// <param name="startRenderingTicks">The start ticks of the audio rendering.</param>
        public abstract void TapFrame(bool dspRunningBehind, uint voiceDropCount, ulong startRenderingTicks);

        /// <summary>
        /// Create a new <see cref="PerformanceManager"/>.
        /// </summary>
        /// <param name="performanceBuffer">The backing memory available for use by the manager.</param>
        /// <param name="parameter">The audio renderer configuration.</param>
        /// <param name="behaviourContext">The behaviour context;</param>
        /// <returns>A new <see cref="PerformanceManager"/>.</returns>
        public static PerformanceManager Create(Memory<byte> performanceBuffer, ref AudioRendererConfiguration parameter, BehaviourContext behaviourContext)
        {
            uint version = behaviourContext.GetPerformanceMetricsDataFormat();

            switch (version)
            {
                case 1:
                    return new PerformanceManagerGeneric<PerformanceFrameHeaderVersion1, PerformanceEntryVersion1, PerformanceDetailVersion1>(performanceBuffer,
                                                                                                                                              ref parameter);
                case 2:
                    return new PerformanceManagerGeneric<PerformanceFrameHeaderVersion2, PerformanceEntryVersion2, PerformanceDetailVersion2>(performanceBuffer,
                                                                                                                                              ref parameter);
                default:
                    throw new NotImplementedException($"Unknown Performance metrics data format version {version}");
            }
        }
    }
}
