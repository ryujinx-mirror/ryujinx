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
        uint Estimate(LimiterCommandVersion1 command);
        uint Estimate(LimiterCommandVersion2 command);
        uint Estimate(MultiTapBiquadFilterCommand command);
        uint Estimate(CaptureBufferCommand command);
        uint Estimate(CompressorCommand command);
        uint Estimate(BiquadFilterAndMixCommand command);
        uint Estimate(MultiTapBiquadFilterAndMixCommand command);
    }
}
