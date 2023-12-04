using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Sf;
using System;
using ApplicationId = Ryujinx.Horizon.Sdk.Ncm.ApplicationId;

namespace Ryujinx.Horizon.Sdk.Prepo
{
    interface IPrepoService : IServiceObject
    {
        Result SaveReport(ReadOnlySpan<byte> gameRoomBuffer, ReadOnlySpan<byte> reportBuffer, ulong pid);
        Result SaveReportWithUser(Uid userId, ReadOnlySpan<byte> gameRoomBuffer, ReadOnlySpan<byte> reportBuffer, ulong pid);
        Result RequestImmediateTransmission();
        Result GetTransmissionStatus(out int status);
        Result GetSystemSessionId(out ulong systemSessionId);
        Result SaveSystemReport(ReadOnlySpan<byte> gameRoomBuffer, ApplicationId applicationId, ReadOnlySpan<byte> reportBuffer);
        Result SaveSystemReportWithUser(Uid userId, ReadOnlySpan<byte> gameRoomBuffer, ApplicationId applicationId, ReadOnlySpan<byte> reportBuffer);
        Result IsUserAgreementCheckEnabled(out bool enabled);
        Result SetUserAgreementCheckEnabled(bool enabled);
    }
}
