using Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService;

namespace Ryujinx.HLE.HOS.Services.Sdb.Pdm
{
    [Service("pdm:qry")]
    class IQueryService : IpcService
    {
        public IQueryService(ServiceCtx context) { }

        [CommandCmif(13)] // 5.0.0+
        // QueryApplicationPlayStatisticsForSystem(buffer<bytes, 5> title_id_list) -> (buffer<bytes, 6> entries, s32 entries_count)
        public ResultCode QueryApplicationPlayStatisticsForSystem(ServiceCtx context)
        {
            return QueryPlayStatisticsManager.GetPlayStatistics(context);
        }

        [CommandCmif(16)] // 6.0.0+
        // QueryApplicationPlayStatisticsByUserAccountIdForSystem(nn::account::Uid, buffer<bytes, 5> title_id_list) -> (buffer<bytes, 6> entries, s32 entries_count)
        public ResultCode QueryApplicationPlayStatisticsByUserAccountIdForSystem(ServiceCtx context)
        {
            return QueryPlayStatisticsManager.GetPlayStatistics(context, true);
        }
    }
}
