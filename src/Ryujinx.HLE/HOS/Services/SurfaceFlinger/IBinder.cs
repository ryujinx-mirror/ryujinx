using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    interface IBinder
    {
        ResultCode AdjustRefcount(int addVal, int type);

        void GetNativeHandle(uint typeId, out KReadableEvent readableEvent);

        ResultCode OnTransact(uint code, uint flags, ReadOnlySpan<byte> inputParcel, Span<byte> outputParcel)
        {
            using Parcel inputParcelReader = new(inputParcel);

            // TODO: support objects?
            using Parcel outputParcelWriter = new((uint)(outputParcel.Length - Unsafe.SizeOf<ParcelHeader>()), 0);

            string inputInterfaceToken = inputParcelReader.ReadInterfaceToken();

            if (!InterfaceToken.Equals(inputInterfaceToken))
            {
                Logger.Error?.Print(LogClass.SurfaceFlinger, $"Invalid interface token {inputInterfaceToken} (expected: {InterfaceToken}");

                return ResultCode.Success;
            }

            OnTransact(code, flags, inputParcelReader, outputParcelWriter);

            outputParcelWriter.Finish().CopyTo(outputParcel);

            return ResultCode.Success;
        }

        void OnTransact(uint code, uint flags, Parcel inputParcel, Parcel outputParcel);

        string InterfaceToken { get; }
    }
}
