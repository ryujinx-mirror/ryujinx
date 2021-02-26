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

using Ryujinx.Audio.Renderer.Server.Performance;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class PerformanceCommand : ICommand
    {
        public enum Type
        {
            Invalid,
            Start,
            End
        }

        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.Performance;

        public ulong EstimatedProcessingTime { get; set; }

        public PerformanceEntryAddresses PerformanceEntryAddresses { get; }

        public Type PerformanceType { get; set; }

        public PerformanceCommand(ref PerformanceEntryAddresses performanceEntryAddresses, Type performanceType, int nodeId)
        {
            Enabled = true;
            PerformanceEntryAddresses = performanceEntryAddresses;
            PerformanceType = performanceType;
            NodeId = nodeId;
        }

        public void Process(CommandList context)
        {
            if (PerformanceType == Type.Start)
            {
                PerformanceEntryAddresses.SetStartTime(context.GetTimeElapsedSinceDspStartedProcessing());
            }
            else if (PerformanceType == Type.End)
            {
                PerformanceEntryAddresses.SetProcessingTime(context.GetTimeElapsedSinceDspStartedProcessing());
                PerformanceEntryAddresses.IncrementEntryCount();
            }
        }
    }
}
