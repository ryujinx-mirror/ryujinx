using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Codec.Detail
{
    interface IHardwareOpusDecoderManager : IServiceObject
    {
        Result OpenHardwareOpusDecoder(out IHardwareOpusDecoder decoder, HardwareOpusDecoderParameterInternal parameter, int workBufferHandle, int workBufferSize);
        Result GetWorkBufferSize(out int size, HardwareOpusDecoderParameterInternal parameter);
        Result OpenHardwareOpusDecoderForMultiStream(out IHardwareOpusDecoder decoder, in HardwareOpusMultiStreamDecoderParameterInternal parameter, int workBufferHandle, int workBufferSize);
        Result GetWorkBufferSizeForMultiStream(out int size, in HardwareOpusMultiStreamDecoderParameterInternal parameter);
        Result OpenHardwareOpusDecoderEx(out IHardwareOpusDecoder decoder, HardwareOpusDecoderParameterInternalEx parameter, int workBufferHandle, int workBufferSize);
        Result GetWorkBufferSizeEx(out int size, HardwareOpusDecoderParameterInternalEx parameter);
        Result OpenHardwareOpusDecoderForMultiStreamEx(out IHardwareOpusDecoder decoder, in HardwareOpusMultiStreamDecoderParameterInternalEx parameter, int workBufferHandle, int workBufferSize);
        Result GetWorkBufferSizeForMultiStreamEx(out int size, in HardwareOpusMultiStreamDecoderParameterInternalEx parameter);
        Result GetWorkBufferSizeExEx(out int size, HardwareOpusDecoderParameterInternalEx parameter);
        Result GetWorkBufferSizeForMultiStreamExEx(out int size, in HardwareOpusMultiStreamDecoderParameterInternalEx parameter);
    }
}
