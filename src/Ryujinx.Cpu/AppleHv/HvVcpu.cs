namespace Ryujinx.Cpu.AppleHv
{
    unsafe class HvVcpu
    {
        public readonly ulong Handle;
        public readonly HvVcpuExit* ExitInfo;
        public readonly IHvExecutionContext ShadowContext;
        public readonly IHvExecutionContext NativeContext;
        public readonly bool IsEphemeral;

        public HvVcpu(
            ulong handle,
            HvVcpuExit* exitInfo,
            IHvExecutionContext shadowContext,
            IHvExecutionContext nativeContext,
            bool isEphemeral)
        {
            Handle = handle;
            ExitInfo = exitInfo;
            ShadowContext = shadowContext;
            NativeContext = nativeContext;
            IsEphemeral = isEphemeral;
        }
    }
}
