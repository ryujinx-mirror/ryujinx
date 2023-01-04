using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf.Cmif;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Sdk
{
    static class ServiceUtil
    {
        public static Result SendRequest(out CmifResponse response, int sessionHandle, uint requestId, bool sendPid, scoped ReadOnlySpan<byte> data)
        {
            ulong tlsAddress = HorizonStatic.ThreadContext.TlsAddress;
            int tlsSize = Api.TlsMessageBufferSize;

            using (var tlsRegion = HorizonStatic.AddressSpace.GetWritableRegion(tlsAddress, tlsSize))
            {
                CmifRequest request = CmifMessage.CreateRequest(tlsRegion.Memory.Span, new CmifRequestFormat()
                {
                    DataSize = data.Length,
                    RequestId = requestId,
                    SendPid = sendPid
                });

                data.CopyTo(request.Data);
            }

            Result result = HorizonStatic.Syscall.SendSyncRequest(sessionHandle);

            if (result.IsFailure)
            {
                response = default;
                return result;
            }

            return CmifMessage.ParseResponse(out response, HorizonStatic.AddressSpace.GetWritableRegion(tlsAddress, tlsSize).Memory.Span, false, 0);
        }
    }
}
