namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:al")] // 9.0.0+
    class IAlarmService : IpcService
    {
        public IAlarmService(ServiceCtx context) { }
    }
}
