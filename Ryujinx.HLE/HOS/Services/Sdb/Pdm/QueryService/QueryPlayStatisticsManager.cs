using Ryujinx.Common;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService
{
    static class QueryPlayStatisticsManager
    {
        private static Dictionary<UserId, ApplicationPlayStatistics> applicationPlayStatistics = new Dictionary<UserId, ApplicationPlayStatistics>();

        internal static ResultCode GetPlayStatistics(ServiceCtx context, bool byUserId = false)
        {
            ref readonly var controlProperty = ref context.Device.Application.ControlData.Value;

            ulong inputPosition = context.Request.SendBuff[0].Position;
            ulong inputSize     = context.Request.SendBuff[0].Size;

            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize     = context.Request.ReceiveBuff[0].Size;

            UserId userId = byUserId ? context.RequestData.ReadStruct<UserId>() : new UserId();

            if (byUserId)
            {
                if (!context.Device.System.AccountManager.TryGetUser(userId, out _))
                {
                    return ResultCode.UserNotFound;
                }
            }

            PlayLogQueryCapability queryCapability = (PlayLogQueryCapability)controlProperty.PlayLogQueryCapability;

            List<ulong> titleIds = new List<ulong>();

            for (ulong i = 0; i < inputSize / sizeof(ulong); i++)
            {
                titleIds.Add(context.Memory.Read<ulong>(inputPosition));
            }

            if (queryCapability == PlayLogQueryCapability.WhiteList)
            {
                // Check if input title ids are in the whitelist.
                foreach (ulong titleId in titleIds)
                {
                    if (!controlProperty.PlayLogQueryableApplicationId.ItemsRo.Contains(titleId))
                    {
                        return (ResultCode)Am.ResultCode.ObjectInvalid;
                    }
                }
            }

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            // Return ResultCode.ServiceUnavailable if data is locked by another process.
            var filteredApplicationPlayStatistics = applicationPlayStatistics.AsEnumerable();

            if (queryCapability == PlayLogQueryCapability.None)
            {
                filteredApplicationPlayStatistics = filteredApplicationPlayStatistics.Where(kv => kv.Value.TitleId == context.Process.TitleId);
            }
            else // PlayLogQueryCapability.All
            {
                filteredApplicationPlayStatistics = filteredApplicationPlayStatistics.Where(kv => titleIds.Contains(kv.Value.TitleId));
            }

            if (byUserId)
            {
                filteredApplicationPlayStatistics = filteredApplicationPlayStatistics.Where(kv => kv.Key == userId);
            }

            for (int i = 0; i < filteredApplicationPlayStatistics.Count(); i++)
            {
                MemoryHelper.Write(context.Memory, outputPosition + (ulong)(i * Marshal.SizeOf<ApplicationPlayStatistics>()), filteredApplicationPlayStatistics.ElementAt(i).Value);
            }

            context.ResponseData.Write(filteredApplicationPlayStatistics.Count());

            return ResultCode.Success;
        }
    }
}