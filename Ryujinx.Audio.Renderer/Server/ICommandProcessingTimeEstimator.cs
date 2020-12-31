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

using Ryujinx.Audio.Renderer.Dsp.Command;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// Estimate the time that a <see cref="ICommand"/> should take.
    /// </summary>
    /// <remarks>This is used for voice dropping.</remarks>
    public interface ICommandProcessingTimeEstimator
    {
        uint Estimate(AuxiliaryBufferCommand command);
        uint Estimate(BiquadFilterCommand command);
        uint Estimate(ClearMixBufferCommand command);
        uint Estimate(DelayCommand command);
        uint Estimate(Reverb3dCommand command);
        uint Estimate(ReverbCommand command);
        uint Estimate(DepopPrepareCommand command);
        uint Estimate(DepopForMixBuffersCommand command);
        uint Estimate(MixCommand command);
        uint Estimate(MixRampCommand command);
        uint Estimate(MixRampGroupedCommand command);
        uint Estimate(CopyMixBufferCommand command);
        uint Estimate(PerformanceCommand command);
        uint Estimate(VolumeCommand command);
        uint Estimate(VolumeRampCommand command);
        uint Estimate(PcmInt16DataSourceCommandVersion1 command);
        uint Estimate(PcmFloatDataSourceCommandVersion1 command);
        uint Estimate(AdpcmDataSourceCommandVersion1 command);
        uint Estimate(DataSourceVersion2Command command);
        uint Estimate(CircularBufferSinkCommand command);
        uint Estimate(DeviceSinkCommand command);
        uint Estimate(DownMixSurroundToStereoCommand command);
        uint Estimate(UpsampleCommand command);
    }
}
